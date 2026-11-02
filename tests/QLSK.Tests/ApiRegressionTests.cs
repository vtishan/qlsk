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
using QLSK.Features.TaiKhoan;
using QLSK.Features.ThanhToan;
using QLSK.Infrastructure.PayOS;
using Xunit;
namespace QLSK.Tests;

public class ApiRegressionTests(ApiFactory mayChuKiemThu) : IClassFixture<ApiFactory>
{
    [Fact(DisplayName = "PUT /api/accounts/{id}/role - token cũ mất quyền")]
    public async Task Put_api_accounts_id_role_revokes_old_token()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var clientQuanLyTruocDoiVaiTro = mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien");

        var changeRole = await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien").PutAsJsonAsync(
            $"/api/accounts/{kichBanKiemThu.ManagerId}/role",
            new DoiVaiTroDto { VaiTroId = 1 });
        Assert.Equal(HttpStatusCode.OK, changeRole.StatusCode);

        var phanHoiYeuCauHttp = await clientQuanLyTruocDoiVaiTro.GetAsync("/api/events/manage");
        Assert.Equal(HttpStatusCode.Unauthorized, phanHoiYeuCauHttp.StatusCode);
    }

    [Fact(DisplayName = "PUT /api/auth/change-password - token cũ mất hiệu lực")]
    public async Task Put_api_auth_change_password_revokes_old_token()
    {
        using var phamViDichVu = mayChuKiemThu.Services.CreateScope();
        var nguCanhDuLieu = phamViDichVu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var boBamMatKhau = phamViDichVu.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();
        var taiKhoanId = await ApiTestData.AddPasswordUserAsync(nguCanhDuLieu, boBamMatKhau);
        var clientTruocDoiMatKhau = mayChuKiemThu.CreateAuthenticatedClient(taiKhoanId, "NguoiDung");

        var changePassword = await clientTruocDoiMatKhau.PutAsJsonAsync(
            "/api/auth/change-password",
            new DoiMatKhauDto { MatKhauCu = "Old@123456", MatKhauMoi = "New@123456" });
        Assert.Equal(HttpStatusCode.OK, changePassword.StatusCode);

        var phanHoiYeuCauHttp = await clientTruocDoiMatKhau.GetAsync("/api/accounts/me");
        Assert.Equal(HttpStatusCode.Unauthorized, phanHoiYeuCauHttp.StatusCode);
    }

    [Fact(DisplayName = "GET /api/events/{id} - không lộ mật khẩu tài khoản")]
    public async Task Get_api_events_id_does_not_expose_password_hash()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateClient().GetAsync($"/api/events/{kichBanKiemThu.EventId}");
        var noiDungYeuCauHttp = await phanHoiYeuCauHttp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        Assert.DoesNotContain("MatKhau", noiDungYeuCauHttp, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "GET /api/events/{eventId}/ticket-types - chặn sự kiện riêng tư")]
    public async Task Get_api_events_eventId_ticket_types_hides_private_event()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, TrangThaiSuKien.ChoDuyet);

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateClient().GetAsync($"/api/events/{kichBanKiemThu.EventId}/ticket-types");

        Assert.Equal(HttpStatusCode.Forbidden, phanHoiYeuCauHttp.StatusCode);
    }

    [Fact(DisplayName = "POST /api/check-in - chặn ngoài thời gian sự kiện")]
    public async Task Post_api_check_in_rejects_event_not_in_progress()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, TrangThaiSuKien.DaDuyet);
        var maVeTraCuu = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => nguCanhDuLieu.DanhSachVe
            .Where(veDaPhatHanhDb => veDaPhatHanhDb.VeId == kichBanKiemThu.TicketId)
            .Select(veDaPhatHanhDb => veDaPhatHanhDb.NoiDungMaQr)
            .SingleAsync());

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien")
            .PostAsJsonAsync("/api/check-in", new { ma = maVeTraCuu });

        Assert.Equal(HttpStatusCode.Conflict, phanHoiYeuCauHttp.StatusCode);
    }

    [Fact(DisplayName = "POST /api/payments - đơn miễn phí phát vé không tạo payment")]
    public async Task Post_api_payments_completes_free_order()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(
            mayChuKiemThu,
            trangThaiDonVe: TrangThaiDon.ChoThanhToan,
            trangThaiThanhToan: TrangThaiThanhToan.DangXuLy);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var donVeDangXuLy = await nguCanhDuLieu.DanhSachDonVe.Include(donVeDb => donVeDb.ChiTietDonVe).Include(donVeDb => donVeDb.DanhSachThanhToan).Include(donVeDb => donVeDb.DanhSachVe)
                .SingleAsync(donVeDb => donVeDb.DonVeId == kichBanKiemThu.OrderId);
            var loaiVeDangXuLy = await nguCanhDuLieu.DanhSachLoaiVe.SingleAsync(loaiVeDb => loaiVeDb.LoaiVeId == kichBanKiemThu.TicketTypeId);
            nguCanhDuLieu.DanhSachVe.RemoveRange(donVeDangXuLy.DanhSachVe);
            nguCanhDuLieu.DanhSachThanhToan.RemoveRange(donVeDangXuLy.DanhSachThanhToan);
            donVeDangXuLy.TongTien = 0;
            foreach (var chiTietSuKien in donVeDangXuLy.ChiTietDonVe)
            {
                chiTietSuKien.DonGia = 0;
                chiTietSuKien.ThanhTien = 0;
            }
            loaiVeDangXuLy.GiaVe = 0;
            loaiVeDangXuLy.SoLuongDaBan = 0;
            loaiVeDangXuLy.SoLuongDangGiu = 1;
            await nguCanhDuLieu.SaveChangesAsync();
        });

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung")
            .PostAsJsonAsync("/api/payments", new TaoThanhToanDto { DonVeId = kichBanKiemThu.OrderId });

        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var donVeDangXuLy = await nguCanhDuLieu.DanhSachDonVe.Include(donVeDb => donVeDb.DanhSachThanhToan).Include(donVeDb => donVeDb.DanhSachVe)
                .SingleAsync(donVeDb => donVeDb.DonVeId == kichBanKiemThu.OrderId);
            Assert.Equal(TrangThaiDon.ThanhCong, donVeDangXuLy.TrangThai);
            Assert.Empty(donVeDangXuLy.DanhSachThanhToan);
            Assert.Single(donVeDangXuLy.DanhSachVe);
        });
    }

    [Fact(DisplayName = "POST /api/payments/webhook - không phát vé dở dang")]
    public async Task Post_api_payments_webhook_is_atomic_when_inventory_invalid()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(
            mayChuKiemThu,
            trangThaiDonVe: TrangThaiDon.ChoThanhToan,
            trangThaiThanhToan: TrangThaiThanhToan.DangXuLy);
        var loaiVeThuHaiId = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var donVeDangXuLy = await nguCanhDuLieu.DanhSachDonVe.Include(donVeDb => donVeDb.DanhSachVe).SingleAsync(donVeDb => donVeDb.DonVeId == kichBanKiemThu.OrderId);
            nguCanhDuLieu.DanhSachVe.RemoveRange(donVeDangXuLy.DanhSachVe);
            var loaiVeThuHai = new LoaiVe
            {
                SuKienId = kichBanKiemThu.EventId,
                TenLoaiVe = $"Vé hết-{Guid.NewGuid():N}",
                GiaVe = 50_000,
                SoLuong = 1,
                SoLuongDaBan = 1,
                SoLuongDangGiu = 0,
                ThoiGianMoBan = DateTime.UtcNow.AddDays(-1),
                ThoiGianDongBan = DateTime.UtcNow.AddDays(1)
            };
            donVeDangXuLy.ChiTietDonVe.Add(new ChiTietDonVe
            {
                LoaiVe = loaiVeThuHai,
                SoLuong = 1,
                DonGia = 50_000,
                ThanhTien = 50_000
            });
            donVeDangXuLy.SoLuong = 2;
            donVeDangXuLy.TongTien = 150_000;
            var thanhToanKiemThu = await nguCanhDuLieu.DanhSachThanhToan.SingleAsync(thanhToanDb => thanhToanDb.ThanhToanId == kichBanKiemThu.PaymentId);
            thanhToanKiemThu.SoTien = donVeDangXuLy.TongTien;
            await nguCanhDuLieu.SaveChangesAsync();
            return loaiVeThuHai.LoaiVeId;
        });
        var tonVeTruocWebhook = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => nguCanhDuLieu.DanhSachLoaiVe.AsNoTracking()
            .Where(loaiVeDb => loaiVeDb.LoaiVeId == kichBanKiemThu.TicketTypeId)
            .Select(loaiVeDb => new { DaBan = loaiVeDb.SoLuongDaBan, DangGiu = loaiVeDb.SoLuongDangGiu })
            .SingleAsync());
        var webhook = TaoWebhookThanhToan(kichBanKiemThu.OrderId, 150_000, $"REF-{Guid.NewGuid():N}", $"LINK-{Guid.NewGuid():N}");

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateClient().PostAsJsonAsync("/api/payments/webhook", webhook);

        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var loaiVeDauTien = await nguCanhDuLieu.DanhSachLoaiVe.AsNoTracking().SingleAsync(loaiVeDb => loaiVeDb.LoaiVeId == kichBanKiemThu.TicketTypeId);
            var ketQuaThuHai = await nguCanhDuLieu.DanhSachLoaiVe.AsNoTracking().SingleAsync(loaiVeDb => loaiVeDb.LoaiVeId == loaiVeThuHaiId);
            Assert.Equal(tonVeTruocWebhook.DaBan, loaiVeDauTien.SoLuongDaBan);
            Assert.Equal(tonVeTruocWebhook.DangGiu, loaiVeDauTien.SoLuongDangGiu);
            Assert.Equal(1, ketQuaThuHai.SoLuongDaBan);
            Assert.Equal(0, await nguCanhDuLieu.DanhSachVe.CountAsync(veDaPhatHanhDb => veDaPhatHanhDb.DonVeId == kichBanKiemThu.OrderId));
            Assert.Equal(TrangThaiThanhToan.NghiVan,
                await nguCanhDuLieu.DanhSachThanhToan.Where(thanhToanDb => thanhToanDb.ThanhToanId == kichBanKiemThu.PaymentId).Select(thanhToanDb => thanhToanDb.TrangThai).SingleAsync());
        });
    }

    [Fact(DisplayName = "POST /api/payments/webhook - ghép đúng paymentLinkId")]
    public async Task Post_api_payments_webhook_matches_payment_link()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(
            mayChuKiemThu,
            trangThaiDonVe: TrangThaiDon.ChoThanhToan,
            trangThaiThanhToan: TrangThaiThanhToan.DangXuLy);
        const string lienKetThanhToanDauTien = "payment-link-a";
        const string lienKetThanhToanThuHai = "payment-link-b";
        var thanhToanThuHaiId = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            nguCanhDuLieu.DanhSachVe.RemoveRange(await nguCanhDuLieu.DanhSachVe.Where(veDaPhatHanhDb => veDaPhatHanhDb.DonVeId == kichBanKiemThu.OrderId).ToListAsync());
            var thanhToanDauTien = await nguCanhDuLieu.DanhSachThanhToan.SingleAsync(thanhToanDb => thanhToanDb.ThanhToanId == kichBanKiemThu.PaymentId);
            thanhToanDauTien.MaLienKet = lienKetThanhToanDauTien;
            var ketQuaThuHai = new ThanhToan
            {
                DonVeId = kichBanKiemThu.OrderId,
                SoTien = 100_000,
                MaLienKet = lienKetThanhToanThuHai,
                TrangThai = TrangThaiThanhToan.DangXuLy
            };
            nguCanhDuLieu.DanhSachThanhToan.Add(ketQuaThuHai);
            await nguCanhDuLieu.SaveChangesAsync();
            return ketQuaThuHai.ThanhToanId;
        });
        var maThamChieuThanhToan = $"REF-{Guid.NewGuid():N}";

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateClient().PostAsJsonAsync(
            "/api/payments/webhook",
            TaoWebhookThanhToan(kichBanKiemThu.OrderId, 100_000, maThamChieuThanhToan, lienKetThanhToanDauTien));

        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var thanhToanDauTien = await nguCanhDuLieu.DanhSachThanhToan.AsNoTracking().SingleAsync(thanhToanDb => thanhToanDb.ThanhToanId == kichBanKiemThu.PaymentId);
            var ketQuaThuHai = await nguCanhDuLieu.DanhSachThanhToan.AsNoTracking().SingleAsync(thanhToanDb => thanhToanDb.ThanhToanId == thanhToanThuHaiId);
            Assert.Equal(maThamChieuThanhToan, thanhToanDauTien.MaGiaoDich);
            Assert.Equal(TrangThaiThanhToan.ThanhCong, thanhToanDauTien.TrangThai);
            Assert.Equal(TrangThaiThanhToan.ThatBai, ketQuaThuHai.TrangThai);
        });
    }

    [Fact(DisplayName = "POST /api/payments/{id}/refund - hoàn tồn vé")]
    public async Task Post_api_payments_id_refund_restores_inventory()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiDonVe: TrangThaiDon.ThanhCong);

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/payments/{kichBanKiemThu.PaymentId}/refund",
            new HoanTienDto { SoTienHoan = 100_000, MaHoanTien = $"REFUND-{Guid.NewGuid():N}" });

        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            Assert.Equal(0, await nguCanhDuLieu.DanhSachLoaiVe.Where(loaiVeDb => loaiVeDb.LoaiVeId == kichBanKiemThu.TicketTypeId).Select(loaiVeDb => loaiVeDb.SoLuongDaBan).SingleAsync());
            Assert.Equal(TrangThaiVe.DaHoanTien,
                await nguCanhDuLieu.DanhSachVe.Where(veDaPhatHanhDb => veDaPhatHanhDb.VeId == kichBanKiemThu.TicketId).Select(veDaPhatHanhDb => veDaPhatHanhDb.TrangThai).SingleAsync());
        });
    }

    [Fact(DisplayName = "POST /api/payments/{id}/refund - chặn vé đã check-in")]
    public async Task Post_api_payments_id_refund_rejects_checked_in_ticket()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(
            mayChuKiemThu,
            trangThaiDonVe: TrangThaiDon.ThanhCong,
            trangThaiVe: TrangThaiVe.DaCheckIn);

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/payments/{kichBanKiemThu.PaymentId}/refund",
            new HoanTienDto { SoTienHoan = 100_000, MaHoanTien = $"REFUND-{Guid.NewGuid():N}" });

        Assert.Equal(HttpStatusCode.Conflict, phanHoiYeuCauHttp.StatusCode);
    }

    private static PayOsWebhook TaoWebhookThanhToan(int donVeId, decimal soTienThanhToan, string maThamChieuThanhToan, string maLienKetThanhToan)
    {
        var duLieuWebhookJson = JsonSerializer.SerializeToElement(new
        {
            orderCode = donVeId,
            amount = soTienThanhToan,
            reference = maThamChieuThanhToan,
            paymentLinkId = maLienKetThanhToan,
            code = "00",
            desc = "success"
        });
        return new PayOsWebhook
        {
            MaKetQua = "00",
            MoTaKetQua = "success",
            ThanhCong = true,
            DuLieuWebhook = duLieuWebhookJson,
            ChuKyWebhook = KyDuLieuWebhook(duLieuWebhookJson)
        };
    }

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
}
