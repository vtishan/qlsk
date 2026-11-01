using System.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;

namespace QLSK.Setup;

public static class LegacyDataMigrator
{
    // Explicit one-time adoption of the old seed records, never part of startup/migrations.
    // IDs and business records remain intact. No catalog is loaded or re-seeded.
    public static async Task<int> AdoptAsync(ApplicationDbContext db, IPasswordHasher<TaiKhoan> hasher,
        string adminEmail, string adminPassword)
    {
        var email = AdminProvisioner.ValidateCredentials(adminEmail, adminPassword);
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var legacyAdmin = await db.TaiKhoans.SingleOrDefaultAsync(x => x.Email == "admin@qlsk.local" && x.VaiTroId == 4);
        if (legacyAdmin is not null)
        {
            if (await db.TaiKhoans.AnyAsync(x => x.Id != legacyAdmin.Id && x.Email == email))
                throw new SetupValidationException("Email quản trị mới đã được sử dụng.");
            legacyAdmin.Email = email;
            legacyAdmin.MatKhau = hasher.HashPassword(legacyAdmin, adminPassword);
        }
        else if (!await db.TaiKhoans.AnyAsync(x => x.VaiTroId == 4))
        {
            throw new SetupValidationException("Không tìm thấy quản trị viên. Hãy chạy create-admin trước.");
        }

        var legacyEmails = Enumerable.Range(1, 5).SelectMany(i => new[]
            { $"demo.btc{i:00}@qlsk.local", $"organizer{i:00}@qlsk.local" }).ToArray();
        var owners = await db.TaiKhoans.Where(x => x.VaiTroId == 2 && legacyEmails.Contains(x.Email)).ToListAsync();
        foreach (var owner in owners)
        {
            owner.Email = $"archived-organizer-{owner.Id}@disabled.invalid";
            owner.HoatDong = false;
            owner.MatKhau = hasher.HashPassword(owner, Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)));
        }
        var smokeAccounts = await db.TaiKhoans.Where(x => x.VaiTroId == 1 && x.HoTen == "Smoke Test" &&
            x.Email.StartsWith("smoke-") && x.Email.EndsWith("@test.local")).ToListAsync();
        foreach (var account in smokeAccounts)
        {
            var hasHistory = await db.DonVes.AnyAsync(x => x.TaiKhoanId == account.Id) ||
                await db.BanToChucs.AnyAsync(x => x.TaiKhoanId == account.Id) ||
                await db.YeuCauXacThucToChucs.AnyAsync(x => x.TaiKhoanId == account.Id) ||
                await db.LichSuYeuCauXacThucToChucs.AnyAsync(x => x.NguoiXuLyId == account.Id || x.TaiKhoanId == account.Id);
            if (!hasHistory) db.TaiKhoans.Remove(account);
            else
            {
                account.Email = $"archived-account-{account.Id}@disabled.invalid";
                account.HoTen = "Tài khoản lưu trữ";
                account.HoatDong = false;
                account.MatKhau = hasher.HashPassword(account, Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)));
            }
        }
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return owners.Count;
    }
}
