using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;

namespace QLSK.Setup;

internal static class SetupProgram
{
    public static async Task<int> Main(string[] thamSoDongLenh)
    {
        var interactive = !Console.IsInputRedirected;
        if (thamSoDongLenh is ["--help"] or ["-h"] || thamSoDongLenh.Length == 0 && !interactive)
        {
            ShowUsage();
            return 0;
        }
        if (thamSoDongLenh.Length == 0)
        {
            var lenhThietLap = SetupConsole.ChooseCommand();
            if (lenhThietLap is null) return 0;
            thamSoDongLenh = [lenhThietLap];
        }
        if (thamSoDongLenh.Length != 1 || thamSoDongLenh[0] is not ("migrate" or "create-admin" or "update-admin" or "adopt-legacy-data"))
        {
            Console.Error.WriteLine("Lệnh không hợp lệ.");
            ShowUsage();
            return 2;
        }

        try
        {
            var chuoiKetNoiCoSoDuLieu = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
            if (string.IsNullOrWhiteSpace(chuoiKetNoiCoSoDuLieu) && interactive)
                chuoiKetNoiCoSoDuLieu = DevelopmentSettings.FindConnectionString(Directory.GetCurrentDirectory())
                    ?? DevelopmentSettings.FindConnectionString(AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(chuoiKetNoiCoSoDuLieu))
                throw new SetupValidationException("Cần đặt biến môi trường ConnectionStrings__Default cho cơ sở dữ liệu đích.");

            SqlConnectionStringBuilder giaTriDaPhanTich;
            try { giaTriDaPhanTich = new SqlConnectionStringBuilder(chuoiKetNoiCoSoDuLieu); }
            catch (ArgumentException)
            {
                throw new SetupValidationException("Cấu hình kết nối cơ sở dữ liệu không hợp lệ.");
            }
            if (string.IsNullOrWhiteSpace(giaTriDaPhanTich.DataSource) || string.IsNullOrWhiteSpace(giaTriDaPhanTich.InitialCatalog) ||
                new[] { "master", "model", "msdb", "tempdb" }.Contains(giaTriDaPhanTich.InitialCatalog, StringComparer.OrdinalIgnoreCase))
                throw new SetupValidationException("Cần chỉ định máy chủ và cơ sở dữ liệu ứng dụng riêng.");

            if (interactive)
                Console.WriteLine($"Database: {giaTriDaPhanTich.DataSource} / {giaTriDaPhanTich.InitialCatalog}");
            var tuyChonCoSoDuLieu = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(chuoiKetNoiCoSoDuLieu)
                .Options;
            await using var nguCanhDuLieu = new ApplicationDbContext(tuyChonCoSoDuLieu);
            if (thamSoDongLenh[0] == "migrate")
            {
                await nguCanhDuLieu.Database.MigrateAsync();
                Console.WriteLine("Đã cập nhật cấu trúc cơ sở dữ liệu. Không thêm sự kiện hoặc tài khoản.");
                return 0;
            }

            if (thamSoDongLenh[0] == "create-admin" && await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.VaiTroId == 4))
            {
                Console.WriteLine("Đã có tài khoản quản trị. Chọn đổi thông tin admin hoặc chạy update-admin.");
                return 0;
            }

            var taiKhoanId = 0;
            if (thamSoDongLenh[0] == "update-admin")
            {
                var chuoiTaiKhoanId = Environment.GetEnvironmentVariable("QLSK_ADMIN_ID");
                if (string.IsNullOrWhiteSpace(chuoiTaiKhoanId) && interactive)
                {
                    var danhSachQuanTriVien = await nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().Where(taiKhoanDb => taiKhoanDb.VaiTroId == 4)
                        .OrderBy(taiKhoanDb => taiKhoanDb.TaiKhoanId).Select(taiKhoanDb => new { Id = taiKhoanDb.TaiKhoanId, taiKhoanDb.Email, taiKhoanDb.HoatDong }).ToListAsync();
                    if (danhSachQuanTriVien.Count == 0)
                        throw new SetupValidationException("Chưa có quản trị viên. Hãy chọn tạo admin mới.");
                    foreach (var taiKhoanQuanTri in danhSachQuanTriVien)
                        Console.WriteLine($"  ID {taiKhoanQuanTri.Id}: {taiKhoanQuanTri.Email} ({(taiKhoanQuanTri.HoatDong ? "đang hoạt động" : "đã khóa")})");
                    chuoiTaiKhoanId = SetupConsole.ReadRequiredInput("ID admin cần đổi");
                }
                if (!int.TryParse(chuoiTaiKhoanId, out taiKhoanId) || taiKhoanId <= 0)
                    throw new SetupValidationException("Cần đặt QLSK_ADMIN_ID cho tài khoản quản trị cần cập nhật.");
                if (!await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId && taiKhoanDb.VaiTroId == 4))
                    throw new SetupValidationException("Không tìm thấy tài khoản quản trị đích.");
            }

            var emailTaiKhoan = Environment.GetEnvironmentVariable("QLSK_ADMIN_EMAIL") ?? "";
            var matKhauNhapLieu = Environment.GetEnvironmentVariable("QLSK_ADMIN_PASSWORD") ?? "";
            if (interactive)
            {
                if (string.IsNullOrWhiteSpace(emailTaiKhoan)) emailTaiKhoan = SetupConsole.ReadRequiredInput("Email quản trị");
                if (string.IsNullOrWhiteSpace(matKhauNhapLieu)) matKhauNhapLieu = SetupConsole.ReadNewPassword();
            }
            if (thamSoDongLenh[0] == "update-admin")
            {
                await AdminProvisioner.UpdateAdminCredentialsAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), taiKhoanId, emailTaiKhoan, matKhauNhapLieu);
                Console.WriteLine("Đã đổi thông tin quản trị. Các phiên đăng nhập cũ đã mất hiệu lực.");
                return 0;
            }
            if (thamSoDongLenh[0] == "adopt-legacy-data")
            {
                var soTaiKhoanDaThuHoi = await LegacyDataMigrator.AdoptLegacyAccountsAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), emailTaiKhoan, matKhauNhapLieu);
                Console.WriteLine($"Đã chuyển đổi dữ liệu cũ, thu hồi thông tin đăng nhập của {soTaiKhoanDaThuHoi} tài khoản sở hữu cũ. Giữ nguyên các bản ghi nghiệp vụ.");
                return 0;
            }
            var ketQuaTaoQuanTri = await AdminProvisioner.CreateFirstAdminAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), emailTaiKhoan, matKhauNhapLieu);
            Console.WriteLine(ketQuaTaoQuanTri == AdminProvisioningResult.Created
                ? "Đã tạo tài khoản quản trị đầu tiên."
                : "Đã có tài khoản quản trị. Không thay đổi tài khoản hoặc mật khẩu hiện có.");
            return 0;
        }
        catch (SetupValidationException loiXuLy)
        {
            Console.Error.WriteLine(loiXuLy.Message);
            return 2;
        }
        catch
        {
            // SQL/provider errors can contain connection details; never print exception data.
            Console.Error.WriteLine("Không thể hoàn tất thiết lập. Kiểm tra kết nối, quyền cơ sở dữ liệu và trạng thái migration.");
            return 1;
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Công cụ thiết lập QLSK; chỉ chạy khi quản trị viên chủ động thực hiện.");
        Console.WriteLine("  dotnet run              Mở menu nhập liệu trong terminal.");
        Console.WriteLine("  QLSK.Setup migrate       Cập nhật cấu trúc cơ sở dữ liệu.");
        Console.WriteLine("  QLSK.Setup create-admin  Tạo tài khoản quản trị đầu tiên.");
        Console.WriteLine("  QLSK.Setup update-admin  Đổi email/mật khẩu của QLSK_ADMIN_ID.");
        Console.WriteLine("  QLSK.Setup adopt-legacy-data  Chuyển đổi tài khoản cũ, giữ dữ liệu nghiệp vụ.");
        Console.WriteLine("Trong terminal: tự đọc appsettings.Development.json của mã nguồn nếu chưa đặt ConnectionStrings__Default.");
        Console.WriteLine("Email, mật khẩu và ID được hỏi trực tiếp khi chưa đặt QLSK_ADMIN_EMAIL, QLSK_ADMIN_PASSWORD, QLSK_ADMIN_ID.");
        Console.WriteLine("Chạy tự động: bắt buộc cấp các biến môi trường; không hỏi nhập liệu hoặc tự chọn database.");
        Console.WriteLine("Không có tài khoản hoặc mật khẩu mặc định. Không truyền thông tin đăng nhập qua tham số lệnh.");
    }
}
