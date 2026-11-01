using System.Data;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;

namespace QLSK.Setup;

public enum AdminProvisioningResult
{
    Created,
    AlreadyConfigured
}

public static class AdminProvisioner
{
    public static async Task<AdminProvisioningResult> CreateFirstAsync(
        ApplicationDbContext db,
        IPasswordHasher<TaiKhoan> hasher,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = ValidateCredentials(email, password);
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;

        // Setup never resets credentials, unlocks accounts or grants an existing user a role.
        if (await db.TaiKhoans.AnyAsync(user => user.VaiTroId == 4, cancellationToken))
            return AdminProvisioningResult.AlreadyConfigured;

        if (!await db.VaiTros.AnyAsync(role => role.Id == 4, cancellationToken))
            throw new SetupValidationException("Chưa có vai trò quản trị. Hãy chạy lệnh migrate trước.");
        if (await db.TaiKhoans.AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
            throw new SetupValidationException("Email đã được sử dụng. Công cụ không thay đổi vai trò của tài khoản hiện có.");

        var administrator = new TaiKhoan
        {
            Email = normalizedEmail,
            HoTen = "Quản trị viên",
            VaiTroId = 4,
            HoatDong = true,
            NgayTao = DateTime.UtcNow
        };
        administrator.MatKhau = hasher.HashPassword(administrator, password);
        db.TaiKhoans.Add(administrator);
        await db.SaveChangesAsync(cancellationToken);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        return AdminProvisioningResult.Created;
    }

    public static async Task UpdateAsync(ApplicationDbContext db, IPasswordHasher<TaiKhoan> hasher,
        int id, string email, string password)
    {
        var normalizedEmail = ValidateCredentials(email, password);
        var admin = await db.TaiKhoans.SingleOrDefaultAsync(x => x.Id == id && x.VaiTroId == 4)
            ?? throw new SetupValidationException("Không tìm thấy tài khoản quản trị đích.");
        if (await db.TaiKhoans.AnyAsync(x => x.Id != id && x.Email == normalizedEmail))
            throw new SetupValidationException("Email đã được sử dụng.");
        admin.Email = normalizedEmail;
        admin.MatKhau = hasher.HashPassword(admin, password);
        await db.SaveChangesAsync();
    }

    internal static string ValidateCredentials(string email, string password)
    {
        var normalizedEmail = (email ?? "").Trim().ToLowerInvariant();
        if (normalizedEmail.Length > 200 ||
            !MailAddress.TryCreate(normalizedEmail, out var address) ||
            !string.Equals(address.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(address.DisplayName))
            throw new SetupValidationException("Cần cung cấp email quản trị hợp lệ, tối đa 200 ký tự.");

        var categories = new[] { password.Any(char.IsUpper), password.Any(char.IsLower), password.Any(char.IsDigit),
            password.Any(character => !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character)) }.Count(x => x);
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12 || categories < 3)
            throw new SetupValidationException("Mật khẩu quản trị cần ít nhất 12 ký tự và ít nhất ba nhóm chữ hoa, chữ thường, số, ký tự đặc biệt.");
        return normalizedEmail;
    }
}

public sealed class SetupValidationException(string message) : Exception(message);
