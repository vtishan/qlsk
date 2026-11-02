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

public class ApiEndpointTests(ApiFactory mayChuKiemThu) : IClassFixture<ApiFactory>
{
    [Fact(DisplayName = "GET /api/event-types")]
    public async Task Get_api_event_types()
    {
        await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateClient().GetAsync("/api/event-types"));
    }

    [Fact(DisplayName = "GET /api/event-types/all")]
    public async Task Get_api_event_types_all()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").GetAsync("/api/event-types/all"));
    }

    [Fact(DisplayName = "POST /api/event-types")]
    public async Task Post_api_event_types()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").PostAsJsonAsync(
            "/api/event-types", new LoaiSuKienDto { TenLoaiSuKien = TaoChuoiTenDuyNhat("Loại"), MoTaLoaiSuKien = "Test" }));
    }

    [Fact(DisplayName = "PUT /api/event-types/{id}")]
    public async Task Put_api_event_types_id()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").PutAsJsonAsync(
            $"/api/event-types/{kichBanKiemThu.CategoryId}", new LoaiSuKienDto { TenLoaiSuKien = TaoChuoiTenDuyNhat("Loại sửa"), MoTaLoaiSuKien = "Test" }));
    }

    [Fact(DisplayName = "GET /api/events")]
    public async Task Get_api_events()
    {
        await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateClient().GetAsync("/api/events"));
    }

    [Fact(DisplayName = "GET /api/events/manage")]
    public async Task Get_api_events_manage()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").GetAsync("/api/events/manage"));
    }

    [Fact(DisplayName = "GET /api/events/mine")]
    public async Task Get_api_events_mine()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").GetAsync("/api/events/mine"));
    }

    [Fact(DisplayName = "GET /api/events/{id}")]
    public async Task Get_api_events_id()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateClient().GetAsync($"/api/events/{kichBanKiemThu.EventId}"));
    }

    [Fact(DisplayName = "POST /api/events")]
    public async Task Post_api_events()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PostAsJsonAsync(
            "/api/events", TaoDuLieuSuKien(kichBanKiemThu.CategoryId, TaoChuoiTenDuyNhat("Sự kiện"))));
    }

    [Fact(DisplayName = "PUT /api/events/{id}")]
    public async Task Put_api_events_id()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, TrangThaiSuKien.ChoDuyet);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PutAsJsonAsync(
            $"/api/events/{kichBanKiemThu.EventId}", TaoDuLieuSuKien(kichBanKiemThu.CategoryId, TaoChuoiTenDuyNhat("Sự kiện sửa"))));
    }

    [Fact(DisplayName = "POST /api/events/{id}/submit")]
    public async Task Post_api_events_id_submit()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, TrangThaiSuKien.ChoDuyet);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/events/{kichBanKiemThu.EventId}/submit", null));
    }

    [Fact(DisplayName = "POST /api/events/{id}/approve")]
    public async Task Post_api_events_id_approve()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, TrangThaiSuKien.ChoDuyet);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsync(
            $"/api/events/{kichBanKiemThu.EventId}/approve", null));
    }

    [Fact(DisplayName = "POST /api/events/{id}/reject")]
    public async Task Post_api_events_id_reject()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, TrangThaiSuKien.ChoDuyet);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/events/{kichBanKiemThu.EventId}/reject", new TuChoiSuKienDto { LyDo = "Không phù hợp" }));
    }

    [Fact(DisplayName = "POST /api/events/{id}/cancel")]
    public async Task Post_api_events_id_cancel()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/events/{kichBanKiemThu.EventId}/cancel", null));
    }

    [Fact(DisplayName = "POST /api/auth/logout")]
    public async Task Post_api_auth_logout()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").PostAsync("/api/auth/logout", null));
    }

    [Fact(DisplayName = "PUT /api/auth/change-password")]
    public async Task Put_api_auth_change_password()
    {
        var taiKhoanId = await TaoTaiKhoanCoMatKhau("Old@123456");
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(taiKhoanId, "NguoiDung").PutAsJsonAsync(
            "/api/auth/change-password", new DoiMatKhauDto { MatKhauCu = "Old@123456", MatKhauMoi = "New@123456" }));
    }

    [Fact(DisplayName = "GET /api/accounts/me")]
    public async Task Get_api_accounts_me()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").GetAsync("/api/accounts/me"));
    }

    [Fact(DisplayName = "PUT /api/accounts/me")]
    public async Task Put_api_accounts_me()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").PutAsJsonAsync(
            "/api/accounts/me", new SuaTaiKhoanDto { HoTen = "Tên mới" }));
    }

    [Fact(DisplayName = "GET /api/accounts")]
    public async Task Get_api_accounts()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").GetAsync(
            "/api/accounts?page=1&size=10"));
    }

    [Fact(DisplayName = "POST /api/accounts/{id}/lock")]
    public async Task Post_api_accounts_id_lock()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").PostAsync(
            $"/api/accounts/{kichBanKiemThu.UserId}/lock", null));
    }

    [Fact(DisplayName = "POST /api/accounts/{id}/unlock")]
    public async Task Post_api_accounts_id_unlock()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var taiKhoanCanXuLy = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => ApiTestData.AddUserAsync(nguCanhDuLieu, 1, false));
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").PostAsync(
            $"/api/accounts/{taiKhoanCanXuLy}/unlock", null));
    }

    [Fact(DisplayName = "PUT /api/accounts/{id}/role")]
    public async Task Put_api_accounts_id_role()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").PutAsJsonAsync(
            $"/api/accounts/{kichBanKiemThu.UserId}/role", new DoiVaiTroDto { VaiTroId = 2 }));
    }

    [Fact(DisplayName = "GET /api/venues")]
    public async Task Get_api_venues()
    {
        await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateClient().GetAsync("/api/venues"));
    }

    [Fact(DisplayName = "GET /api/venues/all")]
    public async Task Get_api_venues_all()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").GetAsync("/api/venues/all"));
    }

    [Fact(DisplayName = "POST /api/venues")]
    public async Task Post_api_venues()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").PostAsJsonAsync(
            "/api/venues", TaoDuLieuDiaDiem(TaoChuoiTenDuyNhat("Địa điểm"))));
    }

    [Fact(DisplayName = "PUT /api/venues/{id}")]
    public async Task Put_api_venues_id()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").PutAsJsonAsync(
            $"/api/venues/{kichBanKiemThu.VenueId}", TaoDuLieuDiaDiem(TaoChuoiTenDuyNhat("Địa điểm sửa"))));
    }

    [Fact(DisplayName = "POST /api/venue-bookings")]
    public async Task Post_api_venue_bookings()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var phieuDatDiaDiem = await nguCanhDuLieu.DanhSachDatDiaDiem.FindAsync(kichBanKiemThu.BookingId);
            nguCanhDuLieu.DanhSachDatDiaDiem.Remove(phieuDatDiaDiem!);
            await nguCanhDuLieu.SaveChangesAsync();
        });
        var thoiDiemBatDau = DateTime.UtcNow.AddDays(10);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PostAsJsonAsync(
            "/api/venue-bookings",
            new DatDiaDiemDto { DiaDiemId = kichBanKiemThu.VenueId, SuKienId = kichBanKiemThu.EventId, ThoiGianBatDau = thoiDiemBatDau, ThoiGianKetThuc = thoiDiemBatDau.AddHours(2), TienCoc = 100_000 }));
    }

    [Fact(DisplayName = "GET /api/venue-bookings")]
    public async Task Get_api_venue_bookings()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").GetAsync("/api/venue-bookings"));
    }

    [Fact(DisplayName = "POST /api/venue-bookings/{id}/approve")]
    public async Task Post_api_venue_bookings_id_approve()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiDatDiaDiem: TrangThaiDatCho.ChoXacNhan);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsync(
            $"/api/venue-bookings/{kichBanKiemThu.BookingId}/approve", null));
    }

    [Fact(DisplayName = "POST /api/venue-bookings/{id}/reject")]
    public async Task Post_api_venue_bookings_id_reject()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiDatDiaDiem: TrangThaiDatCho.ChoXacNhan);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsync(
            $"/api/venue-bookings/{kichBanKiemThu.BookingId}/reject", null));
    }

    [Fact(DisplayName = "POST /api/venue-bookings/{id}/cancel")]
    public async Task Post_api_venue_bookings_id_cancel()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/venue-bookings/{kichBanKiemThu.BookingId}/cancel", null));
    }

    [Fact(DisplayName = "GET /api/events/{eventId}/ticket-types")]
    public async Task Get_api_events_eventId_ticket_types()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateClient().GetAsync($"/api/events/{kichBanKiemThu.EventId}/ticket-types"));
    }

    [Fact(DisplayName = "POST /api/events/{eventId}/ticket-types")]
    public async Task Post_api_events_eventId_ticket_types()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PostAsJsonAsync(
            $"/api/events/{kichBanKiemThu.EventId}/ticket-types", TaoDuLieuLoaiVe(TaoChuoiTenDuyNhat("Vé"))));
    }

    [Fact(DisplayName = "PUT /api/ticket-types/{id}")]
    public async Task Put_api_ticket_types_id()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PutAsJsonAsync(
            $"/api/ticket-types/{kichBanKiemThu.TicketTypeId}", TaoDuLieuLoaiVe(TaoChuoiTenDuyNhat("Vé sửa"))));
    }

    [Fact(DisplayName = "POST /api/ticket-types/{id}/open")]
    public async Task Post_api_ticket_types_id_open()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/ticket-types/{kichBanKiemThu.TicketTypeId}/open", null));
    }

    [Fact(DisplayName = "POST /api/ticket-types/{id}/close")]
    public async Task Post_api_ticket_types_id_close()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PostAsync(
            $"/api/ticket-types/{kichBanKiemThu.TicketTypeId}/close", null));
    }

    [Fact(DisplayName = "POST /api/orders")]
    public async Task Post_api_orders()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").PostAsJsonAsync(
            "/api/orders", new TaoDonDto { ChiTietDatVe = [new DatVeItemDto { LoaiVeId = kichBanKiemThu.TicketTypeId, SoLuong = 1 }] }));
    }

    [Fact(DisplayName = "GET /api/orders/me")]
    public async Task Get_api_orders_me()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").GetAsync("/api/orders/me"));
    }

    [Fact(DisplayName = "GET /api/orders/{id}")]
    public async Task Get_api_orders_id()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").GetAsync($"/api/orders/{kichBanKiemThu.OrderId}"));
    }

    [Fact(DisplayName = "POST /api/orders/{id}/cancel")]
    public async Task Post_api_orders_id_cancel()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").PostAsync(
            $"/api/orders/{kichBanKiemThu.OrderId}/cancel", null));
    }

    [Fact(DisplayName = "POST /api/payments")]
    public async Task Post_api_payments()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiThanhToan: TrangThaiThanhToan.DangXuLy);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").PostAsJsonAsync(
            "/api/payments", new TaoThanhToanDto { DonVeId = kichBanKiemThu.OrderId }));
    }

    [Fact(DisplayName = "POST /api/payments/webhook")]
    public async Task Post_api_payments_webhook()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiThanhToan: TrangThaiThanhToan.DangXuLy);
        var webhookData = JsonSerializer.SerializeToElement(new
        {
            orderCode = kichBanKiemThu.OrderId,
            amount = 100_000,
            reference = TaoChuoiTenDuyNhat("REF"),
            paymentLinkId = TaoChuoiTenDuyNhat("LINK"),
            code = "00",
            desc = "success"
        });
        var webhook = new PayOsWebhook
        {
            MaKetQua = "00",
            MoTaKetQua = "success",
            ThanhCong = true,
            DuLieuWebhook = webhookData,
            ChuKyWebhook = KyDuLieuWebhook(webhookData)
        };
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateClient().PostAsJsonAsync("/api/payments/webhook", webhook));
    }

    [Fact(DisplayName = "POST /api/payments/{id}/refund")]
    public async Task Post_api_payments_id_refund()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiDonVe: TrangThaiDon.ThanhCong);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/payments/{kichBanKiemThu.PaymentId}/refund",
            new HoanTienDto { SoTienHoan = 100_000, MaHoanTien = TaoChuoiTenDuyNhat("REFUND") }));
    }

    [Fact(DisplayName = "GET /api/tickets/me")]
    public async Task Get_api_tickets_me()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").GetAsync("/api/tickets/me"));
    }

    [Fact(DisplayName = "GET /api/tickets/{id}")]
    public async Task Get_api_tickets_id()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").GetAsync($"/api/tickets/{kichBanKiemThu.TicketId}"));
    }

    [Fact(DisplayName = "GET /api/tickets/by-code/{code}")]
    public async Task Get_api_tickets_by_code_code()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var maVeTraCuu = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => nguCanhDuLieu.DanhSachVe.Where(veDaPhatHanhDb => veDaPhatHanhDb.VeId == kichBanKiemThu.TicketId).Select(veDaPhatHanhDb => veDaPhatHanhDb.MaVe).SingleAsync());
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").GetAsync($"/api/tickets/by-code/{maVeTraCuu}"));
    }

    [Fact(DisplayName = "POST /api/check-in")]
    public async Task Post_api_check_in()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiSuKien: TrangThaiSuKien.DangDienRa);
        var maVeTraCuu = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => nguCanhDuLieu.DanhSachVe.Where(veDaPhatHanhDb => veDaPhatHanhDb.VeId == kichBanKiemThu.TicketId).Select(veDaPhatHanhDb => veDaPhatHanhDb.NoiDungMaQr).SingleAsync());
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            "/api/check-in", new { ma = maVeTraCuu }));
    }

    [Fact(DisplayName = "POST /api/tickets/{id}/export")]
    public async Task Post_api_tickets_id_export()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").PostAsync(
            $"/api/tickets/{kichBanKiemThu.TicketId}/export", null);
        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        Assert.Equal("application/pdf", phanHoiYeuCauHttp.Content.Headers.ContentType?.MediaType);
    }

    [Fact(DisplayName = "POST /api/tickets/{id}/email")]
    public async Task Post_api_tickets_id_email()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").PostAsync(
            $"/api/tickets/{kichBanKiemThu.TicketId}/email", null));
    }

    [Fact(DisplayName = "POST /api/organizer-verifications")]
    public async Task Post_api_organizer_verifications()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").PostAsJsonAsync(
            "/api/organizer-verifications",
            new GuiYeuCauDto { TenToChuc = TaoChuoiTenDuyNhat("Tổ chức"), GiayTo = "https://test.local/document.pdf" }));
    }

    [Fact(DisplayName = "GET /api/organizer-verifications/me")]
    public async Task Get_api_organizer_verifications_me()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            nguCanhDuLieu.YeuCauXacThucToChuc.Add(new YeuCauXacThucToChuc
            {
                TaiKhoanId = kichBanKiemThu.UserId,
                TenToChuc = TaoChuoiTenDuyNhat("Tổ chức"),
                GiayTo = "https://test.local/document.pdf"
            });
            await nguCanhDuLieu.SaveChangesAsync();
        });
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").GetAsync(
            "/api/organizer-verifications/me"));
    }

    [Fact(DisplayName = "GET /api/organizer-verifications")]
    public async Task Get_api_organizer_verifications()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(ApiTestData.AddPendingVerificationAsync);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").GetAsync(
            "/api/organizer-verifications"));
    }

    [Fact(DisplayName = "POST /api/organizer-verifications/{id}/approve")]
    public async Task Post_api_organizer_verifications_id_approve()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var maYeuCauHttp = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(ApiTestData.AddPendingVerificationAsync);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsync(
            $"/api/organizer-verifications/{maYeuCauHttp}/approve", null));
    }

    [Fact(DisplayName = "POST /api/organizer-verifications/{id}/reject")]
    public async Task Post_api_organizer_verifications_id_reject()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var maYeuCauHttp = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(ApiTestData.AddPendingVerificationAsync);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/organizer-verifications/{maYeuCauHttp}/reject",
            new TuChoiYeuCauDto { LyDo = "Giấy tờ không hợp lệ" }));
    }

    [Fact(DisplayName = "GET /api/admin/dashboard")]
    public async Task Get_api_admin_dashboard()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await KiemTraPhanHoiThanhCong(mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").GetAsync("/api/admin/dashboard"));
    }

    private static async Task KiemTraPhanHoiThanhCong(Task<HttpResponseMessage> tacVuGuiYeuCauHttp)
    {
        using var phanHoiYeuCauHttp = await tacVuGuiYeuCauHttp;
        var noiDungYeuCauHttp = await phanHoiYeuCauHttp.Content.ReadAsStringAsync();
        Assert.True(phanHoiYeuCauHttp.IsSuccessStatusCode, $"Expected 2xx, got {(int)phanHoiYeuCauHttp.StatusCode}: {noiDungYeuCauHttp}");
    }

    private async Task<int> TaoTaiKhoanCoMatKhau(string matKhauNhapLieu)
    {
        using var phamViDichVu = mayChuKiemThu.Services.CreateScope();
        var nguCanhDuLieu = phamViDichVu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var boBamMatKhau = phamViDichVu.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();
        var taiKhoanNguoiDung = new TaiKhoan
        {
            HoTen = "Password test",
            Email = $"password-{Guid.NewGuid():N}@test.local",
            VaiTroId = 1,
            HoatDong = true
        };
        taiKhoanNguoiDung.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanNguoiDung, matKhauNhapLieu);
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoanNguoiDung);
        await nguCanhDuLieu.SaveChangesAsync();
        return taiKhoanNguoiDung.TaiKhoanId;
    }

    private static TaoSuKienDto TaoDuLieuSuKien(int loaiSuKienId, string tenSuKien)
    {
        var thoiDiemBatDau = DateTime.UtcNow.AddDays(20);
        return new TaoSuKienDto
        {
            TenSuKien = tenSuKien,
            MoTaSuKien = "Integration test",
            ThoiGianBatDau = thoiDiemBatDau,
            ThoiGianKetThuc = thoiDiemBatDau.AddHours(2),
            SoNguoi = 100,
            LoaiSuKienId = loaiSuKienId
        };
    }

    private static DiaDiemDto TaoDuLieuDiaDiem(string tenDiaDiem) => new()
    {
        TenDiaDiem = tenDiaDiem,
        DiaChi = TaoChuoiTenDuyNhat("Địa chỉ"),
        SucChua = 500,
        GiaThue = 1_000_000,
        HoatDong = true
    };

    private static LoaiVeDto TaoDuLieuLoaiVe(string tenLoaiVe) => new()
    {
        TenLoaiVe = tenLoaiVe,
        GiaVe = 100_000,
        SoLuong = 100,
        ThoiGianMoBan = DateTime.UtcNow.AddDays(-1),
        ThoiGianDongBan = DateTime.UtcNow.AddDays(5)
    };

    private static string KyDuLieuWebhook(JsonElement duLieuWebhookJson)
    {
        var noiDungJsonGoc = string.Join("&", duLieuWebhookJson.EnumerateObject()
            .OrderBy(thuocTinhJson => thuocTinhJson.Name, StringComparer.Ordinal)
            .Select(thuocTinhJson => $"{thuocTinhJson.Name}={thuocTinhJson.Value.ValueKind switch
            {
                JsonValueKind.String => thuocTinhJson.Value.GetString() ?? "",
                JsonValueKind.Null or JsonValueKind.Undefined => "",
                _ => thuocTinhJson.Value.GetRawText()
            }}"));
        using var boBamHmac = new HMACSHA256(Encoding.UTF8.GetBytes("test-checksum-key"));
        return Convert.ToHexString(boBamHmac.ComputeHash(Encoding.UTF8.GetBytes(noiDungJsonGoc))).ToLowerInvariant();
    }

    private static string TaoChuoiTenDuyNhat(string tienToDuLieu) => $"{tienToDuLieu}-{Guid.NewGuid():N}";
}
