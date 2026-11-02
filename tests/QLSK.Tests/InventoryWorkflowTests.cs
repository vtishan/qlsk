using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLSK.Data;
using QLSK.Domain.Enums;
using QLSK.Features.DonVe;
using Xunit;

namespace QLSK.Tests;

public class InventoryWorkflowTests(ApiFactory mayChuKiemThu) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Expiry_releases_only_expired_pending_reservations_once()
    {
        var donHetHan = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var donConHan = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var donDaThanhToan = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiDonVe: TrangThaiDon.ThanhCong);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            (await nguCanhDuLieu.DanhSachDonVe.FindAsync(donHetHan.OrderId))!.ThoiGianHetHan = DateTime.UtcNow.AddMinutes(-1);
            (await nguCanhDuLieu.DanhSachDonVe.FindAsync(donDaThanhToan.OrderId))!.ThoiGianHetHan = DateTime.UtcNow.AddMinutes(-1);
            await nguCanhDuLieu.SaveChangesAsync();
        });

        using (var phamViDichVu = mayChuKiemThu.Services.CreateScope())
            Assert.Equal(1, await phamViDichVu.ServiceProvider.GetRequiredService<DonVeService>().XuLyCacDonVeHetHan());
        using (var phamViDichVu = mayChuKiemThu.Services.CreateScope())
            Assert.Equal(0, await phamViDichVu.ServiceProvider.GetRequiredService<DonVeService>().XuLyCacDonVeHetHan());

        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            Assert.Equal(TrangThaiDon.HetHan, (await nguCanhDuLieu.DanhSachDonVe.FindAsync(donHetHan.OrderId))!.TrangThai);
            Assert.Equal(0, (await nguCanhDuLieu.DanhSachLoaiVe.FindAsync(donHetHan.TicketTypeId))!.SoLuongDangGiu);
            Assert.Equal(TrangThaiDon.ChoThanhToan, (await nguCanhDuLieu.DanhSachDonVe.FindAsync(donConHan.OrderId))!.TrangThai);
            Assert.Equal(1, (await nguCanhDuLieu.DanhSachLoaiVe.FindAsync(donConHan.TicketTypeId))!.SoLuongDangGiu);
            Assert.Equal(TrangThaiDon.ThanhCong, (await nguCanhDuLieu.DanhSachDonVe.FindAsync(donDaThanhToan.OrderId))!.TrangThai);
            Assert.Equal(1, (await nguCanhDuLieu.DanhSachLoaiVe.FindAsync(donDaThanhToan.TicketTypeId))!.SoLuongDaBan);
        });
    }

    [Fact]
    public async Task Cancellation_releases_inventory_once_and_rejects_a_stale_row_version()
    {
        var kichBan = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        using var scopeCu = mayChuKiemThu.Services.CreateScope();
        var nguCanhDuLieuTruocDo = scopeCu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var loaiVeCu = (await nguCanhDuLieuTruocDo.DanhSachLoaiVe.FindAsync(kichBan.TicketTypeId))!;
        var rowVersionCu = loaiVeCu.PhienBanDong.ToArray();
        var clientYeuCauHttp = mayChuKiemThu.CreateAuthenticatedClient(kichBan.UserId, "NguoiDung");
        var duongDanHuy = $"/api/orders/{kichBan.OrderId}/cancel";

        Assert.Equal(HttpStatusCode.OK, (await clientYeuCauHttp.PostAsync(duongDanHuy, null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await clientYeuCauHttp.PostAsync(duongDanHuy, null)).StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var loaiVe = (await nguCanhDuLieu.DanhSachLoaiVe.FindAsync(kichBan.TicketTypeId))!;
            Assert.Equal(0, loaiVe.SoLuongDangGiu);
            Assert.Equal(0, loaiVe.SoLuongDaBan);
            Assert.False(rowVersionCu.SequenceEqual(loaiVe.PhienBanDong));
            Assert.Equal(TrangThaiDon.DaHuy, (await nguCanhDuLieu.DanhSachDonVe.FindAsync(kichBan.OrderId))!.TrangThai);
        });

        loaiVeCu.SoLuong += 1;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => nguCanhDuLieuTruocDo.SaveChangesAsync());
    }

    [Fact]
    public async Task Account_search_keeps_the_original_q_query_parameter()
    {
        var kichBan = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        string emailTaiKhoan = "";
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => emailTaiKhoan = (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBan.UserId))!.Email);
        var clientYeuCauHttp = mayChuKiemThu.CreateAuthenticatedClient(kichBan.AdminId, "QuanTriVien");
        var phanHoi = await clientYeuCauHttp.GetAsync("/api/accounts?q=" + Uri.EscapeDataString(emailTaiKhoan));
        Assert.Equal(HttpStatusCode.OK, phanHoi.StatusCode);
        using var noiDung = JsonDocument.Parse(await phanHoi.Content.ReadAsStringAsync());
        var taiKhoan = Assert.Single(noiDung.RootElement.GetProperty("data").EnumerateArray());
        Assert.Equal(kichBan.UserId, taiKhoan.GetProperty("id").GetInt32());
    }
}
