using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.QuanTri;
using QLSK.Features.SuKien;
using Xunit;

namespace QLSK.Tests;

public class OperationRefactorTests(ApiFactory mayChuKiemThu) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("lock", false)]
    [InlineData("unlock", true)]
    public async Task Account_operations_set_opposite_activity_states(string thaoTacCanThucThi, bool hoatDongMongDoi)
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => { (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.UserId))!.HoatDong = !hoatDongMongDoi; await nguCanhDuLieu.SaveChangesAsync(); });
        var phanHoiYeuCauHttp = await TaoClientQuanTriKiemThu(kichBanKiemThu).PostAsync($"/api/accounts/{kichBanKiemThu.UserId}/{thaoTacCanThucThi}", null);
        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.Equal(hoatDongMongDoi, (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.UserId))!.HoatDong));
        var phanHoiHoSoTaiKhoan = await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung").GetAsync("/api/accounts/me");
        Assert.Equal(hoatDongMongDoi ? HttpStatusCode.OK : HttpStatusCode.Forbidden, phanHoiHoSoTaiKhoan.StatusCode);
    }

    [Theory]
    [InlineData("approve", TrangThaiDatCho.DaXacNhan)]
    [InlineData("reject", TrangThaiDatCho.TuChoi)]
    public async Task Booking_decisions_keep_pending_guard(string thaoTacCanThucThi, TrangThaiDatCho trangThaiMongDoi)
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, trangThaiDatDiaDiem: TrangThaiDatCho.ChoXacNhan);
        var clientYeuCauHttp = mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien");
        var duongDanTep = $"/api/venue-bookings/{kichBanKiemThu.BookingId}/{thaoTacCanThucThi}";
        Assert.Equal(HttpStatusCode.OK, (await clientYeuCauHttp.PostAsync(duongDanTep, null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await clientYeuCauHttp.PostAsync(duongDanTep, null)).StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.Equal(trangThaiMongDoi, (await nguCanhDuLieu.DanhSachDatDiaDiem.FindAsync(kichBanKiemThu.BookingId))!.TrangThai));
    }

    [Fact]
    public async Task Organizer_create_grants_role_but_update_preserves_existing_role()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var duLieuQuanTriToChuc = new QuanTriToChucDto { TaiKhoanId = kichBanKiemThu.UserId, TenBanToChuc = "Tổ chức mới", MoTaBanToChuc = "Hồ sơ" };
        var clientYeuCauHttp = TaoClientQuanTriKiemThu(kichBanKiemThu);
        var phanHoiYeuCauHttp = await clientYeuCauHttp.PostAsJsonAsync("/api/admin/catalog/organizers", duLieuQuanTriToChuc);
        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        using var taiLieuJsonPhanHoi = JsonDocument.Parse(await phanHoiYeuCauHttp.Content.ReadAsStringAsync());
        var banGhiId = taiLieuJsonPhanHoi.RootElement.GetProperty("data").GetProperty("id").GetInt32();
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var taiKhoanChuSoHuu = (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.UserId))!;
            Assert.Equal(2, taiKhoanChuSoHuu.VaiTroId);
            taiKhoanChuSoHuu.VaiTroId = 1;
            await nguCanhDuLieu.SaveChangesAsync();
        });
        duLieuQuanTriToChuc.TenBanToChuc = "Tên đã sửa";
        Assert.Equal(HttpStatusCode.OK, (await clientYeuCauHttp.PutAsJsonAsync($"/api/admin/catalog/organizers/{banGhiId}", duLieuQuanTriToChuc)).StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            Assert.Equal(1, (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.UserId))!.VaiTroId);
            Assert.Equal(duLieuQuanTriToChuc.TenBanToChuc, (await nguCanhDuLieu.DanhSachBanToChuc.FindAsync(banGhiId))!.TenBanToChuc);
        });
        duLieuQuanTriToChuc.TaiKhoanId = kichBanKiemThu.ManagerId;
        Assert.Equal(HttpStatusCode.Conflict, (await clientYeuCauHttp.PutAsJsonAsync($"/api/admin/catalog/organizers/{banGhiId}", duLieuQuanTriToChuc)).StatusCode);
    }

    [Theory]
    [InlineData("venues")]
    [InlineData("event-types")]
    [InlineData("ticket-types")]
    [InlineData("organizers")]
    public async Task Typed_delete_operations_remove_only_unused_records(string loaiDanhMuc)
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var banGhiId = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            object entityCanXuLy = loaiDanhMuc switch
            {
                "venues" => new DiaDiem { TenDiaDiem = "Địa điểm chưa dùng", DiaChi = "Địa chỉ", SucChua = 10 },
                "event-types" => new LoaiSuKien { TenLoaiSuKien = $"Chưa dùng-{Guid.NewGuid():N}" },
                "ticket-types" => new LoaiVe { SuKienId = kichBanKiemThu.EventId, TenLoaiVe = "Hạng vé chưa dùng", SoLuong = 1, ThoiGianMoBan = DateTime.UtcNow, ThoiGianDongBan = DateTime.UtcNow.AddDays(1) },
                _ => new BanToChuc { TaiKhoanId = kichBanKiemThu.UserId, TenBanToChuc = "Tổ chức chưa dùng" }
            };
            if (loaiDanhMuc == "organizers") (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.UserId))!.VaiTroId = 2;
            nguCanhDuLieu.Add(entityCanXuLy); await nguCanhDuLieu.SaveChangesAsync();
            return (int)nguCanhDuLieu.Entry(entityCanXuLy).Property(nguCanhDuLieu.Entry(entityCanXuLy).Metadata.FindPrimaryKey()!.Properties.Single().Name).CurrentValue!;
        });
        var clientYeuCauHttp = TaoClientQuanTriKiemThu(kichBanKiemThu);
        Assert.Equal(HttpStatusCode.OK, (await clientYeuCauHttp.DeleteAsync($"/api/admin/catalog/{loaiDanhMuc}/{banGhiId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await clientYeuCauHttp.DeleteAsync($"/api/admin/catalog/{loaiDanhMuc}/{banGhiId}")).StatusCode);
        if (loaiDanhMuc == "organizers")
            await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.Equal(1, (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.UserId))!.VaiTroId));
    }

    [Fact]
    public async Task Unknown_catalog_kind_keeps_not_found_contract()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var phanHoiYeuCauHttp = await TaoClientQuanTriKiemThu(kichBanKiemThu).DeleteAsync($"/api/admin/catalog/unknown/{kichBanKiemThu.EventId}");
        Assert.Equal(HttpStatusCode.NotFound, phanHoiYeuCauHttp.StatusCode);
        using var taiLieuJsonPhanHoi = JsonDocument.Parse(await phanHoiYeuCauHttp.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", taiLieuJsonPhanHoi.RootElement.GetProperty("code").GetString());
    }

    [Theory]
    [InlineData(TrangThaiSuKien.DaDuyet, -1, 1, TrangThaiSuKien.DangDienRa)]
    [InlineData(TrangThaiSuKien.DaDuyet, -2, -1, TrangThaiSuKien.DaKetThuc)]
    [InlineData(TrangThaiSuKien.DangDienRa, -2, -1, TrangThaiSuKien.DaKetThuc)]
    [InlineData(TrangThaiSuKien.DaDuyet, 1, 2, TrangThaiSuKien.DaDuyet)]
    [InlineData(TrangThaiSuKien.ChoDuyet, -2, -1, TrangThaiSuKien.ChoDuyet)]
    [InlineData(TrangThaiSuKien.TuChoi, -2, -1, TrangThaiSuKien.TuChoi)]
    [InlineData(TrangThaiSuKien.DaHuy, -2, -1, TrangThaiSuKien.DaHuy)]
    [InlineData(TrangThaiSuKien.DaKetThuc, -2, -1, TrangThaiSuKien.DaKetThuc)]
    public async Task Scheduled_lifecycle_only_transitions_eligible_events(TrangThaiSuKien duLieuBanDau, int gioBatDauThue, int gioKetThucThue, TrangThaiSuKien trangThaiMongDoi)
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, duLieuBanDau);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var suKien = (await nguCanhDuLieu.DanhSachSuKien.FindAsync(kichBanKiemThu.EventId))!;
            suKien.ThoiGianBatDau = DateTime.UtcNow.AddHours(gioBatDauThue); suKien.ThoiGianKetThuc = DateTime.UtcNow.AddHours(gioKetThucThue);
            await nguCanhDuLieu.SaveChangesAsync();
        });
        using var phamViDichVu = mayChuKiemThu.Services.CreateScope();
        var dichVuSuKien = phamViDichVu.ServiceProvider.GetRequiredService<SuKienService>();
        await dichVuSuKien.CapNhatVongDoiSuKienTheoThoiGian(CancellationToken.None);
        await dichVuSuKien.CapNhatVongDoiSuKienTheoThoiGian(CancellationToken.None);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.Equal(trangThaiMongDoi, (await nguCanhDuLieu.DanhSachSuKien.FindAsync(kichBanKiemThu.EventId))!.TrangThai));
    }

    [Fact]
    public async Task Editing_rejected_event_returns_it_to_pending()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, TrangThaiSuKien.TuChoi);
        var duLieuTaoSuKien = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var suKien = (await nguCanhDuLieu.DanhSachSuKien.FindAsync(kichBanKiemThu.EventId))!;
            return new TaoSuKienDto { TenSuKien = "Gửi lại sau sửa", MoTaSuKien = suKien.MoTaSuKien, LoaiSuKienId = kichBanKiemThu.CategoryId, ThoiGianBatDau = suKien.ThoiGianBatDau, ThoiGianKetThuc = suKien.ThoiGianKetThuc, SoNguoi = suKien.SoNguoi };
        });
        Assert.Equal(HttpStatusCode.OK, (await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.OrganizerUserId, "BanToChuc").PutAsJsonAsync($"/api/events/{kichBanKiemThu.EventId}", duLieuTaoSuKien)).StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.Equal(TrangThaiSuKien.ChoDuyet, (await nguCanhDuLieu.DanhSachSuKien.FindAsync(kichBanKiemThu.EventId))!.TrangThai));
    }

    [Fact]
    public async Task Check_in_statistics_does_not_check_in_tickets()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu, TrangThaiSuKien.DangDienRa);
        Assert.Equal(HttpStatusCode.OK, (await TaoClientQuanTriKiemThu(kichBanKiemThu).GetAsync($"/api/admin/dashboard/events/{kichBanKiemThu.EventId}/check-in")).StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var veDaPhatHanh = (await nguCanhDuLieu.DanhSachVe.FindAsync(kichBanKiemThu.TicketId))!;
            Assert.Equal(TrangThaiVe.ChuaSuDung, veDaPhatHanh.TrangThai); Assert.Null(veDaPhatHanh.NgayCheckIn);
        });
    }

    private HttpClient TaoClientQuanTriKiemThu(TestScenario kichBanKiemThu) => mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien");
}
