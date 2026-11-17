using QLSK.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Auth;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;

using TaiKhoanEntity = QLSK.Domain.Entities.TaiKhoan;
namespace QLSK.Features.TaiKhoan;
public class TaiKhoanService(ApplicationDbContext nguCanhDuLieu, IPasswordHasher<TaiKhoanEntity> boBamMatKhau, JwtService dichVuTokenDangNhap, QLSK.Common.Security.LoginAttemptLimiter? loginLimiter = null)
{
    private static readonly TaiKhoanEntity DummyAccount = new();
    private static readonly string DummyPasswordHash = new PasswordHasher<TaiKhoanEntity>()
        .HashPassword(DummyAccount, "QLSK-dummy-password-not-a-user-credential");
    static TaiKhoanDto ChuyenSangTaiKhoanDto(TaiKhoanEntity taiKhoan) => new(taiKhoan.TaiKhoanId, taiKhoan.HoTen, taiKhoan.Email, taiKhoan.VaiTro.TenVaiTro, taiKhoan.HoatDong, taiKhoan.NgayTao);

    public async Task<TaiKhoanDto> DangKyTaiKhoan(DangKyDto duLieuDangKy)
    {
        var emailTaiKhoan = duLieuDangKy.Email.Trim().ToLowerInvariant();
        if (await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.Email == emailTaiKhoan, nguCanhDuLieu.RequestAborted))
            throw new ApiException(409, "EMAIL_EXISTS", "Email đã tồn tại.");

        var taiKhoan = new TaiKhoanEntity { HoTen = duLieuDangKy.HoTen.Trim(), Email = emailTaiKhoan, VaiTroId = 1, HoatDong = true };
        taiKhoan.MatKhauBam = boBamMatKhau.HashPassword(taiKhoan, duLieuDangKy.MatKhau);
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoan);
        await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);
        await nguCanhDuLieu.Entry(taiKhoan).Reference(taiKhoanDb => taiKhoanDb.VaiTro).LoadAsync(nguCanhDuLieu.RequestAborted);
        return ChuyenSangTaiKhoanDto(taiKhoan);
    }

    public async Task<DangNhapResult> DangNhapTaiKhoan(DangNhapDto duLieuDangNhap)
    {
        var emailTaiKhoan = duLieuDangNhap.Email.Trim().ToLowerInvariant();
        loginLimiter?.Check(emailTaiKhoan);
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.Include(taiKhoanDb => taiKhoanDb.VaiTro).SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.Email == emailTaiKhoan, nguCanhDuLieu.RequestAborted);
        // Unknown accounts still perform password verification, reducing account-enumeration timing differences.
        var ketQuaXacMinhMatKhau = boBamMatKhau.VerifyHashedPassword(taiKhoan ?? DummyAccount, taiKhoan?.MatKhauBam ?? DummyPasswordHash, duLieuDangNhap.MatKhau);
        if (taiKhoan is null || ketQuaXacMinhMatKhau == PasswordVerificationResult.Failed)
            throw new ApiException(401, "LOGIN_FAILED", "Email hoặc mật khẩu không đúng.");
        if (!taiKhoan.HoatDong) throw new ApiException(403, "ACCOUNT_LOCKED", "Tài khoản đã bị khóa.");
        if (ketQuaXacMinhMatKhau == PasswordVerificationResult.SuccessRehashNeeded)
        {
            taiKhoan.MatKhauBam = boBamMatKhau.HashPassword(taiKhoan, duLieuDangNhap.MatKhau);
            await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);
        }
        return new DangNhapResult(dichVuTokenDangNhap.TaoTokenDangNhap(taiKhoan), ChuyenSangTaiKhoanDto(taiKhoan));
    }

    public async Task DangXuatTaiKhoan(int taiKhoanId)
    {
        await nguCanhDuLieu.DanhSachTaiKhoan.Where(t => t.TaiKhoanId == taiKhoanId)
            .ExecuteUpdateAsync(update => update.SetProperty(t => t.TokenVersion, t => t.TokenVersion + 1), nguCanhDuLieu.RequestAborted);
    }

    public async Task DoiMatKhauTaiKhoan(int taiKhoanId, DoiMatKhauDto duLieuDoiMatKhau)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync([taiKhoanId], nguCanhDuLieu.RequestAborted) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        if (boBamMatKhau.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauBam, duLieuDoiMatKhau.MatKhauCu) == PasswordVerificationResult.Failed)
            throw new ApiException(400, "PASSWORD_INVALID", "Mật khẩu hiện tại không đúng.");
        taiKhoan.MatKhauBam = boBamMatKhau.HashPassword(taiKhoan, duLieuDoiMatKhau.MatKhauMoi);
        await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);
    }

    public async Task<TaiKhoanDto> LayTaiKhoanTheoId(int taiKhoanId)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().Include(taiKhoanDb => taiKhoanDb.VaiTro).SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId, nguCanhDuLieu.RequestAborted)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        return ChuyenSangTaiKhoanDto(taiKhoan);
    }

    public async Task<TaiKhoanDto> CapNhatHoTenTaiKhoan(int taiKhoanId, SuaTaiKhoanDto duLieuSuaTaiKhoan)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.Include(taiKhoanDb => taiKhoanDb.VaiTro).SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId, nguCanhDuLieu.RequestAborted)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        taiKhoan.HoTen = duLieuSuaTaiKhoan.HoTen.Trim();
        await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);
        return ChuyenSangTaiKhoanDto(taiKhoan);
    }

    public async Task<List<TaiKhoanDto>> LayDanhSachTaiKhoan(string? tuKhoa, int? vaiTroId, bool? hoatDong, int soTrang = 1, int kichThuocTrang = 20)
    {
        soTrang = Math.Max(soTrang, 1); kichThuocTrang = Math.Clamp(kichThuocTrang, 1, 100);
        var truyVanTaiKhoan = nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().Include(taiKhoanDb => taiKhoanDb.VaiTro).AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa)) truyVanTaiKhoan = truyVanTaiKhoan.Where(taiKhoanDb => taiKhoanDb.HoTen.Contains(tuKhoa) || taiKhoanDb.Email.Contains(tuKhoa));
        if (vaiTroId.HasValue) truyVanTaiKhoan = truyVanTaiKhoan.Where(taiKhoanDb => taiKhoanDb.VaiTroId == vaiTroId);
        if (hoatDong.HasValue) truyVanTaiKhoan = truyVanTaiKhoan.Where(taiKhoanDb => taiKhoanDb.HoatDong == hoatDong);
        return await truyVanTaiKhoan.OrderByDescending(taiKhoanDb => taiKhoanDb.TaiKhoanId).Paginate(nguCanhDuLieu, defaultSize: kichThuocTrang)
            .Select(taiKhoanDb => new TaiKhoanDto(taiKhoanDb.TaiKhoanId, taiKhoanDb.HoTen, taiKhoanDb.Email, taiKhoanDb.VaiTro.TenVaiTro, taiKhoanDb.HoatDong, taiKhoanDb.NgayTao)).ToListAsync(nguCanhDuLieu.RequestAborted);
    }

    public Task KhoaTaiKhoan(int taiKhoanId) => CapNhatTrangThaiTaiKhoan(taiKhoanId, hoatDong: false);

    public Task MoKhoaTaiKhoan(int taiKhoanId) => CapNhatTrangThaiTaiKhoan(taiKhoanId, hoatDong: true);

    private async Task CapNhatTrangThaiTaiKhoan(int taiKhoanId, bool hoatDong)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync([taiKhoanId], nguCanhDuLieu.RequestAborted) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        taiKhoan.HoatDong = hoatDong;
        await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);
    }

    public async Task DoiVaiTroTaiKhoan(int taiKhoanId, int vaiTroId)
    {
        if (!await nguCanhDuLieu.DanhSachVaiTro.AnyAsync(vaiTroDb => vaiTroDb.VaiTroId == vaiTroId, nguCanhDuLieu.RequestAborted)) throw new ApiException(400, "VALIDATION_ERROR", "Vai trò không hợp lệ.");
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync([taiKhoanId], nguCanhDuLieu.RequestAborted) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        taiKhoan.VaiTroId = vaiTroId;
        await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted);
    }
}
