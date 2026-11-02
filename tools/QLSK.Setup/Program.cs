using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;

namespace QLSK.Setup;

internal static class SetupProgram
{
    public static async Task<int> Main(string[] args)
    {
        var interactive = !Console.IsInputRedirected;
        if (args is ["--help"] or ["-h"] || args.Length == 0 && !interactive)
        {
            ShowUsage();
            return 0;
        }
        if (args.Length == 0)
        {
            var command = SetupConsole.ChooseCommand();
            if (command is null) return 0;
            args = [command];
        }
        if (args.Length != 1 || args[0] is not ("migrate" or "create-admin" or "update-admin" or "adopt-legacy-data"))
        {
            Console.Error.WriteLine("Lệnh không hợp lệ.");
            ShowUsage();
            return 2;
        }

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
            if (string.IsNullOrWhiteSpace(connectionString) && interactive)
                connectionString = DevelopmentSettings.FindConnectionString(Directory.GetCurrentDirectory())
                    ?? DevelopmentSettings.FindConnectionString(AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new SetupValidationException("Cần đặt biến môi trường ConnectionStrings__Default cho cơ sở dữ liệu đích.");

            SqlConnectionStringBuilder parsed;
            try { parsed = new SqlConnectionStringBuilder(connectionString); }
            catch (ArgumentException)
            {
                throw new SetupValidationException("Cấu hình kết nối cơ sở dữ liệu không hợp lệ.");
            }
            if (string.IsNullOrWhiteSpace(parsed.DataSource) || string.IsNullOrWhiteSpace(parsed.InitialCatalog) ||
                new[] { "master", "model", "msdb", "tempdb" }.Contains(parsed.InitialCatalog, StringComparer.OrdinalIgnoreCase))
                throw new SetupValidationException("Cần chỉ định máy chủ và cơ sở dữ liệu ứng dụng riêng.");

            if (interactive)
                Console.WriteLine($"Database: {parsed.DataSource} / {parsed.InitialCatalog}");
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;
            await using var db = new ApplicationDbContext(options);
            if (args[0] == "migrate")
            {
                await db.Database.MigrateAsync();
                Console.WriteLine("Đã cập nhật cấu trúc cơ sở dữ liệu. Không thêm sự kiện hoặc tài khoản.");
                return 0;
            }

            if (args[0] == "create-admin" && await db.TaiKhoans.AnyAsync(x => x.VaiTroId == 4))
            {
                Console.WriteLine("Đã có tài khoản quản trị. Chọn đổi thông tin admin hoặc chạy update-admin.");
                return 0;
            }

            var id = 0;
            if (args[0] == "update-admin")
            {
                var idText = Environment.GetEnvironmentVariable("QLSK_ADMIN_ID");
                if (string.IsNullOrWhiteSpace(idText) && interactive)
                {
                    var admins = await db.TaiKhoans.AsNoTracking().Where(x => x.VaiTroId == 4)
                        .OrderBy(x => x.Id).Select(x => new { x.Id, x.Email, x.HoatDong }).ToListAsync();
                    if (admins.Count == 0)
                        throw new SetupValidationException("Chưa có quản trị viên. Hãy chọn tạo admin mới.");
                    foreach (var admin in admins)
                        Console.WriteLine($"  ID {admin.Id}: {admin.Email} ({(admin.HoatDong ? "đang hoạt động" : "đã khóa")})");
                    idText = SetupConsole.ReadRequired("ID admin cần đổi");
                }
                if (!int.TryParse(idText, out id) || id <= 0)
                    throw new SetupValidationException("Cần đặt QLSK_ADMIN_ID cho tài khoản quản trị cần cập nhật.");
                if (!await db.TaiKhoans.AnyAsync(x => x.Id == id && x.VaiTroId == 4))
                    throw new SetupValidationException("Không tìm thấy tài khoản quản trị đích.");
            }

            var email = Environment.GetEnvironmentVariable("QLSK_ADMIN_EMAIL") ?? "";
            var password = Environment.GetEnvironmentVariable("QLSK_ADMIN_PASSWORD") ?? "";
            if (interactive)
            {
                if (string.IsNullOrWhiteSpace(email)) email = SetupConsole.ReadRequired("Email quản trị");
                if (string.IsNullOrWhiteSpace(password)) password = SetupConsole.ReadNewPassword();
            }
            if (args[0] == "update-admin")
            {
                await AdminProvisioner.UpdateAsync(db, new PasswordHasher<TaiKhoan>(), id, email, password);
                Console.WriteLine("Đã đổi thông tin quản trị. Các phiên đăng nhập cũ đã mất hiệu lực.");
                return 0;
            }
            if (args[0] == "adopt-legacy-data")
            {
                var count = await LegacyDataMigrator.AdoptAsync(db, new PasswordHasher<TaiKhoan>(), email, password);
                Console.WriteLine($"Đã chuyển đổi dữ liệu cũ, thu hồi thông tin đăng nhập của {count} tài khoản sở hữu cũ. Giữ nguyên các bản ghi nghiệp vụ.");
                return 0;
            }
            var result = await AdminProvisioner.CreateFirstAsync(db, new PasswordHasher<TaiKhoan>(), email, password);
            Console.WriteLine(result == AdminProvisioningResult.Created
                ? "Đã tạo tài khoản quản trị đầu tiên."
                : "Đã có tài khoản quản trị. Không thay đổi tài khoản hoặc mật khẩu hiện có.");
            return 0;
        }
        catch (SetupValidationException error)
        {
            Console.Error.WriteLine(error.Message);
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
