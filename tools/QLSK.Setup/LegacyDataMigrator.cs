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
    public static async Task<int> AdoptLegacyAccountsAsync(ApplicationDbContext nguCanhDuLieu, IPasswordHasher<TaiKhoan> boBamMatKhau,
        string emailQuanTriVien, string matKhauQuanTriVien)
    {
        var emailTaiKhoan = AdminProvisioner.ValidateCredentials(emailQuanTriVien, matKhauQuanTriVien);
        await using var giaoDichCoSoDuLieu = nguCanhDuLieu.Database.IsRelational()
            ? await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var taiKhoanQuanTriCu = await nguCanhDuLieu.DanhSachTaiKhoan.SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.Email == "admin@qlsk.local" && taiKhoanDb.VaiTroId == 4);
        if (taiKhoanQuanTriCu is not null)
        {
            if (await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId != taiKhoanQuanTriCu.TaiKhoanId && taiKhoanDb.Email == emailTaiKhoan))
                throw new SetupValidationException("Email quản trị mới đã được sử dụng.");
            taiKhoanQuanTriCu.Email = emailTaiKhoan;
            taiKhoanQuanTriCu.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanQuanTriCu, matKhauQuanTriVien);
        }
        else if (!await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.VaiTroId == 4))
        {
            throw new SetupValidationException("Không tìm thấy quản trị viên. Hãy chạy create-admin trước.");
        }

        var legacyEmails = Enumerable.Range(1, 5).SelectMany(chiSo => new[]
            { $"demo.btc{chiSo:00}@qlsk.local", $"organizer{chiSo:00}@qlsk.local" }).ToArray();
        var taiKhoanChuSoHuuCu = await nguCanhDuLieu.DanhSachTaiKhoan.Where(taiKhoanDb => taiKhoanDb.VaiTroId == 2 && legacyEmails.Contains(taiKhoanDb.Email)).ToListAsync();
        foreach (var taiKhoanChuSoHuu in taiKhoanChuSoHuuCu)
        {
            taiKhoanChuSoHuu.Email = $"archived-organizer-{taiKhoanChuSoHuu.TaiKhoanId}@disabled.invalid";
            taiKhoanChuSoHuu.HoatDong = false;
            taiKhoanChuSoHuu.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanChuSoHuu, Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)));
        }
        var taiKhoanKiemThuCu = await nguCanhDuLieu.DanhSachTaiKhoan.Where(taiKhoanDb => taiKhoanDb.VaiTroId == 1 && taiKhoanDb.HoTen == "Smoke Test" &&
            taiKhoanDb.Email.StartsWith("smoke-") && taiKhoanDb.Email.EndsWith("@test.local")).ToListAsync();
        foreach (var taiKhoan in taiKhoanKiemThuCu)
        {
            var coLichSuGiaoDich = await nguCanhDuLieu.DanhSachDonVe.AnyAsync(donVeDb => donVeDb.TaiKhoanId == taiKhoan.TaiKhoanId) ||
                await nguCanhDuLieu.DanhSachBanToChuc.AnyAsync(banToChucDb => banToChucDb.TaiKhoanId == taiKhoan.TaiKhoanId) ||
                await nguCanhDuLieu.YeuCauXacThucToChuc.AnyAsync(yeuCauXacThucDb => yeuCauXacThucDb.TaiKhoanId == taiKhoan.TaiKhoanId) ||
                await nguCanhDuLieu.LichSuYeuCauXacThucToChuc.AnyAsync(lichSuXacThucDb => lichSuXacThucDb.NguoiXuLyId == taiKhoan.TaiKhoanId || lichSuXacThucDb.TaiKhoanId == taiKhoan.TaiKhoanId);
            if (!coLichSuGiaoDich) nguCanhDuLieu.DanhSachTaiKhoan.Remove(taiKhoan);
            else
            {
                taiKhoan.Email = $"archived-account-{taiKhoan.TaiKhoanId}@disabled.invalid";
                taiKhoan.HoTen = "Tài khoản lưu trữ";
                taiKhoan.HoatDong = false;
                taiKhoan.MatKhauBam = boBamMatKhau.HashPassword(taiKhoan, Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)));
            }
        }
        await nguCanhDuLieu.SaveChangesAsync();
        if (giaoDichCoSoDuLieu is not null) await giaoDichCoSoDuLieu.CommitAsync();
        return taiKhoanChuSoHuuCu.Count;
    }
}
