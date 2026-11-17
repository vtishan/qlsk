using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Infrastructure.PayOS;
using Xunit;

namespace QLSK.Tests;

public sealed class SecurityIntegrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Logout_revokes_the_bearer_on_public_and_private_actions()
    {
        var scenario = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var client = factory.CreateAuthenticatedClient(scenario.UserId, "NguoiDung");
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/api/auth/logout", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/accounts/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/api/events/{scenario.EventId}")).StatusCode);
    }

    [Fact]
    public async Task Public_event_hides_account_and_contract_even_inside_ticket_type_navigation()
    {
        var scenario = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/events/{scenario.EventId}");
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        foreach (var field in new[] { "email", "matKhau", "tokenVersion", "tienCoc", "tongTien", "giayTo", "maQR", "thanhToans" })
            Assert.DoesNotContain(field, json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("loaiVes", json);
        Assert.Contains("diaDiem", json);
    }

    [Theory]
    [InlineData("/api/orders/{0}", false)]
    [InlineData("/api/tickets/{1}", false)]
    [InlineData("/api/tickets/{1}/exports", false)]
    [InlineData("/api/tickets/{1}/export", true)]
    [InlineData("/api/tickets/{1}/email", true)]
    public async Task Another_buyer_cannot_read_or_export_tickets(string route, bool post)
    {
        var scenario = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        var other = await factory.ThucThiVoiCoSoDuLieuKiemThu(db => ApiTestData.AddUserAsync(db, 1));
        using var client = factory.CreateAuthenticatedClient(other, "NguoiDung");
        var path = string.Format(route, scenario.OrderId, scenario.TicketId);
        var response = post ? await client.PostAsync(path, null) : await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("maQR", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Concurrent_same_key_orders_reserve_once_and_reject_changed_payload()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var client = factory.CreateAuthenticatedClient(s.UserId, "NguoiDung");
        var key = Guid.NewGuid().ToString("N");
        async Task<HttpResponseMessage> Send(int quantity)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
            {
                Content = JsonContent.Create(new { items = new[] { new { loaiVeId = s.TicketTypeId, soLuong = quantity } } })
            };
            request.Headers.Add("Idempotency-Key", key);
            return await client.SendAsync(request);
        }
        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Send(2)));
        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        var ids = new List<int>();
        foreach (var response in responses)
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            ids.Add(json.RootElement.GetProperty("data").GetProperty("id").GetInt32());
        }
        Assert.Single(ids.Distinct());
        Assert.Equal(HttpStatusCode.Conflict, (await Send(3)).StatusCode);
        await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
        {
            Assert.Equal(1, await db.DanhSachDonVe.CountAsync(o => o.TaiKhoanId == s.UserId && o.IdempotencyKey == key));
            Assert.Equal(3, (await db.DanhSachLoaiVe.FindAsync(s.TicketTypeId))!.SoLuongDangGiu);
        });
    }

    [Fact]
    public async Task Last_two_tickets_under_sixteen_parallel_buyers_never_oversell()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        var users = new List<int>();
        await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
        {
            (await db.DanhSachLoaiVe.FindAsync(s.TicketTypeId))!.SoLuong = 3;
            (await db.DanhSachSuKien.FindAsync(s.EventId))!.SoNguoi = 3;
            await db.SaveChangesAsync();
            for (var i = 0; i < 16; i++) users.Add(await ApiTestData.AddUserAsync(db, 1));
        });
        var clients = users.Select(id => factory.CreateAuthenticatedClient(id, "NguoiDung")).ToArray();
        try
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var responses = await Task.WhenAll(clients.Select(client => client.PostAsJsonAsync("/api/orders",
                new { items = new[] { new { loaiVeId = s.TicketTypeId, soLuong = 1 } } })));
            Assert.Equal(2, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
            Assert.All(responses, r => Assert.Contains(r.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }));
            await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
            {
                var type = (await db.DanhSachLoaiVe.FindAsync(s.TicketTypeId))!;
                Assert.Equal(3, type.SoLuongDangGiu);
                Assert.Equal(3, await db.DanhSachDonVe.CountAsync(o => o.SuKienId == s.EventId));
                Assert.True(type.SoLuongDaBan + type.SoLuongDangGiu <= type.SoLuong);
            });
            Assert.True(timer.Elapsed < TimeSpan.FromSeconds(30), "The local contention test exceeded the request budget.");
        }
        finally { foreach (var client in clients) client.Dispose(); }
    }

    [Fact]
    public async Task Event_capacity_is_shared_by_different_ticket_types()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        var secondId = await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
        {
            (await db.DanhSachSuKien.FindAsync(s.EventId))!.SoNguoi = 1;
            var type = new LoaiVe { SuKienId = s.EventId, TenLoaiVe = "Second", SoLuong = 100, GiaVe = 50000,
                ThoiGianMoBan = DateTime.UtcNow.AddDays(-1), ThoiGianDongBan = DateTime.UtcNow.AddDays(1) };
            db.Add(type); await db.SaveChangesAsync(); return type.LoaiVeId;
        });
        using var client = factory.CreateAuthenticatedClient(s.UserId, "NguoiDung");
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/orders",
            new { items = new[] { new { loaiVeId = secondId, soLuong = 1 } } })).StatusCode);
    }

    [Fact]
    public async Task Payment_burst_reuses_one_gateway_record_and_webhook_replay_issues_once()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var client = factory.CreateAuthenticatedClient(s.UserId, "NguoiDung");
        using var created = await client.PostAsJsonAsync("/api/orders", new { items = new[] { new { loaiVeId = s.TicketTypeId, soLuong = 2 } } });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        using var json = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var orderId = json.RootElement.GetProperty("data").GetProperty("id").GetInt32();
        var payments = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => client.PostAsJsonAsync("/api/payments", new { donVeId = orderId })));
        Assert.Contains(payments, p => p.StatusCode == HttpStatusCode.OK);
        Assert.All(payments, p => Assert.Contains(p.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }));
        Assert.Equal(1, await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.DanhSachThanhToan.CountAsync(p => p.DonVeId == orderId)));
        var webhook = SignedWebhook(orderId, 200000, "test-payment-" + orderId);
        using var gateway = factory.CreateClient();
        var callbacks = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => gateway.PostAsJsonAsync("/api/payments/webhook", webhook)));
        Assert.All(callbacks, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
        {
            Assert.Equal(2, await db.DanhSachVe.CountAsync(t => t.DonVeId == orderId));
            Assert.Equal(TrangThaiDon.ThanhCong, (await db.DanhSachDonVe.FindAsync(orderId))!.TrangThai);
            Assert.Equal(2, (await db.DanhSachLoaiVe.FindAsync(s.TicketTypeId))!.SoLuongDaBan);
        });
    }

    [Fact]
    public async Task Refund_receipts_are_idempotent_even_after_another_partial_refund()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory, trangThaiDonVe: TrangThaiDon.ThanhCong);
        using var client = factory.CreateAuthenticatedClient(s.ManagerId, "QuanLySuKien");
        var a = Guid.NewGuid().ToString("N"); var b = Guid.NewGuid().ToString("N");
        var route = $"/api/payments/{s.PaymentId}/refund";
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(route, new { soTien = 10000, maHoanTien = a })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(route, new { soTien = 20000, maHoanTien = b })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(route, new { soTien = 10000, maHoanTien = a })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync(route, new { soTien = 20000, maHoanTien = a })).StatusCode);
        await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
        {
            Assert.Equal(30000, (await db.DanhSachThanhToan.FindAsync(s.PaymentId))!.SoTienHoan);
            Assert.Equal(2, await db.DanhSachHoanTien.CountAsync(r => r.ThanhToanId == s.PaymentId));
        });
    }

    [Fact]
    public async Task Concurrent_check_ins_allow_one_admission()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory, TrangThaiSuKien.DangDienRa, TrangThaiDon.ThanhCong);
        var qr = await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.DanhSachVe.Where(t => t.VeId == s.TicketId).Select(t => t.NoiDungMaQr).SingleAsync());
        using var client = factory.CreateAuthenticatedClient(s.ManagerId, "QuanLySuKien");
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => client.PostAsJsonAsync("/api/check-in", new { ma = qr })));
        Assert.Single(results, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Equal(7, results.Count(r => r.StatusCode == HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task Event_list_has_bounded_pages_summary_metadata_and_one_query()
    {
        for (var i = 0; i < 3; i++) await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var client = factory.CreateClient();
        factory.QueryCounter.Reset();
        var response = await client.GetAsync("/api/events?size=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.True(json.RootElement.GetProperty("data")[0].GetProperty("metadataComplete").GetBoolean());
        Assert.Equal("true", response.Headers.GetValues("X-Has-More").Single());
        Assert.Equal(1, factory.QueryCounter.Count);
        Assert.Contains("OFFSET", factory.QueryCounter.LastCommand);
    }

    [Fact]
    public async Task Public_catalog_cache_is_invalidated_by_a_real_admin_write()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var anon = factory.CreateClient();
        using var admin = factory.CreateAuthenticatedClient(s.AdminId, "QuanTriVien");
        var url = "/api/venues?size=100&verification=" + Guid.NewGuid().ToString("N");
        factory.QueryCounter.Reset();
        await anon.GetStringAsync(url);
        var count = factory.QueryCounter.Count;
        await anon.GetStringAsync(url);
        Assert.Equal(count, factory.QueryCounter.Count);
        var name = "Cache invalidated " + Guid.NewGuid().ToString("N");
        var update = await admin.PutAsJsonAsync($"/api/venues/{s.VenueId}", new { ten = name, diaChi = "Test venue", sucChua = 1000, giaThue = 1000000, hoatDong = true });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var afterWrite = factory.QueryCounter.Count;
        Assert.Contains(name, await anon.GetStringAsync(url));
        Assert.True(factory.QueryCounter.Count > afterWrite);
    }

    [Fact]
    public async Task A_pending_order_limit_stops_stock_hoarding()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var client = factory.CreateAuthenticatedClient(s.UserId, "NguoiDung");
        var body = new { items = new[] { new { loaiVeId = s.TicketTypeId, soLuong = 1 } } };
        for (var i = 0; i < 4; i++) Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/orders", body)).StatusCode);
        var rejected = await client.PostAsJsonAsync("/api/orders", body);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(rejected.Headers.RetryAfter);
        Assert.Equal(5, await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.DanhSachDonVe.CountAsync(o => o.TaiKhoanId == s.UserId)));
    }

    [Fact]
    public async Task Statistics_count_all_records_independently_of_list_page_size()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        await ApiTestData.TaoKichBanKiemThuAsync(factory);
        await factory.ThucThiVoiCoSoDuLieuKiemThu(db => ApiTestData.AddPendingVerificationAsync(db));
        var bookings = await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.DanhSachDatDiaDiem.CountAsync());
        var verifications = await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.YeuCauXacThucToChuc.CountAsync());
        using var client = factory.CreateAuthenticatedClient(s.ManagerId, "QuanLySuKien");
        using var bookingJson = JsonDocument.Parse(await client.GetStringAsync("/api/venue-bookings/statistics?size=1"));
        var groups = bookingJson.RootElement.GetProperty("data").GetProperty("theoTrangThai");
        Assert.Equal(bookings, groups.EnumerateObject().Sum(g => g.Value.GetProperty("soLuong").GetInt32()));
        using var verificationJson = JsonDocument.Parse(await client.GetStringAsync("/api/organizer-verifications/statistics?size=1"));
        Assert.Equal(verifications, verificationJson.RootElement.GetProperty("data").EnumerateArray().Sum(g => g.GetProperty("soLuong").GetInt32()));
    }

    [Theory]
    [InlineData("/api/venue-bookings/statistics")]
    [InlineData("/api/organizer-verifications/statistics")]
    public async Task Statistics_require_staff_privileges(string route)
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var anonymous = factory.CreateClient();
        using var buyer = factory.CreateAuthenticatedClient(s.UserId, "NguoiDung");
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(route)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await buyer.GetAsync(route)).StatusCode);
    }

    [Fact]
    public async Task Sql_payload_remains_a_search_value_and_does_not_modify_accounts()
    {
        var before = await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.DanhSachTaiKhoan.CountAsync());
        using var client = factory.CreateClient();
        const string injection = "'; DROP TABLE TaiKhoan;--";
        var response = await client.GetAsync("/api/events?q=" + Uri.EscapeDataString(injection));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(injection, factory.QueryCounter.LastCommand);
        Assert.Equal(before, await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.DanhSachTaiKhoan.CountAsync()));
    }

    [Theory]
    [InlineData("page=-1")]
    [InlineData("page=garbage")]
    [InlineData("size=0")]
    [InlineData("size=100&page=1&page=2")]
    public async Task Invalid_pagination_is_rejected_without_a_server_error(string query)
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/events?" + query)).StatusCode);
    }

    internal static PayOsWebhook SignedWebhook(int orderId, decimal amount, string link)
    {
        var data = JsonSerializer.SerializeToElement(new { orderCode = orderId, amount, currency = "VND", reference = Guid.NewGuid().ToString("N"), paymentLinkId = link, code = "00", desc = "success" });
        var text = string.Join("&", data.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => $"{p.Name}={p.Value}"));
        return new PayOsWebhook { MaKetQua = "00", ThanhCong = true, DuLieuWebhook = data,
            ChuKyWebhook = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("test-checksum-key"), Encoding.UTF8.GetBytes(text))).ToLowerInvariant() };
    }
}
