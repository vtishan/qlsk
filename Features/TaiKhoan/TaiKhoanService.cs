using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Auth;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;

using TaiKhoanEntity = QLSK.Domain.Entities.TaiKhoan;
namespace QLSK.Features.TaiKhoan;
public class TaiKhoanService(ApplicationDbContext nguCanhDuLieu, IPasswordHasher<TaiKhoanEntity> boBamMatKhau, JwtService dichVuTokenDangNhap)
{
    static TaiKhoanDto ChuyenSangTaiKhoanDto(TaiKhoanEntity taiKhoan) => new(taiKhoan.TaiKhoanId, taiKhoan.HoTen, taiKhoan.Email, taiKhoan.VaiTro.TenVaiTro, taiKhoan.HoatDong, taiKhoan.NgayTao);

    public async Task<TaiKhoanDto> DangKyTaiKhoan(DangKyDto duLieuDangKy)
    {
        var emailTaiKhoan = duLieuDangKy.Email.Trim().ToLowerInvariant();
        if (await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanDb => taiKhoanDb.Email == emailTaiKhoan))
            throw new ApiException(409, "EMAIL_EXISTS", "Email đã tồn tại.");

        var taiKhoan = new TaiKhoanEntity { HoTen = duLieuDangKy.HoTen.Trim(), Email = emailTaiKhoan, VaiTroId = 1, HoatDong = true };
        taiKhoan.MatKhauBam = boBamMatKhau.HashPassword(taiKhoan, duLieuDangKy.MatKhau);
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoan);
        await nguCanhDuLieu.SaveChangesAsync();
        await nguCanhDuLieu.Entry(taiKhoan).Reference(taiKhoanDb => taiKhoanDb.VaiTro).LoadAsync();
        return ChuyenSangTaiKhoanDto(taiKhoan);
    }

    public async Task<DangNhapResult> DangNhapTaiKhoan(DangNhapDto duLieuDangNhap)
    {
        var emailTaiKhoan = duLieuDangNhap.Email.Trim().ToLowerInvariant();
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.Include(taiKhoanDb => taiKhoanDb.VaiTro).SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.Email == emailTaiKhoan)
            ?? throw new ApiException(401, "LOGIN_FAILED", "Email hoặc mật khẩu không đúng.");
        if (!taiKhoan.HoatDong) throw new ApiException(403, "ACCOUNT_LOCKED", "Tài khoản đã bị khóa.");
        var ketQuaXacMinhMatKhau = boBamMatKhau.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauBam, duLieuDangNhap.MatKhau);
        if (ketQuaXacMinhMatKhau == PasswordVerificationResult.Failed)
            throw new ApiException(401, "LOGIN_FAILED", "Email hoặc mật khẩu không đúng.");
        return new DangNhapResult(dichVuTokenDangNhap.TaoTokenDangNhap(taiKhoan), ChuyenSangTaiKhoanDto(taiKhoan));
    }

    public async Task DoiMatKhauTaiKhoan(int taiKhoanId, DoiMatKhauDto duLieuDoiMatKhau)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(taiKhoanId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        if (boBamMatKhau.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauBam, duLieuDoiMatKhau.MatKhauCu) == PasswordVerificationResult.Failed)
            throw new ApiException(400, "PASSWORD_INVALID", "Mật khẩu hiện tại không đúng.");
        taiKhoan.MatKhauBam = boBamMatKhau.HashPassword(taiKhoan, duLieuDoiMatKhau.MatKhauMoi);
        await nguCanhDuLieu.SaveChangesAsync();
    }

    public async Task<TaiKhoanDto> LayTaiKhoanTheoId(int taiKhoanId)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().Include(taiKhoanDb => taiKhoanDb.VaiTro).SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        return ChuyenSangTaiKhoanDto(taiKhoan);
    }

    public async Task<TaiKhoanDto> CapNhatHoTenTaiKhoan(int taiKhoanId, SuaTaiKhoanDto duLieuSuaTaiKhoan)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.Include(taiKhoanDb => taiKhoanDb.VaiTro).SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        taiKhoan.HoTen = duLieuSuaTaiKhoan.HoTen.Trim();
        await nguCanhDuLieu.SaveChangesAsync();
        return ChuyenSangTaiKhoanDto(taiKhoan);
    }

    public async Task<List<TaiKhoanDto>> LayDanhSachTaiKhoan(string? tuKhoa, int? vaiTroId, bool? hoatDong, int soTrang = 1, int kichThuocTrang = 20)
    {
        soTrang = Math.Max(soTrang, 1); kichThuocTrang = Math.Clamp(kichThuocTrang, 1, 100);
        var truyVanTaiKhoan = nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().Include(taiKhoanDb => taiKhoanDb.VaiTro).AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa)) truyVanTaiKhoan = truyVanTaiKhoan.Where(taiKhoanDb => taiKhoanDb.HoTen.Contains(tuKhoa) || taiKhoanDb.Email.Contains(tuKhoa));
        if (vaiTroId.HasValue) truyVanTaiKhoan = truyVanTaiKhoan.Where(taiKhoanDb => taiKhoanDb.VaiTroId == vaiTroId);
        if (hoatDong.HasValue) truyVanTaiKhoan = truyVanTaiKhoan.Where(taiKhoanDb => taiKhoanDb.HoatDong == hoatDong);
        return await truyVanTaiKhoan.OrderByDescending(taiKhoanDb => taiKhoanDb.TaiKhoanId).Skip((soTrang - 1) * kichThuocTrang).Take(kichThuocTrang)
            .Select(taiKhoanDb => new TaiKhoanDto(taiKhoanDb.TaiKhoanId, taiKhoanDb.HoTen, taiKhoanDb.Email, taiKhoanDb.VaiTro.TenVaiTro, taiKhoanDb.HoatDong, taiKhoanDb.NgayTao)).ToListAsync();
    }

    public Task KhoaTaiKhoan(int taiKhoanId) => CapNhatTrangThaiTaiKhoan(taiKhoanId, hoatDong: false);

    public Task MoKhoaTaiKhoan(int taiKhoanId) => CapNhatTrangThaiTaiKhoan(taiKhoanId, hoatDong: true);

    private async Task CapNhatTrangThaiTaiKhoan(int taiKhoanId, bool hoatDong)
    {
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(taiKhoanId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        taiKhoan.HoatDong = hoatDong;
        await nguCanhDuLieu.SaveChangesAsync();
    }

    public async Task DoiVaiTroTaiKhoan(int taiKhoanId, int vaiTroId)
    {
        if (!await nguCanhDuLieu.DanhSachVaiTro.AnyAsync(vaiTroDb => vaiTroDb.VaiTroId == vaiTroId)) throw new ApiException(400, "VALIDATION_ERROR", "Vai trò không hợp lệ.");
        var taiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.FindAsync(taiKhoanId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        taiKhoan.VaiTroId = vaiTroId;
        await nguCanhDuLieu.SaveChangesAsync();
    }
}
