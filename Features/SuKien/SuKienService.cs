using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

using SuKienEntity = QLSK.Domain.Entities.SuKien;
namespace QLSK.Features.SuKien;
public class SuKienService(ApplicationDbContext db)
{
    public async Task<List<LoaiSuKien>> LayLoai(bool all = false)
        => await db.LoaiSuKiens.AsNoTracking().Where(x => all || x.HoatDong).OrderBy(x => x.Ten).ToListAsync();

    public async Task<LoaiSuKien> TaoLoai(LoaiSuKienDto dto)
    {
        if (await db.LoaiSuKiens.AnyAsync(x => x.Ten == dto.Ten.Trim())) throw new ApiException(409, "TYPE_EXISTS", "Loại sự kiện đã tồn tại.");
        var x = new LoaiSuKien { Ten = dto.Ten.Trim(), MoTa = dto.MoTa.Trim(), HoatDong = dto.HoatDong };
        db.LoaiSuKiens.Add(x); await db.SaveChangesAsync(); return x;
    }

    public async Task<LoaiSuKien> SuaLoai(int id, LoaiSuKienDto dto)
    {
        var x = await db.LoaiSuKiens.FindAsync(id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy loại sự kiện.");
        if (await db.LoaiSuKiens.AnyAsync(y => y.Id != id && y.Ten == dto.Ten.Trim())) throw new ApiException(409, "TYPE_EXISTS", "Loại sự kiện đã tồn tại.");
        x.Ten = dto.Ten.Trim(); x.MoTa = dto.MoTa.Trim(); x.HoatDong = dto.HoatDong; await db.SaveChangesAsync(); return x;
    }

    public async Task<List<SuKienEntity>> DanhSach(bool manager = false)
    {
        var q = db.SuKiens.AsNoTracking().Include(x => x.LoaiSuKien).Include(x => x.BanToChuc).AsQueryable();
        if (!manager) q = q.Where(x => x.TrangThai == TrangThaiSuKien.DaDuyet || x.TrangThai == TrangThaiSuKien.DangDienRa || x.TrangThai == TrangThaiSuKien.DaKetThuc);
        return await q.OrderBy(x => x.BatDau).ToListAsync();
    }

    public async Task<SuKienEntity> Lay(int id, int? userId = null, bool manager = false)
    {
        var x = await db.SuKiens.AsNoTracking().Include(x => x.LoaiSuKien).Include(x => x.BanToChuc).ThenInclude(x => x.TaiKhoan)
            .Include(x => x.DatDiaDiem).ThenInclude(x => x!.DiaDiem).Include(x => x.LoaiVes).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        var publicState = x.TrangThai is TrangThaiSuKien.DaDuyet or TrangThaiSuKien.DangDienRa or TrangThaiSuKien.DaKetThuc;
        var owner = userId.HasValue && x.BanToChuc.TaiKhoanId == userId.Value;
        if (!publicState && !manager && !owner) throw new ApiException(403, "FORBIDDEN", "Không có quyền xem sự kiện.");
        return x;
    }

    public async Task<List<SuKienEntity>> CuaToi(int userId)
    {
        var btcId = await LayBanToChucId(userId);
        return await db.SuKiens.AsNoTracking().Where(x => x.BanToChucId == btcId).OrderByDescending(x => x.Id).ToListAsync();
    }

    public async Task<SuKienEntity> Tao(int userId, TaoSuKienDto dto)
    {
        Validate(dto);
        var btcId = await LayBanToChucId(userId);
        if (!await db.LoaiSuKiens.AnyAsync(x => x.Id == dto.LoaiSuKienId && x.HoatDong)) throw new ApiException(400, "VALIDATION_ERROR", "Loại sự kiện không hoạt động.");
        var x = new SuKienEntity { BanToChucId = btcId, LoaiSuKienId = dto.LoaiSuKienId, Ten = dto.Ten.Trim(), MoTa = dto.MoTa.Trim(), BatDau = dto.BatDau, KetThuc = dto.KetThuc, SoNguoi = dto.SoNguoi, TrangThai = TrangThaiSuKien.ChoDuyet };
        db.SuKiens.Add(x); await db.SaveChangesAsync(); return x;
    }

    public async Task<SuKienEntity> Sua(int userId, int id, TaoSuKienDto dto)
    {
        Validate(dto);
        var x = await LayCuaChu(userId, id);
        if (x.TrangThai is not (TrangThaiSuKien.ChoDuyet or TrangThaiSuKien.TuChoi)) throw new ApiException(409, "EVENT_INVALID_STATE", "Không thể sửa sự kiện ở trạng thái hiện tại.");
        if (!await db.LoaiSuKiens.AnyAsync(y => y.Id == dto.LoaiSuKienId && y.HoatDong)) throw new ApiException(400, "VALIDATION_ERROR", "Loại sự kiện không hoạt động.");
        x.LoaiSuKienId = dto.LoaiSuKienId; x.Ten = dto.Ten.Trim(); x.MoTa = dto.MoTa.Trim(); x.BatDau = dto.BatDau; x.KetThuc = dto.KetThuc; x.SoNguoi = dto.SoNguoi; x.TrangThai = TrangThaiSuKien.ChoDuyet;
        await db.SaveChangesAsync(); return x;
    }

    public async Task<SuKienEntity> GuiDuyet(int userId, int id)
    {
        var x = await LayCuaChu(userId, id);
        if (x.TrangThai is not (TrangThaiSuKien.ChoDuyet or TrangThaiSuKien.TuChoi)) throw new ApiException(409, "EVENT_INVALID_STATE", "Sự kiện không thể gửi duyệt.");
        x.TrangThai = TrangThaiSuKien.ChoDuyet; await db.SaveChangesAsync(); return x;
    }

    public async Task Duyet(int managerId, int id)
    {
        var x = await db.SuKiens.Include(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Id == id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (x.TrangThai != TrangThaiSuKien.ChoDuyet) throw new ApiException(409, "EVENT_INVALID_STATE", "Sự kiện không ở trạng thái chờ duyệt.");
        if (x.BanToChuc.TaiKhoanId == managerId) throw new ApiException(403, "FORBIDDEN", "Người tạo không được tự duyệt sự kiện.");
        x.TrangThai = TrangThaiSuKien.DaDuyet; await db.SaveChangesAsync();
    }

    public async Task TuChoi(int managerId, int id, string lyDo)
    {
        var x = await db.SuKiens.Include(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Id == id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (x.TrangThai != TrangThaiSuKien.ChoDuyet) throw new ApiException(409, "EVENT_INVALID_STATE", "Sự kiện không ở trạng thái chờ duyệt.");
        if (x.BanToChuc.TaiKhoanId == managerId) throw new ApiException(403, "FORBIDDEN", "Người tạo không được tự xử lý sự kiện.");
        x.TrangThai = TrangThaiSuKien.TuChoi;
        x.MoTa = string.IsNullOrWhiteSpace(lyDo) ? x.MoTa : $"{x.MoTa}\n[Lý do từ chối: {lyDo.Trim()}]";
        await db.SaveChangesAsync();
    }

    public async Task Huy(int userId, int id)
    {
        var x = await LayCuaChu(userId, id);
        if (x.TrangThai is TrangThaiSuKien.DaKetThuc or TrangThaiSuKien.DaHuy) throw new ApiException(409, "EVENT_INVALID_STATE", "Không thể hủy sự kiện.");
        x.TrangThai = TrangThaiSuKien.DaHuy; await db.SaveChangesAsync();
    }

    async Task<int> LayBanToChucId(int userId)
    {
        var btc = await db.BanToChucs.AsNoTracking().SingleOrDefaultAsync(x => x.TaiKhoanId == userId)
            ?? throw new ApiException(403, "ORGANIZER_NOT_VERIFIED", "Ban tổ chức chưa được xác thực.");
        return btc.Id;
    }
    async Task<SuKienEntity> LayCuaChu(int userId, int id)
    {
        var x = await db.SuKiens.Include(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Id == id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (x.BanToChuc.TaiKhoanId != userId) throw new ApiException(403, "FORBIDDEN", "Không phải sự kiện của bạn.");
        return x;
    }
    static void Validate(TaoSuKienDto dto)
    {
        if (dto.KetThuc <= dto.BatDau) throw new ApiException(400, "VALIDATION_ERROR", "Thời gian kết thúc phải sau thời gian bắt đầu.");
        if (dto.SoNguoi <= 0) throw new ApiException(400, "VALIDATION_ERROR", "Số người phải lớn hơn 0.");
    }
}
