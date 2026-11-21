using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Domain.Enums;
using QLSK.Features.Accounts;
using QLSK.Features.Requests;

namespace QLSK.Data.Seed;

internal static class DemoSeed
{
    internal static async Task Run(IServiceProvider services, AppDbContext db, IConfiguration config)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        await SqlOperations.Lock(db, "qlsk:demo-seed", CancellationToken.None);
        const string key = "QLSK.DemoSeedVersion";
        if (await DatabaseSetup.Version(db, key) >= 1)
        {
            Console.WriteLine("Cơ sở dữ liệu đã có bộ demo QLSK.");
            return;
        }
        var configuredPassword = config["QLSK:Demo:Password"];
        var password = configuredPassword ?? Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
        await Accounts(db, password);
        await DatabaseSetup.Script(db, "Seed.010-policies-settings.sql");
        await DatabaseSetup.Script(db, "Seed.020-catalog-events.sql");
        await DatabaseSetup.Script(db, "Seed.030-rental-operations.sql");
        await ProtectRentals(db, services.GetRequiredService<IDataProtectionProvider>());
        await DatabaseSetup.Script(db, "Seed.040-tickets-finance.sql");
        await DatabaseSetup.Script(db, "Seed.050-home-account.sql");
        var document = await DemoOrganizerDocuments.Create(services, db);
        try
        {
            await DatabaseSetup.Stamp(db, key, 1);
        }
        catch
        {
            File.Delete(document);
            throw;
        }
        // Keep the document if the server commits but the acknowledgement is lost.
        await tx.CommitAsync();
        Console.WriteLine("Đã nạp bộ demo QLSK. Email quản trị: admin@qlsk.test.");
        if (configuredPassword == null) Console.WriteLine($"Mật khẩu demo (hiển thị lần đầu): {password}");
    }

    private static async Task Accounts(AppDbContext db, string password)
    {
        (string Code, string Name, VaiTroTaiKhoan Role)[] accounts = [
            ("admin", "Trần Minh An", VaiTroTaiKhoan.QuanTriVien),
            ("manager", "Phạm Thanh Hà", VaiTroTaiKhoan.QuanLyHeThong),
            ("organizer", "Nguyễn Minh Anh", VaiTroTaiKhoan.BanToChuc),
            ("other-organizer", "Lê Bảo Trâm", VaiTroTaiKhoan.BanToChuc),
            ("staff", "Vũ Hoàng Nam", VaiTroTaiKhoan.QuanLySuKien),
            ("customer", "Mai Khánh Linh", VaiTroTaiKhoan.NguoiDung),
            ("pending-organizer", "Đỗ Hải Yến", VaiTroTaiKhoan.NguoiDung)
        ];
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE #SeedAccount(Code varchar(30) PRIMARY KEY, Id int NOT NULL)");
        foreach (var (code, name, role) in accounts)
        {
            var account = AccountService.NewAccount(name, "demo-" + code, code + "@qlsk.test", null, password);
            if (await db.TaiKhoan.AnyAsync(a => a.Email == account.Email || a.TenDangNhap == account.TenDangNhap))
                throw new InvalidOperationException($"Tài khoản {account.Email} trùng với dữ liệu hiện có. Chọn cơ sở dữ liệu thử khác.");
            account.VaiTro = role;
            account.NgayTao = DateTime.UtcNow.AddMonths(-4);
            account.EmailXacMinhLuc = account.NgayTao;
            if (role == VaiTroTaiKhoan.QuanLyHeThong)
                account.QuyenQuanLy = """{"organizer.review":true,"events.review":true,"rentals.review":true,"support.review":true,"refunds.review":true,"finance.review":true,"monitoring.view":true}""";
            db.TaiKhoan.Add(account);
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT #SeedAccount(Code,Id) VALUES ({code},{account.Id})");
        }
    }

    private static async Task ProtectRentals(AppDbContext db, IDataProtectionProvider protection)
    {
        var protector = protection.CreateProtector("QLSK.Rental.Content.v1");
        var rentals = db.YeuCauThue.FromSqlRaw("SELECT r.* FROM dbo.YeuCauThue r JOIN #SeedRequest s ON r.Id = s.Id");
        var rows = await (from rental in rentals
                          join owner in db.TaiKhoan on rental.TaiKhoanId equals owner.Id
                          join venue in db.DiaDiem on rental.DiaDiemId equals venue.Id
                          select new { rental, owner.HoTen, owner.VaiTro, venue.SucChua }).ToListAsync();
        foreach (var row in rows)
        {
            var r = row.rental;
            var community = row.VaiTro == VaiTroTaiKhoan.NguoiDung;
            var content = new RentalCreateRequest(r.DiaDiemId, DateTime.SpecifyKind(r.ThoiGianBatDau, DateTimeKind.Utc),
                DateTime.SpecifyKind(r.ThoiGianKetThuc, DateTimeKind.Utc), Math.Min(community ? 100 : 40, row.SucChua), r.ChinhSachId,
                community ? "Ngày hội gia đình" : "Diễn đàn khởi nghiệp sáng tạo", community ? "Giao lưu cộng đồng" : "Hội thảo",
                community ? "Tổ chức sinh hoạt gia đình và hoạt động nhóm." : "Giới thiệu dự án và kết nối các nhóm khởi nghiệp.",
                "Đón khách, khai mạc, trình bày và giao lưu.", "Máy chiếu, hai micro không dây và bàn tiếp tân.",
                "Bố trí lối vào cho người sử dụng xe lăn.", row.HoTen, "0900000000");
            r.NoiDungYeuCau = protector.Protect(JsonSerializer.Serialize(content));
        }
        await db.SaveChangesAsync();
    }
}
