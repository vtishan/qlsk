using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QLSK.Data;
using QLSK.Domain.Entities;

namespace QLSK.LocalData;

internal static class LocalDataProgram
{
    private static async Task<int> Main(string[] thamSoDongLenh)
    {
        try
        {
            if (thamSoDongLenh.Length == 0 || thamSoDongLenh.Contains("--help"))
            {
                Console.WriteLine("Local data maintenance; never run as part of backend startup.");
                Console.WriteLine("--normalize-existing [--dry-run] [--root PROJECT_DIRECTORY]");
                Console.WriteLine("--populate [--root PROJECT_DIRECTORY] (new account password: LOCALDATA_PASSWORD environment variable)");
                Console.WriteLine("--populate-users [--root PROJECT_DIRECTORY] (100 sample users and related orders on the configured local database)");
                return thamSoDongLenh.Length == 0 ? 1 : 0;
            }
            var canChuanHoaDuLieuMau = thamSoDongLenh.Contains("--normalize-existing");
            var canTaoDuLieuMau = thamSoDongLenh.Contains("--populate");
            var canTaoNguoiDungMau = thamSoDongLenh.Contains("--populate-users");
            var chiXemTruocThayDoi = thamSoDongLenh.Contains("--dry-run");
            if (new[] { canChuanHoaDuLieuMau, canTaoDuLieuMau, canTaoNguoiDungMau }.Count(giaTriCanXuLy => giaTriCanXuLy) != 1 || (!canChuanHoaDuLieuMau && chiXemTruocThayDoi))
                throw new ArgumentException("Choose exactly one operation; --dry-run is available for normalization only.");
            string? thuMucDuAnChiDinh = null;
            for (var chiSo = 0; chiSo < thamSoDongLenh.Length; chiSo++)
            {
                if (thamSoDongLenh[chiSo] == "--root")
                {
                    if (++chiSo >= thamSoDongLenh.Length) throw new ArgumentException("--root requires the project directory.");
                    thuMucDuAnChiDinh = thamSoDongLenh[chiSo];
                }
                else if (thamSoDongLenh[chiSo] is not ("--normalize-existing" or "--populate" or "--populate-users" or "--dry-run"))
                    throw new ArgumentException("Unknown command argument. Use --help.");
            }
            var thuMucDuAn = XacDinhThuMucDuAn(thuMucDuAnChiDinh);
            var boDungCauHinh = new ConfigurationBuilder().SetBasePath(thuMucDuAn)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: false)
                .AddJsonFile("appsettings.Local.json", optional: true);
            if (canTaoNguoiDungMau) boDungCauHinh.AddUserSecrets(typeof(ApplicationDbContext).Assembly, optional: true);
            var cauHinhUngDung = boDungCauHinh.AddEnvironmentVariables().Build();
            var chuoiKetNoiCoSoDuLieu = cauHinhUngDung.GetConnectionString("Default") ?? "";
            LocalSqlGuard.KiemTraKetNoiSqlLocal(chuoiKetNoiCoSoDuLieu, requireTestDatabase: canTaoDuLieuMau);
            var tuyChonCoSoDuLieu = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(chuoiKetNoiCoSoDuLieu).Options;
            await using var nguCanhDuLieu = new ApplicationDbContext(tuyChonCoSoDuLieu);
            if (canChuanHoaDuLieuMau)
            {
                var ketQuaThucHien = await LocalEventSeeder.ChuanHoaDuLieuMauHienCoAsync(nguCanhDuLieu, chiXemTruocThayDoi);
                Console.WriteLine(JsonSerializer.Serialize(new { operation = "normalize-existing", dryRun = chiXemTruocThayDoi, result = ketQuaThucHien }));
            }
            else if (canTaoNguoiDungMau)
            {
                var matKhauNhapLieu = cauHinhUngDung["LOCALDATA_PASSWORD"] ?? cauHinhUngDung["LocalData:UserPassword"] ?? "";
                var ketQuaThucHien = await LocalUserSeeder.TaoDuLieuNguoiDungMauAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), matKhauNhapLieu);
                Console.WriteLine(JsonSerializer.Serialize(new { operation = "populate-users", result = ketQuaThucHien }));
            }
            else
            {
                var matKhauNhapLieu = Environment.GetEnvironmentVariable("LOCALDATA_PASSWORD") ?? "";
                if (string.IsNullOrWhiteSpace(matKhauNhapLieu))
                    throw new ArgumentException("Set LOCALDATA_PASSWORD for newly created organizer accounts.");
                var soBanGhiDaThem = await LocalEventSeeder.TaoDuLieuSuKienMauAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), thuMucDuAn, matKhauNhapLieu);
                Console.WriteLine(JsonSerializer.Serialize(new { operation = "populate", addedEvents = soBanGhiDaThem }));
            }
            return 0;
        }
        catch (ArgumentException loiXuLy) { Console.Error.WriteLine(loiXuLy.Message); return 2; }
        catch (InvalidOperationException loiXuLy) when (!loiXuLy.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
        { Console.Error.WriteLine(loiXuLy.Message); return 2; }
        catch
        {
            Console.Error.WriteLine("Unable to complete the local database operation. Check the local configuration and migration state.");
            return 3;
        }
    }

    private static string XacDinhThuMucDuAn(string? giaTriCanXuLy)
    {
        if (giaTriCanXuLy is not null)
        {
            var thuMucDuAn = Path.GetFullPath(giaTriCanXuLy);
            if (!File.Exists(Path.Combine(thuMucDuAn, "QLSK.csproj"))) throw new ArgumentException("--root must identify the QLSK project directory.");
            return thuMucDuAn;
        }
        foreach (var thoiDiemBatDau in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            for (var giaTriHienTai = new DirectoryInfo(thoiDiemBatDau); giaTriHienTai is not null; giaTriHienTai = giaTriHienTai.Parent)
                if (File.Exists(Path.Combine(giaTriHienTai.FullName, "QLSK.csproj"))) return giaTriHienTai.FullName;
        throw new ArgumentException("Unable to find the project directory. Pass --root explicitly.");
    }
}
