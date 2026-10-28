using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Auth;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;

using TaiKhoanEntity = QLSK.Domain.Entities.TaiKhoan;
namespace QLSK.Features.TaiKhoan;
public class TaiKhoanService(ApplicationDbContext db, IPasswordHasher<TaiKhoanEntity> hasher, JwtService jwt)
{
    static TaiKhoanDto Map(TaiKhoanEntity x) => new(x.Id, x.HoTen, x.Email, x.VaiTro.Ten, x.HoatDong, x.NgayTao);

    public async Task<TaiKhoanDto> DangKy(DangKyDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.TaiKhoans.AnyAsync(x => x.Email == email))
            throw new ApiException(409, "EMAIL_EXISTS", "Email đã tồn tại.");

        var user = new TaiKhoanEntity { HoTen = dto.HoTen.Trim(), Email = email, VaiTroId = 1, HoatDong = true };
        user.MatKhau = hasher.HashPassword(user, dto.MatKhau);
        db.TaiKhoans.Add(user);
        await db.SaveChangesAsync();
        await db.Entry(user).Reference(x => x.VaiTro).LoadAsync();
        return Map(user);
    }

    public async Task<DangNhapResult> DangNhap(DangNhapDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await db.TaiKhoans.Include(x => x.VaiTro).SingleOrDefaultAsync(x => x.Email == email)
            ?? throw new ApiException(401, "LOGIN_FAILED", "Email hoặc mật khẩu không đúng.");
        if (!user.HoatDong) throw new ApiException(403, "ACCOUNT_LOCKED", "Tài khoản đã bị khóa.");
        var ok = hasher.VerifyHashedPassword(user, user.MatKhau, dto.MatKhau);
        if (ok == PasswordVerificationResult.Failed)
            throw new ApiException(401, "LOGIN_FAILED", "Email hoặc mật khẩu không đúng.");
        return new DangNhapResult(jwt.Tao(user), Map(user));
    }

    public async Task DoiMatKhau(int id, DoiMatKhauDto dto)
    {
        var user = await db.TaiKhoans.FindAsync(id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        if (hasher.VerifyHashedPassword(user, user.MatKhau, dto.MatKhauCu) == PasswordVerificationResult.Failed)
            throw new ApiException(400, "PASSWORD_INVALID", "Mật khẩu hiện tại không đúng.");
        user.MatKhau = hasher.HashPassword(user, dto.MatKhauMoi);
        await db.SaveChangesAsync();
    }

    public async Task<TaiKhoanDto> Lay(int id)
    {
        var user = await db.TaiKhoans.AsNoTracking().Include(x => x.VaiTro).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        return Map(user);
    }

    public async Task<TaiKhoanDto> Sua(int id, SuaTaiKhoanDto dto)
    {
        var user = await db.TaiKhoans.Include(x => x.VaiTro).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        user.HoTen = dto.HoTen.Trim();
        await db.SaveChangesAsync();
        return Map(user);
    }

    public async Task<List<TaiKhoanDto>> DanhSach(string? q, int? vaiTroId, bool? hoatDong, int page = 1, int size = 20)
    {
        page = Math.Max(page, 1); size = Math.Clamp(size, 1, 100);
        var query = db.TaiKhoans.AsNoTracking().Include(x => x.VaiTro).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.HoTen.Contains(q) || x.Email.Contains(q));
        if (vaiTroId.HasValue) query = query.Where(x => x.VaiTroId == vaiTroId);
        if (hoatDong.HasValue) query = query.Where(x => x.HoatDong == hoatDong);
        return await query.OrderByDescending(x => x.Id).Skip((page - 1) * size).Take(size)
            .Select(x => new TaiKhoanDto(x.Id, x.HoTen, x.Email, x.VaiTro.Ten, x.HoatDong, x.NgayTao)).ToListAsync();
    }

    public async Task Khoa(int id, bool khoa)
    {
        var user = await db.TaiKhoans.FindAsync(id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        user.HoatDong = !khoa;
        await db.SaveChangesAsync();
    }

    public async Task DoiVaiTro(int id, int vaiTroId)
    {
        if (!await db.VaiTros.AnyAsync(x => x.Id == vaiTroId)) throw new ApiException(400, "VALIDATION_ERROR", "Vai trò không hợp lệ.");
        var user = await db.TaiKhoans.FindAsync(id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy tài khoản.");
        user.VaiTroId = vaiTroId;
        await db.SaveChangesAsync();
    }
}
