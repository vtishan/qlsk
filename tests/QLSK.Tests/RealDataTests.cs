using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.QuanTri;
using QLSK.Features.LoaiVe;
using QLSK.Setup;
using Xunit;

namespace QLSK.Tests;

public class RealDataTests(ApiFactory mayChuKiemThu) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Administrator_edits_approved_imported_event_and_public_reads_database_changes()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var duLieuQuanTriSuKien = await TaoDuLieuSuKien(kichBanKiemThu.EventId);
        duLieuQuanTriSuKien.TenSuKien = "Nội dung quản trị vừa cập nhật"; duLieuQuanTriSuKien.HinhAnh = "/assets/images/events/event-002.jpg";
        var phanHoiYeuCauHttp = await TaoClientQuanTriKiemThu(kichBanKiemThu).PutAsJsonAsync($"/api/admin/catalog/events/{kichBanKiemThu.EventId}", duLieuQuanTriSuKien);
        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        using var taiLieuJsonPhanHoi = JsonDocument.Parse(await mayChuKiemThu.CreateClient().GetStringAsync($"/api/events/{kichBanKiemThu.EventId}"));
        Assert.Equal(duLieuQuanTriSuKien.TenSuKien, taiLieuJsonPhanHoi.RootElement.GetProperty("data").GetProperty("ten").GetString());
        Assert.Equal(duLieuQuanTriSuKien.HinhAnh, taiLieuJsonPhanHoi.RootElement.GetProperty("data").GetProperty("hinhAnh").GetString());
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.Equal(TrangThaiSuKien.DaDuyet, (await nguCanhDuLieu.DanhSachSuKien.FindAsync(kichBanKiemThu.EventId))!.TrangThai));
    }

    [Fact]
    public async Task Booked_schedule_and_sold_capacity_cannot_be_overwritten()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var duLieuQuanTriSuKien = await TaoDuLieuSuKien(kichBanKiemThu.EventId); duLieuQuanTriSuKien.ThoiGianBatDau = duLieuQuanTriSuKien.ThoiGianBatDau.AddDays(1); duLieuQuanTriSuKien.ThoiGianKetThuc = duLieuQuanTriSuKien.ThoiGianKetThuc.AddDays(1);
        var phanHoiYeuCauHttp = await TaoClientQuanTriKiemThu(kichBanKiemThu).PutAsJsonAsync($"/api/admin/catalog/events/{kichBanKiemThu.EventId}", duLieuQuanTriSuKien);
        Assert.Equal(HttpStatusCode.Conflict, phanHoiYeuCauHttp.StatusCode);
    }

    [Theory]
    [InlineData("events")]
    [InlineData("venues")]
    [InlineData("event-types")]
    [InlineData("ticket-types")]
    [InlineData("organizers")]
    public async Task Linked_records_cannot_be_deleted(string loaiDanhMuc)
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var banGhiId = loaiDanhMuc switch { "events" => kichBanKiemThu.EventId, "venues" => kichBanKiemThu.VenueId, "event-types" => kichBanKiemThu.CategoryId, "ticket-types" => kichBanKiemThu.TicketTypeId,
            _ => await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => nguCanhDuLieu.DanhSachBanToChuc.Where(banToChucDb => banToChucDb.TaiKhoanId == kichBanKiemThu.OrganizerUserId).Select(banToChucDb => banToChucDb.BanToChucId).SingleAsync()) };
        Assert.Equal(HttpStatusCode.Conflict, (await TaoClientQuanTriKiemThu(kichBanKiemThu).DeleteAsync($"/api/admin/catalog/{loaiDanhMuc}/{banGhiId}")).StatusCode);
    }

    [Theory]
    [InlineData("NguoiDung", 1)]
    [InlineData("BanToChuc", 2)]
    [InlineData("QuanLySuKien", 3)]
    public async Task Catalog_mutations_are_admin_only(string tenVaiTro, int vaiTroId)
    {
        var taiKhoanId = await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => ApiTestData.AddUserAsync(nguCanhDuLieu, vaiTroId));
        var clientYeuCauHttp = mayChuKiemThu.CreateAuthenticatedClient(taiKhoanId, tenVaiTro);
        Assert.Equal(HttpStatusCode.Forbidden, (await clientYeuCauHttp.GetAsync("/api/admin/catalog")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await clientYeuCauHttp.DeleteAsync("/api/admin/catalog/events/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await clientYeuCauHttp.PostAsJsonAsync("/api/admin/catalog/events", new QuanTriSuKienDto())).StatusCode);
    }

    [Fact]
    public async Task Unused_event_can_be_created_and_deleted_without_seeding_other_data()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var duLieuQuanTriSuKien = await TaoDuLieuSuKien(kichBanKiemThu.EventId);
        var phanHoiYeuCauHttp = await TaoClientQuanTriKiemThu(kichBanKiemThu).PostAsJsonAsync("/api/admin/catalog/events", duLieuQuanTriSuKien);
        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
        using var taiLieuJsonPhanHoi = JsonDocument.Parse(await phanHoiYeuCauHttp.Content.ReadAsStringAsync());
        var banGhiId = taiLieuJsonPhanHoi.RootElement.GetProperty("data").GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK, (await TaoClientQuanTriKiemThu(kichBanKiemThu).DeleteAsync($"/api/admin/catalog/events/{banGhiId}")).StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.False(await nguCanhDuLieu.DanhSachSuKien.AnyAsync(suKienDb => suKienDb.SuKienId == banGhiId)));
    }

    [Fact]
    public async Task Administrator_can_manage_existing_ticket_types_but_cannot_reduce_reserved_inventory()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var duLieuLoaiVe = new LoaiVeDto { TenLoaiVe = "Vé đã chỉnh sửa", GiaVe = 220000, SoLuong = 120, ThoiGianMoBan = DateTime.UtcNow.AddDays(-1), ThoiGianDongBan = DateTime.UtcNow.AddDays(4) };
        Assert.Equal(HttpStatusCode.OK, (await TaoClientQuanTriKiemThu(kichBanKiemThu).PutAsJsonAsync($"/api/ticket-types/{kichBanKiemThu.TicketTypeId}", duLieuLoaiVe)).StatusCode);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.Equal(duLieuLoaiVe.GiaVe, (await nguCanhDuLieu.DanhSachLoaiVe.FindAsync(kichBanKiemThu.TicketTypeId))!.GiaVe));
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => { (await nguCanhDuLieu.DanhSachLoaiVe.FindAsync(kichBanKiemThu.TicketTypeId))!.SoLuongDangGiu = 10; await nguCanhDuLieu.SaveChangesAsync(); });
        duLieuLoaiVe.SoLuong = 1;
        Assert.Equal(HttpStatusCode.Conflict, (await TaoClientQuanTriKiemThu(kichBanKiemThu).PutAsJsonAsync($"/api/ticket-types/{kichBanKiemThu.TicketTypeId}", duLieuLoaiVe)).StatusCode);
    }

    [Fact]
    public async Task Activity_filters_by_account_foreign_key_and_export_history_uses_real_records()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        using var taiLieuJsonPhanHoi = JsonDocument.Parse(await TaoClientQuanTriKiemThu(kichBanKiemThu).GetStringAsync($"/api/admin/catalog/accounts/{kichBanKiemThu.OrganizerUserId}/activity"));
        Assert.Contains(taiLieuJsonPhanHoi.RootElement.GetProperty("data").GetProperty("suKien").EnumerateArray(), phanTuJson => phanTuJson.GetProperty("id").GetInt32() == kichBanKiemThu.EventId);
        using var danhSachDonVeKiemThu = JsonDocument.Parse(await TaoClientQuanTriKiemThu(kichBanKiemThu).GetStringAsync($"/api/admin/catalog/accounts/{kichBanKiemThu.UserId}/activity"));
        Assert.Contains(danhSachDonVeKiemThu.RootElement.GetProperty("data").GetProperty("donVe").EnumerateArray(), phanTuJson => phanTuJson.GetProperty("id").GetInt32() == kichBanKiemThu.OrderId);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => { nguCanhDuLieu.LichSuXuatVe.Add(new XuatVe { VeId = kichBanKiemThu.TicketId, HinhThucXuat = "Email", ThanhCong = false }); await nguCanhDuLieu.SaveChangesAsync(); });
        var clientYeuCauHttp = mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.UserId, "NguoiDung");
        using var taiLieuLichSuXuatVe = JsonDocument.Parse(await clientYeuCauHttp.GetStringAsync($"/api/tickets/{kichBanKiemThu.TicketId}/exports"));
        var banGhiDangXuLy = Assert.Single(taiLieuLichSuXuatVe.RootElement.GetProperty("data").EnumerateArray());
        Assert.Equal("Email", banGhiDangXuLy.GetProperty("kieu").GetString()); Assert.False(banGhiDangXuLy.GetProperty("thanhCong").GetBoolean());
        Assert.Equal(HttpStatusCode.Forbidden, (await mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.ManagerId, "QuanLySuKien").GetAsync($"/api/tickets/{kichBanKiemThu.TicketId}/exports")).StatusCode);
    }

    [Fact]
    public async Task Terminal_admin_update_hashes_password_and_revokes_existing_session()
    {
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThu);
        var clientTruocThayDoi = TaoClientQuanTriKiemThu(kichBanKiemThu);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(nguCanhDuLieu => AdminProvisioner.UpdateAdminCredentialsAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), kichBanKiemThu.AdminId, $"changed-{Guid.NewGuid():N}@customer.test", "lowercase123!password"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await clientTruocThayDoi.GetAsync("/api/admin/catalog")).StatusCode);
    }

    [Fact]
    public async Task Adoption_preserves_business_keys_and_revokes_old_credentials_idempotently()
    {
        using var mayChuKiemThuRieng = new ApiFactory();
        var kichBanKiemThu = await ApiTestData.TaoKichBanKiemThuAsync(mayChuKiemThuRieng);
        var boBamMatKhau = new PasswordHasher<TaiKhoan>();
        await mayChuKiemThuRieng.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.AdminId))!.Email = "admin@qlsk.local";
            (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.OrganizerUserId))!.Email = "demo.btc01@qlsk.local";
            await nguCanhDuLieu.SaveChangesAsync();
        });
        await mayChuKiemThuRieng.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu => Assert.Equal(1, await LegacyDataMigrator.AdoptLegacyAccountsAsync(nguCanhDuLieu, boBamMatKhau, "owner@customer.test", "lowercase123!password")));
        await mayChuKiemThuRieng.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            Assert.Equal(0, await LegacyDataMigrator.AdoptLegacyAccountsAsync(nguCanhDuLieu, boBamMatKhau, "owner@customer.test", "lowercase123!password"));
            Assert.Equal(1, await nguCanhDuLieu.DanhSachSuKien.CountAsync()); Assert.Equal(1, await nguCanhDuLieu.DanhSachDonVe.CountAsync());
            var taiKhoanChuSoHuu = (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.OrganizerUserId))!;
            Assert.False(taiKhoanChuSoHuu.HoatDong); Assert.EndsWith("@disabled.invalid", taiKhoanChuSoHuu.Email);
            Assert.Equal(kichBanKiemThu.EventId, (await nguCanhDuLieu.DanhSachSuKien.SingleAsync()).SuKienId);
            var taiKhoanQuanTri = (await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(kichBanKiemThu.AdminId))!;
            Assert.NotEqual(PasswordVerificationResult.Failed, boBamMatKhau.VerifyHashedPassword(taiKhoanQuanTri, taiKhoanQuanTri.MatKhauBam, "lowercase123!password"));
        });
    }

    private HttpClient TaoClientQuanTriKiemThu(TestScenario kichBanKiemThu) => mayChuKiemThu.CreateAuthenticatedClient(kichBanKiemThu.AdminId, "QuanTriVien");
    private Task<QuanTriSuKienDto> TaoDuLieuSuKien(int suKienId) => mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
    {
        var suKienDangXuLy = (await nguCanhDuLieu.DanhSachSuKien.AsNoTracking().SingleAsync(suKienDb => suKienDb.SuKienId == suKienId));
        return new QuanTriSuKienDto { BanToChucId = suKienDangXuLy.BanToChucId, LoaiSuKienId = suKienDangXuLy.LoaiSuKienId,
            TenSuKien = suKienDangXuLy.TenSuKien, MoTaSuKien = suKienDangXuLy.MoTaSuKien, HinhAnh = suKienDangXuLy.HinhAnh, ThoiGianBatDau = suKienDangXuLy.ThoiGianBatDau, ThoiGianKetThuc = suKienDangXuLy.ThoiGianKetThuc, SoNguoi = suKienDangXuLy.SoNguoi };
    });
}
