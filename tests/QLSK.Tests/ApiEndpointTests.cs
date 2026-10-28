using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.DiaDiem;
using QLSK.Features.DonVe;
using QLSK.Features.LoaiVe;
using QLSK.Features.SuKien;
using QLSK.Features.TaiKhoan;
using QLSK.Features.ThanhToan;
using QLSK.Features.XacThucToChuc;
using QLSK.Infrastructure.PayOS;
using Xunit;

namespace QLSK.Tests;

public class ApiEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact(DisplayName = "GET /api/event-types")]
    public async Task Get_api_event_types()
    {
        await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateClient().GetAsync("/api/event-types"));
    }

    [Fact(DisplayName = "GET /api/event-types/all")]
    public async Task Get_api_event_types_all()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").GetAsync("/api/event-types/all"));
    }

    [Fact(DisplayName = "POST /api/event-types")]
    public async Task Post_api_event_types()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").PostAsJsonAsync(
            "/api/event-types", new LoaiSuKienDto { Ten = Unique("Loại"), MoTa = "Test" }));
    }

    [Fact(DisplayName = "PUT /api/event-types/{id}")]
    public async Task Put_api_event_types_id()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").PutAsJsonAsync(
            $"/api/event-types/{data.CategoryId}", new LoaiSuKienDto { Ten = Unique("Loại sửa"), MoTa = "Test" }));
    }

    [Fact(DisplayName = "GET /api/events")]
    public async Task Get_api_events()
    {
        await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateClient().GetAsync("/api/events"));
    }

    [Fact(DisplayName = "GET /api/events/manage")]
    public async Task Get_api_events_manage()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").GetAsync("/api/events/manage"));
    }

    [Fact(DisplayName = "GET /api/events/mine")]
    public async Task Get_api_events_mine()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").GetAsync("/api/events/mine"));
    }

    [Fact(DisplayName = "GET /api/events/{id}")]
    public async Task Get_api_events_id()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateClient().GetAsync($"/api/events/{data.EventId}"));
    }

    [Fact(DisplayName = "POST /api/events")]
    public async Task Post_api_events()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PostAsJsonAsync(
            "/api/events", EventDto(data.CategoryId, Unique("Sự kiện"))));
    }

    [Fact(DisplayName = "PUT /api/events/{id}")]
    public async Task Put_api_events_id()
    {
        var data = await ApiTestData.ScenarioAsync(factory, TrangThaiSuKien.ChoDuyet);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PutAsJsonAsync(
            $"/api/events/{data.EventId}", EventDto(data.CategoryId, Unique("Sự kiện sửa"))));
    }

    [Fact(DisplayName = "POST /api/events/{id}/submit")]
    public async Task Post_api_events_id_submit()
    {
        var data = await ApiTestData.ScenarioAsync(factory, TrangThaiSuKien.ChoDuyet);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/events/{data.EventId}/submit", null));
    }

    [Fact(DisplayName = "POST /api/events/{id}/approve")]
    public async Task Post_api_events_id_approve()
    {
        var data = await ApiTestData.ScenarioAsync(factory, TrangThaiSuKien.ChoDuyet);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsync(
            $"/api/events/{data.EventId}/approve", null));
    }

    [Fact(DisplayName = "POST /api/events/{id}/reject")]
    public async Task Post_api_events_id_reject()
    {
        var data = await ApiTestData.ScenarioAsync(factory, TrangThaiSuKien.ChoDuyet);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/events/{data.EventId}/reject", new TuChoiSuKienDto { LyDo = "Không phù hợp" }));
    }

    [Fact(DisplayName = "POST /api/events/{id}/cancel")]
    public async Task Post_api_events_id_cancel()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/events/{data.EventId}/cancel", null));
    }

    [Fact(DisplayName = "POST /api/auth/logout")]
    public async Task Post_api_auth_logout()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").PostAsync("/api/auth/logout", null));
    }

    [Fact(DisplayName = "PUT /api/auth/change-password")]
    public async Task Put_api_auth_change_password()
    {
        var userId = await AddPasswordUser("Old@123456");
        await AssertOk(factory.CreateAuthenticatedClient(userId, "NguoiDung").PutAsJsonAsync(
            "/api/auth/change-password", new DoiMatKhauDto { MatKhauCu = "Old@123456", MatKhauMoi = "New@123456" }));
    }

    [Fact(DisplayName = "GET /api/accounts/me")]
    public async Task Get_api_accounts_me()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").GetAsync("/api/accounts/me"));
    }

    [Fact(DisplayName = "PUT /api/accounts/me")]
    public async Task Put_api_accounts_me()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").PutAsJsonAsync(
            "/api/accounts/me", new SuaTaiKhoanDto { HoTen = "Tên mới" }));
    }

    [Fact(DisplayName = "GET /api/accounts")]
    public async Task Get_api_accounts()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").GetAsync(
            "/api/accounts?page=1&size=10"));
    }

    [Fact(DisplayName = "POST /api/accounts/{id}/lock")]
    public async Task Post_api_accounts_id_lock()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").PostAsync(
            $"/api/accounts/{data.UserId}/lock", null));
    }

    [Fact(DisplayName = "POST /api/accounts/{id}/unlock")]
    public async Task Post_api_accounts_id_unlock()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var target = await factory.WithDb(db => ApiTestData.AddUserAsync(db, 1, false));
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").PostAsync(
            $"/api/accounts/{target}/unlock", null));
    }

    [Fact(DisplayName = "PUT /api/accounts/{id}/role")]
    public async Task Put_api_accounts_id_role()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").PutAsJsonAsync(
            $"/api/accounts/{data.UserId}/role", new DoiVaiTroDto { VaiTroId = 2 }));
    }

    [Fact(DisplayName = "GET /api/venues")]
    public async Task Get_api_venues()
    {
        await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateClient().GetAsync("/api/venues"));
    }

    [Fact(DisplayName = "GET /api/venues/all")]
    public async Task Get_api_venues_all()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").GetAsync("/api/venues/all"));
    }

    [Fact(DisplayName = "POST /api/venues")]
    public async Task Post_api_venues()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").PostAsJsonAsync(
            "/api/venues", VenueDto(Unique("Địa điểm"))));
    }

    [Fact(DisplayName = "PUT /api/venues/{id}")]
    public async Task Put_api_venues_id()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").PutAsJsonAsync(
            $"/api/venues/{data.VenueId}", VenueDto(Unique("Địa điểm sửa"))));
    }

    [Fact(DisplayName = "POST /api/venue-bookings")]
    public async Task Post_api_venue_bookings()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await factory.WithDb(async db =>
        {
            var booking = await db.DatDiaDiems.FindAsync(data.BookingId);
            db.DatDiaDiems.Remove(booking!);
            await db.SaveChangesAsync();
        });
        var start = DateTime.UtcNow.AddDays(10);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PostAsJsonAsync(
            "/api/venue-bookings",
            new DatDiaDiemDto { DiaDiemId = data.VenueId, SuKienId = data.EventId, BatDau = start, KetThuc = start.AddHours(2), TienCoc = 100_000 }));
    }

    [Fact(DisplayName = "GET /api/venue-bookings")]
    public async Task Get_api_venue_bookings()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").GetAsync("/api/venue-bookings"));
    }

    [Fact(DisplayName = "POST /api/venue-bookings/{id}/approve")]
    public async Task Post_api_venue_bookings_id_approve()
    {
        var data = await ApiTestData.ScenarioAsync(factory, bookingState: TrangThaiDatCho.ChoXacNhan);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsync(
            $"/api/venue-bookings/{data.BookingId}/approve", null));
    }

    [Fact(DisplayName = "POST /api/venue-bookings/{id}/reject")]
    public async Task Post_api_venue_bookings_id_reject()
    {
        var data = await ApiTestData.ScenarioAsync(factory, bookingState: TrangThaiDatCho.ChoXacNhan);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsync(
            $"/api/venue-bookings/{data.BookingId}/reject", null));
    }

    [Fact(DisplayName = "POST /api/venue-bookings/{id}/cancel")]
    public async Task Post_api_venue_bookings_id_cancel()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/venue-bookings/{data.BookingId}/cancel", null));
    }

    [Fact(DisplayName = "GET /api/events/{eventId}/ticket-types")]
    public async Task Get_api_events_eventId_ticket_types()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateClient().GetAsync($"/api/events/{data.EventId}/ticket-types"));
    }

    [Fact(DisplayName = "POST /api/events/{eventId}/ticket-types")]
    public async Task Post_api_events_eventId_ticket_types()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PostAsJsonAsync(
            $"/api/events/{data.EventId}/ticket-types", TicketTypeDto(Unique("Vé"))));
    }

    [Fact(DisplayName = "PUT /api/ticket-types/{id}")]
    public async Task Put_api_ticket_types_id()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PutAsJsonAsync(
            $"/api/ticket-types/{data.TicketTypeId}", TicketTypeDto(Unique("Vé sửa"))));
    }

    [Fact(DisplayName = "POST /api/ticket-types/{id}/open")]
    public async Task Post_api_ticket_types_id_open()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/ticket-types/{data.TicketTypeId}/open", null));
    }

    [Fact(DisplayName = "POST /api/ticket-types/{id}/close")]
    public async Task Post_api_ticket_types_id_close()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/ticket-types/{data.TicketTypeId}/close", null));
    }

    [Fact(DisplayName = "POST /api/orders")]
    public async Task Post_api_orders()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").PostAsJsonAsync(
            "/api/orders", new TaoDonDto { Items = [new DatVeItemDto { LoaiVeId = data.TicketTypeId, SoLuong = 1 }] }));
    }

    [Fact(DisplayName = "GET /api/orders/me")]
    public async Task Get_api_orders_me()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").GetAsync("/api/orders/me"));
    }

    [Fact(DisplayName = "GET /api/orders/{id}")]
    public async Task Get_api_orders_id()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").GetAsync($"/api/orders/{data.OrderId}"));
    }

    [Fact(DisplayName = "POST /api/orders/{id}/cancel")]
    public async Task Post_api_orders_id_cancel()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").PostAsync(
            $"/api/orders/{data.OrderId}/cancel", null));
    }

    [Fact(DisplayName = "POST /api/payments")]
    public async Task Post_api_payments()
    {
        var data = await ApiTestData.ScenarioAsync(factory, paymentState: TrangThaiThanhToan.DangXuLy);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").PostAsJsonAsync(
            "/api/payments", new TaoThanhToanDto { DonVeId = data.OrderId }));
    }

    [Fact(DisplayName = "POST /api/payments/webhook")]
    public async Task Post_api_payments_webhook()
    {
        var data = await ApiTestData.ScenarioAsync(factory, paymentState: TrangThaiThanhToan.DangXuLy);
        var webhookData = JsonSerializer.SerializeToElement(new
        {
            orderCode = data.OrderId,
            amount = 100_000,
            reference = Unique("REF"),
            paymentLinkId = Unique("LINK"),
            code = "00",
            desc = "success"
        });
        var webhook = new PayOsWebhook
        {
            Code = "00",
            Desc = "success",
            Success = true,
            Data = webhookData,
            Signature = SignWebhook(webhookData)
        };
        await AssertOk(factory.CreateClient().PostAsJsonAsync("/api/payments/webhook", webhook));
    }

    [Fact(DisplayName = "POST /api/payments/{id}/refund")]
    public async Task Post_api_payments_id_refund()
    {
        var data = await ApiTestData.ScenarioAsync(factory, orderState: TrangThaiDon.ThanhCong);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/payments/{data.PaymentId}/refund",
            new HoanTienDto { SoTien = 100_000, MaHoanTien = Unique("REFUND") }));
    }

    [Fact(DisplayName = "GET /api/tickets/me")]
    public async Task Get_api_tickets_me()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").GetAsync("/api/tickets/me"));
    }

    [Fact(DisplayName = "GET /api/tickets/{id}")]
    public async Task Get_api_tickets_id()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").GetAsync($"/api/tickets/{data.TicketId}"));
    }

    [Fact(DisplayName = "GET /api/tickets/by-code/{code}")]
    public async Task Get_api_tickets_by_code_code()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var code = await factory.WithDb(db => db.Ves.Where(x => x.Id == data.TicketId).Select(x => x.MaVe).SingleAsync());
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").GetAsync($"/api/tickets/by-code/{code}"));
    }

    [Fact(DisplayName = "POST /api/check-in")]
    public async Task Post_api_check_in()
    {
        var data = await ApiTestData.ScenarioAsync(factory, eventState: TrangThaiSuKien.DangDienRa);
        var code = await factory.WithDb(db => db.Ves.Where(x => x.Id == data.TicketId).Select(x => x.MaQR).SingleAsync());
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            "/api/check-in", new { ma = code }));
    }

    [Fact(DisplayName = "POST /api/tickets/{id}/export")]
    public async Task Post_api_tickets_id_export()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var response = await factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").PostAsync(
            $"/api/tickets/{data.TicketId}/export", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact(DisplayName = "POST /api/tickets/{id}/email")]
    public async Task Post_api_tickets_id_email()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").PostAsync(
            $"/api/tickets/{data.TicketId}/email", null));
    }

    [Fact(DisplayName = "POST /api/organizer-verifications")]
    public async Task Post_api_organizer_verifications()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").PostAsJsonAsync(
            "/api/organizer-verifications",
            new GuiYeuCauDto { TenToChuc = Unique("Tổ chức"), GiayTo = "https://test.local/document.pdf" }));
    }

    [Fact(DisplayName = "GET /api/organizer-verifications/me")]
    public async Task Get_api_organizer_verifications_me()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await factory.WithDb(async db =>
        {
            db.YeuCauXacThucToChucs.Add(new YeuCauXacThucToChuc
            {
                TaiKhoanId = data.UserId,
                TenToChuc = Unique("Tổ chức"),
                GiayTo = "https://test.local/document.pdf"
            });
            await db.SaveChangesAsync();
        });
        await AssertOk(factory.CreateAuthenticatedClient(data.UserId, "NguoiDung").GetAsync(
            "/api/organizer-verifications/me"));
    }

    [Fact(DisplayName = "GET /api/organizer-verifications")]
    public async Task Get_api_organizer_verifications()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await factory.WithDb(ApiTestData.AddPendingVerificationAsync);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").GetAsync(
            "/api/organizer-verifications"));
    }

    [Fact(DisplayName = "POST /api/organizer-verifications/{id}/approve")]
    public async Task Post_api_organizer_verifications_id_approve()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var requestId = await factory.WithDb(ApiTestData.AddPendingVerificationAsync);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsync(
            $"/api/organizer-verifications/{requestId}/approve", null));
    }

    [Fact(DisplayName = "POST /api/organizer-verifications/{id}/reject")]
    public async Task Post_api_organizer_verifications_id_reject()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var requestId = await factory.WithDb(ApiTestData.AddPendingVerificationAsync);
        await AssertOk(factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/organizer-verifications/{requestId}/reject",
            new TuChoiYeuCauDto { LyDo = "Giấy tờ không hợp lệ" }));
    }

    [Fact(DisplayName = "GET /api/admin/dashboard")]
    public async Task Get_api_admin_dashboard()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        await AssertOk(factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").GetAsync("/api/admin/dashboard"));
    }

    private static async Task AssertOk(Task<HttpResponseMessage> responseTask)
    {
        using var response = await responseTask;
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Expected 2xx, got {(int)response.StatusCode}: {body}");
    }

    private async Task<int> AddPasswordUser(string password)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();
        var user = new TaiKhoan
        {
            HoTen = "Password test",
            Email = $"password-{Guid.NewGuid():N}@test.local",
            VaiTroId = 1,
            HoatDong = true
        };
        user.MatKhau = hasher.HashPassword(user, password);
        db.TaiKhoans.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    private static TaoSuKienDto EventDto(int categoryId, string name)
    {
        var start = DateTime.UtcNow.AddDays(20);
        return new TaoSuKienDto
        {
            Ten = name,
            MoTa = "Integration test",
            BatDau = start,
            KetThuc = start.AddHours(2),
            SoNguoi = 100,
            LoaiSuKienId = categoryId
        };
    }

    private static DiaDiemDto VenueDto(string name) => new()
    {
        Ten = name,
        DiaChi = Unique("Địa chỉ"),
        SucChua = 500,
        GiaThue = 1_000_000,
        HoatDong = true
    };

    private static LoaiVeDto TicketTypeDto(string name) => new()
    {
        Ten = name,
        Gia = 100_000,
        SoLuong = 100,
        MoBan = DateTime.UtcNow.AddDays(-1),
        DongBan = DateTime.UtcNow.AddDays(5)
    };

    private static string SignWebhook(JsonElement data)
    {
        var raw = string.Join("&", data.EnumerateObject()
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => $"{x.Name}={x.Value.ValueKind switch
            {
                JsonValueKind.String => x.Value.GetString() ?? "",
                JsonValueKind.Null or JsonValueKind.Undefined => "",
                _ => x.Value.GetRawText()
            }}"));
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("test-checksum-key"));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
    }

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";
}
