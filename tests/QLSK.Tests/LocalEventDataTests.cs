using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QLSK.LocalData;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using Xunit;

namespace QLSK.Tests;

public class LocalEventDataTests
{
    [Fact]
    public void Local_population_rejects_the_business_database()
        => Assert.Throws<InvalidOperationException>(() => LocalSqlGuard.KiemTraKetNoiSqlLocal(
            "Server=.\\SQL2022EXP;Database=QLSK;Integrated Security=True", requireTestDatabase: true));

    private const string LegacyDescription = "Đây là sự kiện mẫu phục vụ trải nghiệm hệ thống QLSK.";

    [Theory]
    [InlineData("Server=customer.example.com;Database=QLSK;Integrated Security=True")]
    [InlineData("Server=192.0.2.15;Database=QLSK;Integrated Security=True")]
    [InlineData("Server=localhost;Database=master;Integrated Security=True")]
    [InlineData("Server=localhost;Database=TEMPDB;Integrated Security=True")]
    [InlineData("Server=localhost;Integrated Security=True")]
    [InlineData("Server=localhost;Database=QLSK;AttachDbFilename=C:\\test.mdf;Integrated Security=True")]
    public void Local_data_tool_rejects_remote_system_or_attached_databases(string chuoiKetNoiSql)
        => Assert.Throws<InvalidOperationException>(() => LocalSqlGuard.KiemTraKetNoiSqlLocal(chuoiKetNoiSql));

    [Theory]
    [InlineData("Server=.\\SQL2022EXP;Database=QLSK_Local;Integrated Security=True")]
    [InlineData("Server=tcp:127.0.0.1,1433;Database=QLSK_Local;Integrated Security=True")]
    public void Local_data_tool_accepts_explicit_local_application_databases(string chuoiKetNoiSql)
        => LocalSqlGuard.KiemTraKetNoiSqlLocal(chuoiKetNoiSql);

    [Fact]
    public async Task Application_startup_has_no_business_data_or_local_data_dependency()
    {
        using var mayChuKiemThu = new ApiFactory();
        using var clientYeuCauHttp = mayChuKiemThu.CreateClient();

        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            Assert.Equal(4, await nguCanhDuLieu.DanhSachVaiTro.CountAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachTaiKhoan.ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachBanToChuc.ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachDiaDiem.ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachSuKien.ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachLoaiVe.ToListAsync());
        });

        var thuMucUngDung = typeof(Program).Assembly;
        Assert.NotEqual(thuMucUngDung, typeof(LocalEventSeeder).Assembly);
        Assert.DoesNotContain(thuMucUngDung.GetReferencedAssemblies(), tenAssembly => tenAssembly.Name == "QLSK.LocalData");
        Assert.DoesNotContain(thuMucUngDung.GetTypes(), kieuDuLieu => kieuDuLieu.Namespace == "QLSK.Data.Seed");
        foreach (var duongDanTep in new[] { "/Data/Seed/demo-events.json", "/tests/QLSK.LocalData/catalog/events.json" })
        {
            using var phanHoiYeuCauHttp = await clientYeuCauHttp.GetAsync(duongDanTep);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, phanHoiYeuCauHttp.StatusCode);
        }
    }

    [Fact]
    public async Task Explicit_local_population_is_bookable_and_repeated_runs_preserve_edits()
    {
        using var mayChuKiemThu = new ApiFactory();
        var thuMucDuAn = mayChuKiemThu.Services.GetRequiredService<IHostEnvironment>().ContentRootPath;
        var thoiDiemHienTai = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc);
        var boBamMatKhau = new PasswordHasher<TaiKhoan>();

        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            Assert.Equal(100, await LocalEventSeeder.TaoDuLieuSuKienMauAsync(nguCanhDuLieu, boBamMatKhau, thuMucDuAn, "LocalTest@123456", thoiDiemHienTai));
            nguCanhDuLieu.ChangeTracker.Clear();

            var danhSachSuKienHienCo = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.DanhSachLoaiVe)
                .Include(suKienDb => suKienDb.DatDiaDiem).ThenInclude(datDiaDiemDb => datDiaDiemDb!.DiaDiem)
                .OrderBy(suKienDb => suKienDb.SuKienId).ToListAsync();
            Assert.Equal(100, danhSachSuKienHienCo.Count);
            Assert.All(danhSachSuKienHienCo, suKienDangXuLy => Assert.DoesNotContain(LegacyDescription, suKienDangXuLy.MoTaSuKien));
            Assert.Equal(100, danhSachSuKienHienCo.Select(suKienDb => suKienDb.HinhAnh).Distinct().Count());
            Assert.Equal(10, await nguCanhDuLieu.DanhSachLoaiSuKien.CountAsync());
            Assert.Equal(5, await nguCanhDuLieu.DanhSachBanToChuc.CountAsync());
            Assert.Equal(10, await nguCanhDuLieu.DanhSachDiaDiem.CountAsync());
            Assert.Equal(200, await nguCanhDuLieu.DanhSachLoaiVe.CountAsync());
            Assert.Equal(100, await nguCanhDuLieu.DanhSachDatDiaDiem.CountAsync());

            foreach (var suKienDangXuLy in danhSachSuKienHienCo)
            {
                Assert.Equal(TrangThaiSuKien.DaDuyet, suKienDangXuLy.TrangThai);
                Assert.True(suKienDangXuLy.ThoiGianBatDau > thoiDiemHienTai);
                Assert.True(suKienDangXuLy.ThoiGianKetThuc > suKienDangXuLy.ThoiGianBatDau);
                Assert.NotNull(suKienDangXuLy.HinhAnh);
                Assert.StartsWith("/assets/images/events/event-", suKienDangXuLy.HinhAnh);
                Assert.EndsWith(".jpg", suKienDangXuLy.HinhAnh);
                var duongDanTepAnh = Path.Combine(thuMucDuAn, "wwwroot", suKienDangXuLy.HinhAnh.TrimStart('/'));
                Assert.True(File.Exists(duongDanTepAnh), $"Missing local image {suKienDangXuLy.HinhAnh}");
                using (var duongDanHinhAnh = File.OpenRead(duongDanTepAnh))
                {
                    Assert.Equal(0xff, duongDanHinhAnh.ReadByte());
                    Assert.Equal(0xd8, duongDanHinhAnh.ReadByte());
                }

                var phieuDatDiaDiem = Assert.IsType<DatDiaDiem>(suKienDangXuLy.DatDiaDiem);
                Assert.Equal(TrangThaiDatCho.DaXacNhan, phieuDatDiaDiem.TrangThai);
                Assert.Equal(suKienDangXuLy.ThoiGianBatDau, phieuDatDiaDiem.ThoiGianBatDau);
                Assert.Equal(suKienDangXuLy.ThoiGianKetThuc, phieuDatDiaDiem.ThoiGianKetThuc);
                Assert.True(phieuDatDiaDiem.DiaDiem.SucChua >= suKienDangXuLy.SoNguoi);
                Assert.InRange(phieuDatDiaDiem.TienCoc, 0, phieuDatDiaDiem.TongTien);
                Assert.Equal(suKienDangXuLy.SoNguoi, suKienDangXuLy.DanhSachLoaiVe.Sum(loaiVeDb => loaiVeDb.SoLuong));
                Assert.All(suKienDangXuLy.DanhSachLoaiVe, veKiemThu =>
                {
                    Assert.True(veKiemThu.GiaVe >= 0);
                    Assert.True(veKiemThu.SoLuong > 0);
                    Assert.Equal(0, veKiemThu.SoLuongDaBan);
                    Assert.Equal(0, veKiemThu.SoLuongDangGiu);
                    Assert.True(veKiemThu.ThoiGianMoBan <= thoiDiemHienTai && veKiemThu.ThoiGianDongBan > thoiDiemHienTai);
                    Assert.True(veKiemThu.ThoiGianDongBan <= suKienDangXuLy.ThoiGianBatDau);
                });
            }

            foreach (var nhomDiaDiem in danhSachSuKienHienCo.GroupBy(suKienDb => suKienDb.DatDiaDiem!.DiaDiemId))
            {
                var danhSachPhieuDatDiaDiem = nhomDiaDiem.Select(suKienDb => suKienDb.DatDiaDiem!).OrderBy(datDiaDiemDb => datDiaDiemDb.ThoiGianBatDau).ToArray();
                for (var chiSo = 1; chiSo < danhSachPhieuDatDiaDiem.Length; chiSo++)
                    Assert.True(danhSachPhieuDatDiaDiem[chiSo - 1].ThoiGianKetThuc <= danhSachPhieuDatDiaDiem[chiSo].ThoiGianBatDau);
            }

            var suKienDuocSua = danhSachSuKienHienCo[0];
            var thoiGianBatDauTruocSua = suKienDuocSua.ThoiGianBatDau;
            suKienDuocSua.TenSuKien = "Tên sự kiện do người dùng chỉnh sửa";
            suKienDuocSua.MoTaSuKien = "Nội dung đã chỉnh sửa phải được giữ lại.";
            suKienDuocSua.DanhSachLoaiVe.First().GiaVe = 123_000;
            await nguCanhDuLieu.SaveChangesAsync();
            nguCanhDuLieu.ChangeTracker.Clear();

            Assert.Equal(0, await LocalEventSeeder.TaoDuLieuSuKienMauAsync(nguCanhDuLieu, boBamMatKhau, thuMucDuAn, "NewPassword@123", thoiDiemHienTai.AddDays(1)));
            nguCanhDuLieu.ChangeTracker.Clear();
            var suKienDaLuu = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.DanhSachLoaiVe).SingleAsync(suKienDb => suKienDb.SuKienId == suKienDuocSua.SuKienId);
            Assert.Equal(suKienDuocSua.TenSuKien, suKienDaLuu.TenSuKien);
            Assert.Equal(suKienDuocSua.MoTaSuKien, suKienDaLuu.MoTaSuKien);
            Assert.Equal(thoiGianBatDauTruocSua, suKienDaLuu.ThoiGianBatDau);
            Assert.Contains(suKienDaLuu.DanhSachLoaiVe, loaiVeDb => loaiVeDb.GiaVe == 123_000);
            Assert.Equal(100, await nguCanhDuLieu.DanhSachSuKien.CountAsync());
            Assert.Equal(200, await nguCanhDuLieu.DanhSachLoaiVe.CountAsync());
            Assert.Equal(100, await nguCanhDuLieu.DanhSachDatDiaDiem.CountAsync());
            Assert.Equal(5, await nguCanhDuLieu.DanhSachBanToChuc.CountAsync());
            Assert.Equal(10, await nguCanhDuLieu.DanhSachDiaDiem.CountAsync());
        });
    }

    [Fact]
    public async Task Normalizing_existing_data_preserves_identity_relations_edits_and_passwords()
    {
        using var mayChuKiemThu = new ApiFactory();
        var thuMucDuAn = mayChuKiemThu.Services.GetRequiredService<IHostEnvironment>().ContentRootPath;
        var thoiDiemHienTai = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc);
        await mayChuKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            await LocalEventSeeder.TaoDuLieuSuKienMauAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), thuMucDuAn, "LocalTest@123456", thoiDiemHienTai);
            var taiKhoanCanXuLy = await nguCanhDuLieu.DanhSachTaiKhoan.Include(taiKhoanDb => taiKhoanDb.BanToChuc)
                .SingleAsync(taiKhoanDb => taiKhoanDb.Email == "organizer01@qlsk.local");
            var banToChucHienCo = taiKhoanCanXuLy.BanToChuc!;
            var diaDiemSuKien = await nguCanhDuLieu.DanhSachDiaDiem.SingleAsync(diaDiemDb => diaDiemDb.TenDiaDiem == "Trung tâm sự kiện Bến Sông");
            var danhSachSuKienHienCo = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.DanhSachLoaiVe).Include(suKienDb => suKienDb.DatDiaDiem)
                .Where(suKienDb => suKienDb.BanToChucId == banToChucHienCo.BanToChucId).OrderBy(suKienDb => suKienDb.SuKienId).ToListAsync();
            var suKienDauTien = danhSachSuKienHienCo[0];
            var suKienDuocSua = danhSachSuKienHienCo[1];
            taiKhoanCanXuLy.Email = "demo.btc01@qlsk.local";
            taiKhoanCanXuLy.HoTen = "Ban tổ chức mẫu 01";
            banToChucHienCo.TenBanToChuc = "SựKiện Hub Demo 01";
            banToChucHienCo.MoTaBanToChuc = "Ban tổ chức mẫu phục vụ trải nghiệm hệ thống.";
            diaDiemSuKien.TenDiaDiem = "Không gian sự kiện Demo 01 - TP. Hồ Chí Minh";
            diaDiemSuKien.DiaChi = "Khu trải nghiệm mẫu 01, TP. Hồ Chí Minh (địa điểm giả lập)";
            suKienDauTien.MoTaSuKien = "Nội dung đã được biên tập riêng. " + LegacyDescription;
            suKienDuocSua.TenSuKien = "Tên chương trình đã được khách sửa";
            suKienDuocSua.MoTaSuKien = "Nội dung đã chỉnh sửa cần được giữ nguyên.";
            suKienDuocSua.DanhSachLoaiVe.First().GiaVe = 123_000;
            await nguCanhDuLieu.SaveChangesAsync();
            var taiKhoanId = taiKhoanCanXuLy.TaiKhoanId;
            var banToChucId = banToChucHienCo.BanToChucId;
            var diaDiemId = diaDiemSuKien.DiaDiemId;
            var banGhiDauTienId = suKienDauTien.SuKienId;
            var suKienDuocSuaId = suKienDuocSua.SuKienId;
            var matKhauDaBam = taiKhoanCanXuLy.MatKhauBam;
            var thoiDiemBatDauDauTien = suKienDauTien.ThoiGianBatDau;
            var duongDanHinhAnh = suKienDauTien.HinhAnh;
            var datDiaDiemId = suKienDauTien.DatDiaDiem!.DatDiaDiemId;
            var tenSuKienSauSua = suKienDuocSua.TenSuKien;
            var moTaSuKienSauSua = suKienDuocSua.MoTaSuKien;
            nguCanhDuLieu.ChangeTracker.Clear();

        var ketQuaXemTruocChuanHoa = await LocalEventSeeder.ChuanHoaDuLieuMauHienCoAsync(nguCanhDuLieu, chiXemTruocThayDoi: true);
            Assert.True(ketQuaXemTruocChuanHoa.SoTaiKhoanDaChuanHoa > 0 && ketQuaXemTruocChuanHoa.SoBanToChucDaChuanHoa > 0 && ketQuaXemTruocChuanHoa.SoDiaDiemDaChuanHoa > 0 && ketQuaXemTruocChuanHoa.SoSuKienDaChuanHoa > 0);
            nguCanhDuLieu.ChangeTracker.Clear();
            Assert.Equal("demo.btc01@qlsk.local", (await nguCanhDuLieu.DanhSachTaiKhoan.SingleAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId)).Email);
            Assert.Contains(LegacyDescription, (await nguCanhDuLieu.DanhSachSuKien.SingleAsync(suKienDb => suKienDb.SuKienId == banGhiDauTienId)).MoTaSuKien);
            nguCanhDuLieu.ChangeTracker.Clear();

            await LocalEventSeeder.ChuanHoaDuLieuMauHienCoAsync(nguCanhDuLieu);
            nguCanhDuLieu.ChangeTracker.Clear();
            var taiKhoanDaLuuKiemThu = await nguCanhDuLieu.DanhSachTaiKhoan.SingleAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId);
            var banToChucDaLuuKiemThu = await nguCanhDuLieu.DanhSachBanToChuc.SingleAsync(banToChucDb => banToChucDb.BanToChucId == banToChucId);
            var diaDiemDaLuuKiemThu = await nguCanhDuLieu.DanhSachDiaDiem.SingleAsync(diaDiemDb => diaDiemDb.DiaDiemId == diaDiemId);
            var suKienDaLuu = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.DatDiaDiem).SingleAsync(suKienDb => suKienDb.SuKienId == banGhiDauTienId);
            var suKienDaSuaKiemThu = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.DanhSachLoaiVe).SingleAsync(suKienDb => suKienDb.SuKienId == suKienDuocSuaId);
            Assert.Equal("organizer01@qlsk.local", taiKhoanDaLuuKiemThu.Email);
            Assert.Equal(matKhauDaBam, taiKhoanDaLuuKiemThu.MatKhauBam);
            Assert.Equal("Kết Nối Văn Hóa", banToChucDaLuuKiemThu.TenBanToChuc);
            Assert.Equal(taiKhoanId, banToChucDaLuuKiemThu.TaiKhoanId);
            Assert.Equal("Trung tâm sự kiện Bến Sông", diaDiemDaLuuKiemThu.TenDiaDiem);
            Assert.Equal("Khu trung tâm, TP. Hồ Chí Minh", diaDiemDaLuuKiemThu.DiaChi);
            Assert.Equal(600, diaDiemDaLuuKiemThu.SucChua);
            Assert.Equal(2_000_000, diaDiemDaLuuKiemThu.GiaThue);
            Assert.Equal("Nội dung đã được biên tập riêng.", suKienDaLuu.MoTaSuKien);
            Assert.Equal(thoiDiemBatDauDauTien, suKienDaLuu.ThoiGianBatDau);
            Assert.Equal(duongDanHinhAnh, suKienDaLuu.HinhAnh);
            Assert.Equal(banToChucId, suKienDaLuu.BanToChucId);
            Assert.Equal(datDiaDiemId, suKienDaLuu.DatDiaDiem!.DatDiaDiemId);
            Assert.Equal(diaDiemId, suKienDaLuu.DatDiaDiem.DiaDiemId);
            Assert.Equal(tenSuKienSauSua, suKienDaSuaKiemThu.TenSuKien);
            Assert.Equal(moTaSuKienSauSua, suKienDaSuaKiemThu.MoTaSuKien);
            Assert.Contains(suKienDaSuaKiemThu.DanhSachLoaiVe, loaiVeDb => loaiVeDb.GiaVe == 123_000);
            Assert.Equal(100, await nguCanhDuLieu.DanhSachSuKien.CountAsync());
            Assert.Equal(200, await nguCanhDuLieu.DanhSachLoaiVe.CountAsync());
            Assert.Equal(100, await nguCanhDuLieu.DanhSachDatDiaDiem.CountAsync());
            var ketQuaThucHienLai = await LocalEventSeeder.ChuanHoaDuLieuMauHienCoAsync(nguCanhDuLieu);
            Assert.Equal(0, ketQuaThucHienLai.SoTaiKhoanDaChuanHoa + ketQuaThucHienLai.SoBanToChucDaChuanHoa + ketQuaThucHienLai.SoDiaDiemDaChuanHoa + ketQuaThucHienLai.SoSuKienDaChuanHoa);
        });
    }

}
