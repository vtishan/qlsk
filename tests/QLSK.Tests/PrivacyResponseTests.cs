using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Enums;
using Xunit;

namespace QLSK.Tests;

public sealed class PrivacyResponseTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string[] InternalFields = ["taiKhoanId", "banToChucId", "rowVersion",
        "ngayXacThuc", "email", "hoTen", "vaiTro", "vaiTroId", "matKhau", "matKhauBam",
        "tokenVersion", "giayTo", "nguoiXuLyId", "idempotencyKey", "checkoutUrl", "linkCreationStartedAt"];

    private HttpClient Client(TestScenario s, int role) => role switch
    {
        0 => factory.CreateClient(),
        1 => factory.CreateAuthenticatedClient(s.UserId, "NguoiDung"),
        2 => factory.CreateAuthenticatedClient(s.OrganizerUserId, "BanToChuc"),
        3 => factory.CreateAuthenticatedClient(s.ManagerId, "QuanLySuKien"),
        _ => factory.CreateAuthenticatedClient(s.AdminId, "QuanTriVien")
    };

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public async Task Public_list_and_detail_have_the_same_safe_schema_for_every_role(int role)
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        var canary = "PRIVATE-" + Guid.NewGuid().ToString("N");
        var publicName = "Public organizer " + Guid.NewGuid().ToString("N");
        await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
        {
            var owner = await db.DanhSachTaiKhoan.Include(a => a.BanToChuc).SingleAsync(a => a.TaiKhoanId == s.OrganizerUserId);
            owner.HoTen = canary;
            owner.BanToChuc!.TenBanToChuc = publicName;
            owner.BanToChuc.MoTaBanToChuc = canary;
            owner.BanToChuc.NgayXacThuc = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);
            await db.SaveChangesAsync();
        });
        using var client = Client(s, role);
        foreach (var detail in new[] { false, true })
        {
            var path = detail ? $"/api/events/{s.EventId}" : $"/api/events?q={Uri.EscapeDataString(publicName)}&size=1";
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain(canary, text);
            Assert.DoesNotContain("0001-01-01", text);
            using var json = JsonDocument.Parse(text);
            var data = json.RootElement.GetProperty("data");
            var e = detail ? data : Assert.Single(data.EnumerateArray());
            var eventFields = new[] { "id", "loaiSuKienId", "ten", "moTa", "hinhAnh", "giaThapNhat", "metadataComplete",
                "batDau", "ketThuc", "soNguoi", "trangThai", "banToChuc", "loaiSuKien", "datDiaDiem" };
            Fields(e, detail ? eventFields.Concat(["loaiVes"]).ToArray() : eventFields);
            Assert.Equal(s.EventId, e.GetProperty("id").GetInt32());
            RejectFields(e, InternalFields.Concat(["ngayTao", "tienCoc", "tongTien", "suKien", "hoatDong"]).ToArray());
            Fields(e.GetProperty("banToChuc"), "ten");
            Assert.Equal(publicName, e.GetProperty("banToChuc").GetProperty("ten").GetString());
            Fields(e.GetProperty("loaiSuKien"), "id", "ten", "moTa");
            Fields(e.GetProperty("datDiaDiem"), "diaDiem");
            Fields(e.GetProperty("datDiaDiem").GetProperty("diaDiem"), "id", "ten", "diaChi");
            Assert.Equal(100000, e.GetProperty("giaThapNhat").GetDecimal());
            Assert.True(e.GetProperty("metadataComplete").GetBoolean());
            Assert.Equal(detail, e.TryGetProperty("loaiVes", out var types));
            if (detail) Fields(Assert.Single(types.EnumerateArray()), "id", "suKienId", "ten", "gia", "soLuong", "daBan", "dangGiu", "moBan", "dongBan");
        }
    }

    [Fact]
    public async Task Staff_collections_reject_anonymous_buyers_and_unrelated_organizers()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        foreach (var role in new[] { 0, 1, 2 })
        {
            using var client = Client(s, role);
            foreach (var path in new[] { "/api/accounts", "/api/admin/catalog", "/api/admin/dashboard", "/api/events/manage",
                "/api/organizer-verifications", "/api/venue-bookings", "/api/venues/all", "/api/event-types/all" })
            {
                using var response = await client.GetAsync(path);
                Assert.Equal(role == 0 ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden, response.StatusCode);
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                RejectFields(json.RootElement, InternalFields);
                Assert.Null(json.RootElement.GetProperty("data").Deserialize<object>());
            }
        }
    }

    [Fact]
    public async Task Events_without_a_venue_or_ticket_types_do_not_fabricate_entity_defaults()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        var name = "No-metadata-" + Guid.NewGuid().ToString("N");
        var id = await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
        {
            var source = (await db.DanhSachSuKien.FindAsync(s.EventId))!;
            var e = new QLSK.Domain.Entities.SuKien { BanToChucId = source.BanToChucId, LoaiSuKienId = s.CategoryId,
                TenSuKien = name, TrangThai = TrangThaiSuKien.DaDuyet, ThoiGianBatDau = DateTime.UtcNow.AddDays(1),
                ThoiGianKetThuc = DateTime.UtcNow.AddDays(2), SoNguoi = 100 };
            db.Add(e); await db.SaveChangesAsync(); return e.SuKienId;
        });
        using var client = factory.CreateClient();
        foreach (var path in new[] { $"/api/events?q={name}", $"/api/events/{id}" })
        {
            using var json = JsonDocument.Parse(await client.GetStringAsync(path));
            var data = json.RootElement.GetProperty("data");
            var e = data.ValueKind == JsonValueKind.Array ? Assert.Single(data.EnumerateArray()) : data;
            Assert.Equal(JsonValueKind.Null, e.GetProperty("giaThapNhat").ValueKind);
            Assert.Equal(JsonValueKind.Null, e.GetProperty("datDiaDiem").ValueKind);
            RejectFields(e, InternalFields.Concat(["ngayTao"]).ToArray());
            if (e.TryGetProperty("loaiVes", out var types)) Assert.Empty(types.EnumerateArray());
        }
    }

    [Fact]
    public async Task Public_catalogs_serialize_only_advertised_fields()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        using var client = factory.CreateClient();
        foreach (var (path, fields) in new (string, string[])[]
        {
            ("/api/venues?size=100", ["id", "ten", "diaChi", "hinhAnh", "sucChua", "giaThue"]),
            ("/api/event-types?size=100", ["id", "ten", "moTa"]),
            ($"/api/events/{s.EventId}/ticket-types", ["id", "suKienId", "ten", "gia", "soLuong", "daBan", "dangGiu", "moBan", "dongBan"])
        })
        {
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var rows = json.RootElement.GetProperty("data");
            Assert.NotEmpty(rows.EnumerateArray());
            foreach (var row in rows.EnumerateArray()) Fields(row, fields);
            RejectFields(rows, InternalFields);
        }
    }

    [Theory]
    [InlineData(0, HttpStatusCode.Unauthorized)]
    [InlineData(1, HttpStatusCode.Forbidden)]
    [InlineData(2, HttpStatusCode.OK)]
    [InlineData(3, HttpStatusCode.OK)]
    [InlineData(4, HttpStatusCode.OK)]
    [InlineData(5, HttpStatusCode.Forbidden)]
    public async Task Managed_detail_requires_staff_or_the_owning_organizer(int role, HttpStatusCode expected)
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory);
        var otherOrganizer = role == 5 ? await factory.ThucThiVoiCoSoDuLieuKiemThu(db => ApiTestData.AddUserAsync(db, 2)) : 0;
        using var client = role == 5 ? factory.CreateAuthenticatedClient(otherOrganizer, "BanToChuc") : Client(s, role);
        using var response = await client.GetAsync($"/api/events/{s.EventId}/manage");
        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.OK)
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(s.OrganizerUserId, json.RootElement.GetProperty("data").GetProperty("banToChuc").GetProperty("taiKhoanId").GetInt32());
        }
        else Assert.DoesNotContain("taiKhoanId", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(TrangThaiSuKien.ChoDuyet)] [InlineData(TrangThaiSuKien.TuChoi)] [InlineData(TrangThaiSuKien.DaHuy)]
    public async Task Private_events_do_not_escape_via_list_detail_or_ticket_types(TrangThaiSuKien state)
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory, state);
        var name = await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.DanhSachSuKien.Where(e => e.SuKienId == s.EventId).Select(e => e.TenSuKien).SingleAsync());
        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"/api/events?q={Uri.EscapeDataString(name)}");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(json.RootElement.GetProperty("data").EnumerateArray());
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/events/{s.EventId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/events/{s.EventId}/ticket-types")).StatusCode);
    }

    [Fact]
    public async Task Buyer_orders_and_tickets_keep_purchase_data_without_staff_metadata_or_entity_graphs()
    {
        var s = await ApiTestData.TaoKichBanKiemThuAsync(factory, trangThaiDonVe: TrangThaiDon.ThanhCong);
        var qrAndCode = await factory.ThucThiVoiCoSoDuLieuKiemThu(db => db.DanhSachVe.Where(t => t.VeId == s.TicketId).Select(t => new { t.NoiDungMaQr, t.MaVe }).SingleAsync());
        using var client = Client(s, 1);
        foreach (var path in new[] { "/api/orders/me", $"/api/orders/{s.OrderId}", "/api/tickets/me", $"/api/tickets/{s.TicketId}", $"/api/tickets/by-code/{Uri.EscapeDataString(qrAndCode.MaVe)}" })
        {
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var data = json.RootElement.GetProperty("data");
            RejectFields(data, InternalFields.Concat(["datDiaDiem", "tienCoc", "danhSachSuKien"]).ToArray());
            var record = data.ValueKind == JsonValueKind.Array ? Assert.Single(data.EnumerateArray()) : data;
            if (path.StartsWith("/api/orders"))
            {
                Assert.Equal(100000, record.GetProperty("tongTien").GetDecimal());
                Assert.Equal(s.EventId, record.GetProperty("suKien").GetProperty("id").GetInt32());
                Assert.Equal(100000, Assert.Single(record.GetProperty("chiTiets").EnumerateArray()).GetProperty("donGia").GetDecimal());
                if (path == $"/api/orders/{s.OrderId}")
                {
                    Assert.Equal(qrAndCode.NoiDungMaQr, Assert.Single(record.GetProperty("ves").EnumerateArray()).GetProperty("maQR").GetString());
                    Assert.Single(record.GetProperty("thanhToans").EnumerateArray());
                }
            }
            else
            {
                Assert.Equal(qrAndCode.NoiDungMaQr, record.GetProperty("maQR").GetString());
                Assert.Equal(s.EventId, record.GetProperty("loaiVe").GetProperty("suKien").GetProperty("id").GetInt32());
            }
        }
        using var created = await client.PostAsJsonAsync("/api/orders", new { items = new[] { new { loaiVeId = s.TicketTypeId, soLuong = 1 } } });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        RejectFields(createdJson.RootElement, InternalFields);
    }

    private static void Fields(JsonElement value, params string[] expected) =>
        Assert.Equal(expected.Order(StringComparer.Ordinal), value.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));

    private static void RejectFields(JsonElement value, string[] banned)
    {
        if (value.ValueKind == JsonValueKind.Object)
            foreach (var p in value.EnumerateObject())
            {
                Assert.DoesNotContain(p.Name, banned, StringComparer.OrdinalIgnoreCase);
                RejectFields(p.Value, banned);
            }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (var element in value.EnumerateArray()) RejectFields(element, banned);
    }
}
