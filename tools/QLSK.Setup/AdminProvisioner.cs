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
    public static async Task<AdminProvisioningResult> CreateFirstAdminAsync(
        ApplicationDbContext nguCanhDuLieu,
        IPasswordHasher<TaiKhoan> boBamMatKhau,
        string emailTaiKhoan,
        string matKhauNhapLieu,
        CancellationToken tokenHuyThaoTac = default)
    {
        var normalizedEmail = ValidateCredentials(emailTaiKhoan, matKhauNhapLieu);
        await using var giaoDichCoSoDuLieu = nguCanhDuLieu.Database.IsRelational()
            ? await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable, tokenHuyThaoTac)
            : null;

        // Setup never resets credentials, unlocks accounts or grants an existing user a role.
        if (await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.VaiTroId == 4, tokenHuyThaoTac))
            return AdminProvisioningResult.AlreadyConfigured;

        if (!await nguCanhDuLieu.DanhSachVaiTro.AnyAsync(vaiTroDb => vaiTroDb.VaiTroId == 4, tokenHuyThaoTac))
            throw new SetupValidationException("Chưa có vai trò quản trị. Hãy chạy lệnh migrate trước.");
        if (await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.Email == normalizedEmail, tokenHuyThaoTac))
            throw new SetupValidationException("Email đã được sử dụng. Công cụ không thay đổi vai trò của tài khoản hiện có.");

        var taiKhoanQuanTri = new TaiKhoan
        {
            Email = normalizedEmail,
            HoTen = "Quản trị viên",
            VaiTroId = 4,
            HoatDong = true,
            NgayTao = DateTime.UtcNow
        };
        taiKhoanQuanTri.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanQuanTri, matKhauNhapLieu);
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoanQuanTri);
        await nguCanhDuLieu.SaveChangesAsync(tokenHuyThaoTac);
        if (giaoDichCoSoDuLieu is not null) await giaoDichCoSoDuLieu.CommitAsync(tokenHuyThaoTac);
        return AdminProvisioningResult.Created;
    }

    public static async Task UpdateAdminCredentialsAsync(ApplicationDbContext nguCanhDuLieu, IPasswordHasher<TaiKhoan> boBamMatKhau,
        int taiKhoanId, string emailTaiKhoan, string matKhauNhapLieu)
    {
        var normalizedEmail = ValidateCredentials(emailTaiKhoan, matKhauNhapLieu);
        var taiKhoanQuanTri = await nguCanhDuLieu.DanhSachTaiKhoan.SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId && taiKhoanDb.VaiTroId == 4)
            ?? throw new SetupValidationException("Không tìm thấy tài khoản quản trị đích.");
        if (await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId != taiKhoanId && taiKhoanDb.Email == normalizedEmail))
            throw new SetupValidationException("Email đã được sử dụng.");
        taiKhoanQuanTri.Email = normalizedEmail;
        taiKhoanQuanTri.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanQuanTri, matKhauNhapLieu);
        await nguCanhDuLieu.SaveChangesAsync();
    }

    internal static string ValidateCredentials(string emailTaiKhoan, string matKhauNhapLieu)
    {
        var normalizedEmail = (emailTaiKhoan ?? "").Trim().ToLowerInvariant();
        if (normalizedEmail.Length > 200 ||
            !MailAddress.TryCreate(normalizedEmail, out var diaChiEmail) ||
            !string.Equals(diaChiEmail.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(diaChiEmail.DisplayName))
            throw new SetupValidationException("Cần cung cấp email quản trị hợp lệ, tối đa 200 ký tự.");

        var danhSachLoaiSuKien = new[] { matKhauNhapLieu.Any(char.IsUpper), matKhauNhapLieu.Any(char.IsLower), matKhauNhapLieu.Any(char.IsDigit),
            matKhauNhapLieu.Any(kyTuMatKhau => !char.IsLetterOrDigit(kyTuMatKhau) && !char.IsWhiteSpace(kyTuMatKhau)) }.Count(dieuKienDat => dieuKienDat);
        if (string.IsNullOrWhiteSpace(matKhauNhapLieu) || matKhauNhapLieu.Length < 12 || danhSachLoaiSuKien < 3)
            throw new SetupValidationException("Mật khẩu quản trị cần ít nhất 12 ký tự và ít nhất ba nhóm chữ hoa, chữ thường, số, ký tự đặc biệt.");
        return normalizedEmail;
    }
}

public sealed class SetupValidationException(string thongDiepLoi) : Exception(thongDiepLoi);
