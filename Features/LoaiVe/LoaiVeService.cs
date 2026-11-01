using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

using LoaiVeEntity = QLSK.Domain.Entities.LoaiVe;
namespace QLSK.Features.LoaiVe;
public class LoaiVeService(ApplicationDbContext db)
{
    public async Task<List<LoaiVeEntity>> TheoSuKien(int suKienId, int? userId = null, bool manager = false)
    {
        var suKien = await db.SuKiens.AsNoTracking()
            .Where(x => x.Id == suKienId)
            .Select(x => new { x.TrangThai, ChuSoHuuId = x.BanToChuc.TaiKhoanId })
            .SingleOrDefaultAsync()
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        var congKhai = suKien.TrangThai is TrangThaiSuKien.DaDuyet or TrangThaiSuKien.DangDienRa or TrangThaiSuKien.DaKetThuc;
        if (!congKhai && !manager && suKien.ChuSoHuuId != userId)
            throw new ApiException(403, "FORBIDDEN", "Không có quyền xem loại vé của sự kiện.");
        return await db.LoaiVes.AsNoTracking().Where(x => x.SuKienId == suKienId).OrderBy(x => x.Gia).ToListAsync();
    }

    public async Task<LoaiVeEntity> Tao(int userId, int suKienId, LoaiVeDto dto, bool admin = false)
    {
        Validate(dto);
        if (!admin) await CheckOwner(userId, suKienId);
        else if (!await db.SuKiens.AnyAsync(x => x.Id == suKienId)) throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (await db.LoaiVes.AnyAsync(x => x.SuKienId == suKienId && x.Ten == dto.Ten.Trim())) throw new ApiException(409, "TICKET_TYPE_EXISTS", "Tên loại vé đã tồn tại trong sự kiện.");
        var x = new LoaiVeEntity { SuKienId = suKienId, Ten = dto.Ten.Trim(), Gia = dto.Gia, SoLuong = dto.SoLuong, MoBan = dto.MoBan, DongBan = dto.DongBan };
        db.LoaiVes.Add(x); await db.SaveChangesAsync(); return x;
    }

    public async Task<LoaiVeEntity> Sua(int userId, int id, LoaiVeDto dto, bool admin = false)
    {
        Validate(dto);
        var x = await db.LoaiVes.Include(x => x.SuKien).ThenInclude(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Id == id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy loại vé.");
        if (!admin && x.SuKien.BanToChuc.TaiKhoanId != userId) throw new ApiException(403, "FORBIDDEN", "Không có quyền sửa loại vé.");
        if (dto.SoLuong < x.DaBan + x.DangGiu) throw new ApiException(409, "TICKET_LIMIT_EXCEEDED", "Số lượng mới nhỏ hơn số vé đã bán/đang giữ.");
        if (await db.LoaiVes.AnyAsync(y => y.Id != id && y.SuKienId == x.SuKienId && y.Ten == dto.Ten.Trim())) throw new ApiException(409, "TICKET_TYPE_EXISTS", "Tên loại vé đã tồn tại.");
        x.Ten = dto.Ten.Trim(); x.Gia = dto.Gia; x.SoLuong = dto.SoLuong; x.MoBan = dto.MoBan; x.DongBan = dto.DongBan;
        await db.SaveChangesAsync(); return x;
    }

    public async Task MoBan(int userId, int id)
    {
        var x = await GetOwned(userId, id);
        if (x.SuKien.TrangThai != TrangThaiSuKien.DaDuyet) throw new ApiException(409, "EVENT_NOT_APPROVED", "Sự kiện chưa được duyệt.");
        x.MoBan = DateTime.UtcNow;
        if (x.DongBan <= x.MoBan) x.DongBan = x.SuKien.BatDau;
        await db.SaveChangesAsync();
    }

    public async Task DongBan(int userId, int id)
    {
        var x = await GetOwned(userId, id);
        x.DongBan = DateTime.UtcNow;
        if (x.DongBan <= x.MoBan) x.MoBan = x.DongBan.AddSeconds(-1);
        await db.SaveChangesAsync();
    }

    async Task CheckOwner(int userId, int suKienId)
    {
        var ok = await db.SuKiens.AnyAsync(x => x.Id == suKienId && x.BanToChuc.TaiKhoanId == userId);
        if (!ok) throw new ApiException(403, "FORBIDDEN", "Không phải sự kiện của bạn.");
    }
    async Task<LoaiVeEntity> GetOwned(int userId, int id)
    {
        var x = await db.LoaiVes.Include(x => x.SuKien).ThenInclude(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Id == id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy loại vé.");
        if (x.SuKien.BanToChuc.TaiKhoanId != userId) throw new ApiException(403, "FORBIDDEN", "Không có quyền.");
        return x;
    }
    static void Validate(LoaiVeDto dto)
    {
        if (dto.Gia < 0 || dto.Gia != decimal.Truncate(dto.Gia) || dto.SoLuong <= 0 || dto.DongBan <= dto.MoBan) throw new ApiException(400, "VALIDATION_ERROR", "Thông tin loại vé không hợp lệ. Giá vé phải là số VND nguyên.");
    }
}
