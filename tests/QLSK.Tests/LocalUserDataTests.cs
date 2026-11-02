using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.LocalData;
using Xunit;

namespace QLSK.Tests;

public class LocalUserDataTests
{
    private const string Password = "LocalUserTest!2026";
    private static readonly DateTime ThoiDiemKiemThu = new(2026, 11, 2, 3, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Creates_100_unique_users_with_consistent_orders_payments_tickets_and_inventory()
    {
        using var mayChuKiemThu = new ApiFactory();
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            await TaoSuKienKiemThu(nguCanhDuLieu);
            var boBamMatKhau = new PasswordHasher<TaiKhoan>();
            var ketQuaThucHien = await LocalUserSeeder.TaoDuLieuNguoiDungMauAsync(nguCanhDuLieu, boBamMatKhau, Password, ThoiDiemKiemThu);
            Assert.Equal(new UserSeedResult(100, 100, 100, 118, 0), ketQuaThucHien);
            nguCanhDuLieu.ChangeTracker.Clear();

            var danhSachTaiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.Where(taiKhoanDb => taiKhoanDb.VaiTroId == 1).OrderBy(taiKhoanDb => taiKhoanDb.Email).ToListAsync();
            Assert.Equal(100, danhSachTaiKhoan.Count);
            Assert.Equal(100, danhSachTaiKhoan.Select(taiKhoanDb => taiKhoanDb.HoTen).Distinct().Count());
            Assert.Equal(100, danhSachTaiKhoan.Select(taiKhoanDb => taiKhoanDb.Email).Distinct().Count());
            Assert.Equal(100, danhSachTaiKhoan.Select(taiKhoanDb => taiKhoanDb.MatKhauBam).Distinct().Count());
            Assert.Equal(91, danhSachTaiKhoan.Count(taiKhoanDb => taiKhoanDb.HoatDong));
            Assert.Equal(9, danhSachTaiKhoan.Count(taiKhoanDb => !taiKhoanDb.HoatDong));
            Assert.All(danhSachTaiKhoan, taiKhoanDb => Assert.EndsWith("@sample.qlsk.test", taiKhoanDb.Email));
            Assert.NotEqual(PasswordVerificationResult.Failed, boBamMatKhau.VerifyHashedPassword(danhSachTaiKhoan[0], danhSachTaiKhoan[0].MatKhauBam, Password));
            Assert.Empty(await nguCanhDuLieu.DanhSachTaiKhoan.Where(taiKhoanDb => taiKhoanDb.VaiTroId == 4).ToListAsync());

            var danhSachDonVeKiemThu = await nguCanhDuLieu.DanhSachDonVe.Include(donVeDb => donVeDb.TaiKhoan).Include(donVeDb => donVeDb.ChiTietDonVe).ThenInclude(chiTietDonVeDb => chiTietDonVeDb.LoaiVe)
                .Include(donVeDb => donVeDb.DanhSachThanhToan).Include(donVeDb => donVeDb.DanhSachVe).ToListAsync();
            Assert.Equal(100, danhSachDonVeKiemThu.Count);
            Assert.Equal(50, danhSachDonVeKiemThu.Count(donVeDb => donVeDb.TrangThai == TrangThaiDon.ThanhCong));
            Assert.Equal(20, danhSachDonVeKiemThu.Count(donVeDb => donVeDb.TrangThai == TrangThaiDon.ChoThanhToan));
            Assert.Equal(10, danhSachDonVeKiemThu.Count(donVeDb => donVeDb.TrangThai == TrangThaiDon.DaHuy));
            Assert.Equal(10, danhSachDonVeKiemThu.Count(donVeDb => donVeDb.TrangThai == TrangThaiDon.HetHan));
            Assert.Equal(5, danhSachDonVeKiemThu.Count(donVeDb => donVeDb.TrangThai == TrangThaiDon.YeuCauHoanTien));
            Assert.Equal(5, danhSachDonVeKiemThu.Count(donVeDb => donVeDb.TrangThai == TrangThaiDon.DaHoanTien));
            foreach (var donVeDangXuLy in danhSachDonVeKiemThu)
            {
                var chiTietSuKien = Assert.Single(donVeDangXuLy.ChiTietDonVe);
                var thanhToanKiemThu = Assert.Single(donVeDangXuLy.DanhSachThanhToan);
                Assert.Equal(donVeDangXuLy.SoLuong, chiTietSuKien.SoLuong);
                Assert.Equal(donVeDangXuLy.TongTien, chiTietSuKien.DonGia * chiTietSuKien.SoLuong);
                Assert.Equal(donVeDangXuLy.TongTien, chiTietSuKien.ThanhTien);
                Assert.Equal(donVeDangXuLy.TongTien, thanhToanKiemThu.SoTien);
                Assert.Equal(donVeDangXuLy.SuKienId, chiTietSuKien.LoaiVe.SuKienId);
                Assert.True(donVeDangXuLy.TaiKhoan.NgayTao <= donVeDangXuLy.NgayTao && donVeDangXuLy.NgayTao >= chiTietSuKien.LoaiVe.ThoiGianMoBan);
                Assert.InRange(thanhToanKiemThu.SoTienHoan, 0, thanhToanKiemThu.SoTien);
                var daThanhToan = thanhToanKiemThu.TrangThai == TrangThaiThanhToan.ThanhCong;
                Assert.Equal(daThanhToan ? donVeDangXuLy.SoLuong : 0, donVeDangXuLy.DanhSachVe.Count);
                if (daThanhToan) Assert.True(thanhToanKiemThu.NgayThanhToan < donVeDangXuLy.ThoiGianHetHan);
                if (donVeDangXuLy.TrangThai == TrangThaiDon.ChoThanhToan) Assert.True(donVeDangXuLy.ThoiGianHetHan > ThoiDiemKiemThu);
                if (donVeDangXuLy.TrangThai == TrangThaiDon.HetHan) Assert.True(donVeDangXuLy.ThoiGianHetHan < ThoiDiemKiemThu);
                if (donVeDangXuLy.TrangThai == TrangThaiDon.DaHoanTien)
                {
                    Assert.Equal(donVeDangXuLy.TongTien, thanhToanKiemThu.SoTienHoan);
                    Assert.All(donVeDangXuLy.DanhSachVe, veDaPhatHanhDb => Assert.Equal(TrangThaiVe.DaHoanTien, veDaPhatHanhDb.TrangThai));
                }
            }

            var danhSachVeKiemThu = await nguCanhDuLieu.DanhSachVe.ToListAsync();
            Assert.Equal(118, danhSachVeKiemThu.Count);
            Assert.Equal(118, danhSachVeKiemThu.Select(veDaPhatHanhDb => veDaPhatHanhDb.MaVe).Distinct().Count());
            Assert.Equal(118, danhSachVeKiemThu.Select(veDaPhatHanhDb => veDaPhatHanhDb.NoiDungMaQr).Distinct().Count());
            Assert.All(danhSachVeKiemThu, veDaPhatHanhDb => Assert.Null(veDaPhatHanhDb.NgayCheckIn));
            foreach (var loaiVeDangXuLy in await nguCanhDuLieu.DanhSachLoaiVe.ToListAsync())
            {
                Assert.Equal(danhSachVeKiemThu.Count(veDaPhatHanhDb => veDaPhatHanhDb.LoaiVeId == loaiVeDangXuLy.LoaiVeId && veDaPhatHanhDb.TrangThai == TrangThaiVe.ChuaSuDung), loaiVeDangXuLy.SoLuongDaBan);
                Assert.Equal(danhSachDonVeKiemThu.Where(donVeDb => donVeDb.TrangThai == TrangThaiDon.ChoThanhToan)
                    .SelectMany(donVeDb => donVeDb.ChiTietDonVe).Where(chiTietDonVeDb => chiTietDonVeDb.LoaiVeId == loaiVeDangXuLy.LoaiVeId).Sum(chiTietDonVeDb => chiTietDonVeDb.SoLuong), loaiVeDangXuLy.SoLuongDangGiu);
                Assert.InRange(loaiVeDangXuLy.SoLuongDaBan + loaiVeDangXuLy.SoLuongDangGiu, 0, loaiVeDangXuLy.SoLuong);
            }
        });
    }

    [Fact]
    public async Task Repeated_population_preserves_user_edits_passwords_orders_and_inventory()
    {
        using var mayChuKiemThu = new ApiFactory();
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            await TaoSuKienKiemThu(nguCanhDuLieu);
            var boBamMatKhau = new PasswordHasher<TaiKhoan>();
            await LocalUserSeeder.TaoDuLieuNguoiDungMauAsync(nguCanhDuLieu, boBamMatKhau, Password, ThoiDiemKiemThu);
            var taiKhoanNguoiDung = await nguCanhDuLieu.DanhSachTaiKhoan.SingleAsync(taiKhoanDb => taiKhoanDb.Email == LocalUserSeeder.TaoEmailNguoiDungMau(1));
            taiKhoanNguoiDung.HoTen = "Tên người dùng đã chỉnh sửa";
            taiKhoanNguoiDung.HoatDong = false;
            taiKhoanNguoiDung.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanNguoiDung, "ChangedUserPassword!2026");
            await nguCanhDuLieu.SaveChangesAsync();
            var matKhauBam = taiKhoanNguoiDung.MatKhauBam;
            var soLuongTonVe = await nguCanhDuLieu.DanhSachLoaiVe.OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).Select(loaiVeDb => new { Id = loaiVeDb.LoaiVeId, DaBan = loaiVeDb.SoLuongDaBan, DangGiu = loaiVeDb.SoLuongDangGiu }).ToListAsync();
            nguCanhDuLieu.ChangeTracker.Clear();

            var ketQuaThucHien = await LocalUserSeeder.TaoDuLieuNguoiDungMauAsync(nguCanhDuLieu, boBamMatKhau, "DoNotResetPassword!2026", ThoiDiemKiemThu.AddDays(1));
            Assert.Equal(new UserSeedResult(0, 0, 0, 0, 100), ketQuaThucHien);
            taiKhoanNguoiDung = await nguCanhDuLieu.DanhSachTaiKhoan.SingleAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanNguoiDung.TaiKhoanId);
            Assert.Equal("Tên người dùng đã chỉnh sửa", taiKhoanNguoiDung.HoTen);
            Assert.False(taiKhoanNguoiDung.HoatDong);
            Assert.Equal(matKhauBam, taiKhoanNguoiDung.MatKhauBam);
            Assert.Equal(100, await nguCanhDuLieu.DanhSachDonVe.CountAsync());
            Assert.Equal(100, await nguCanhDuLieu.DanhSachThanhToan.CountAsync());
            Assert.Equal(118, await nguCanhDuLieu.DanhSachVe.CountAsync());
            Assert.Equal(soLuongTonVe, await nguCanhDuLieu.DanhSachLoaiVe.OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).Select(loaiVeDb => new { Id = loaiVeDb.LoaiVeId, DaBan = loaiVeDb.SoLuongDaBan, DangGiu = loaiVeDb.SoLuongDangGiu }).ToListAsync());
        });
    }

    [Fact]
    public async Task Insufficient_inventory_does_not_leave_a_partial_batch_or_change_capacity()
    {
        using var mayChuKiemThu = new ApiFactory();
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            await TaoSuKienKiemThu(nguCanhDuLieu, soLuongVeMoiHang: 1);
            nguCanhDuLieu.ChangeTracker.Clear();
            await Assert.ThrowsAsync<InvalidOperationException>(() => LocalUserSeeder.TaoDuLieuNguoiDungMauAsync(nguCanhDuLieu,
                new PasswordHasher<TaiKhoan>(), Password, ThoiDiemKiemThu));
            Assert.DoesNotContain(nguCanhDuLieu.ChangeTracker.Entries(), banGhiTheoDoi => banGhiTheoDoi.State == EntityState.Added || banGhiTheoDoi.State == EntityState.Modified);
            Assert.Empty(await nguCanhDuLieu.DanhSachTaiKhoan.Where(taiKhoanDb => taiKhoanDb.VaiTroId == 1).ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachDonVe.ToListAsync());
            Assert.All(await nguCanhDuLieu.DanhSachLoaiVe.ToListAsync(), loaiVeDb => Assert.Equal(0, loaiVeDb.SoLuongDaBan + loaiVeDb.SoLuongDangGiu));
        });
    }

    private static async Task TaoSuKienKiemThu(QLSK.Data.ApplicationDbContext nguCanhDuLieu, int soLuongVeMoiHang = 200)
    {
        var suKienDangXuLy = new SuKien
        {
            TenSuKien = "Sự kiện cho dữ liệu người dùng", MoTaSuKien = "Integration test", TrangThai = TrangThaiSuKien.DaDuyet,
            ThoiGianBatDau = ThoiDiemKiemThu.AddDays(7), ThoiGianKetThuc = ThoiDiemKiemThu.AddDays(7).AddHours(3), SoNguoi = soLuongVeMoiHang * 2,
            BanToChuc = new BanToChuc
            {
                TenBanToChuc = "Đơn vị kiểm thử", TaiKhoan = new TaiKhoan { Email = "owner@sample.test", HoTen = "Chủ sự kiện", VaiTroId = 2 }
            },
            LoaiSuKien = new LoaiSuKien { TenLoaiSuKien = "Loại kiểm thử" }
        };
        foreach (var tenDoiTuong in new[] { "Tiêu chuẩn", "Đồng hành" })
            suKienDangXuLy.DanhSachLoaiVe.Add(new LoaiVe
            {
                TenLoaiVe = tenDoiTuong, GiaVe = tenDoiTuong == "Tiêu chuẩn" ? 120_000 : 220_000, SoLuong = soLuongVeMoiHang,
                ThoiGianMoBan = ThoiDiemKiemThu.AddDays(-3), ThoiGianDongBan = suKienDangXuLy.ThoiGianBatDau
            });
        nguCanhDuLieu.DanhSachSuKien.Add(suKienDangXuLy);
        await nguCanhDuLieu.SaveChangesAsync();
    }
}
