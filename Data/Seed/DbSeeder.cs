using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;

namespace QLSK.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration config,
        IHostEnvironment environment)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing")) return;

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Database.IsRelational()) await db.Database.MigrateAsync();
        else await db.Database.EnsureCreatedAsync();

        if (!environment.IsDevelopment()) return;

        var email = config["SeedAdmin:Email"]?.Trim().ToLowerInvariant();
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        if (await db.TaiKhoans.AnyAsync(x => x.Email == email)) return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();
        var admin = new TaiKhoan
        {
            HoTen = "Quản trị viên",
            Email = email,
            VaiTroId = 4,
            HoatDong = true
        };
        admin.MatKhau = hasher.HashPassword(admin, password);
        db.TaiKhoans.Add(admin);
        await db.SaveChangesAsync();
    }
}
