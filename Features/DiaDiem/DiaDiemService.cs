using System.Data;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

using DiaDiemEntity = QLSK.Domain.Entities.DiaDiem;
namespace QLSK.Features.DiaDiem;
public class DiaDiemService(ApplicationDbContext db)
{
    public async Task<List<DiaDiemEntity>> DanhSach(bool all = false)
        => await db.DiaDiems.AsNoTracking().Where(x => all || x.HoatDong).OrderBy(x => x.Ten).ToListAsync();

    public async Task<DiaDiemEntity> Tao(DiaDiemDto dto)
    {
        Validate(dto);
        var x = new DiaDiemEntity { Ten = dto.Ten.Trim(), DiaChi = dto.DiaChi.Trim(), SucChua = dto.SucChua, GiaThue = dto.GiaThue, HoatDong = dto.HoatDong };
        db.DiaDiems.Add(x); await db.SaveChangesAsync(); return x;
    }

    public async Task<DiaDiemEntity> Sua(int id, DiaDiemDto dto)
    {
        Validate(dto);
        var x = await db.DiaDiems.FindAsync(id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy địa điểm.");
        x.Ten = dto.Ten.Trim(); x.DiaChi = dto.DiaChi.Trim(); x.SucChua = dto.SucChua; x.GiaThue = dto.GiaThue; x.HoatDong = dto.HoatDong;
        await db.SaveChangesAsync(); return x;
    }

    public async Task<DatDiaDiem> Dat(int userId, DatDiaDiemDto dto)
    {
        if (dto.KetThuc <= dto.BatDau) throw new ApiException(400, "VALIDATION_ERROR", "Thời gian đặt không hợp lệ.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var suKien = await db.SuKiens.Include(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Id == dto.SuKienId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (suKien.BanToChuc.TaiKhoanId != userId) throw new ApiException(403, "FORBIDDEN", "Không phải sự kiện của bạn.");
        if (await db.DatDiaDiems.AnyAsync(x => x.SuKienId == dto.SuKienId)) throw new ApiException(409, "VENUE_ALREADY_BOOKED", "Sự kiện đã có đặt địa điểm.");
        var diaDiem = await db.DiaDiems.SingleOrDefaultAsync(x => x.Id == dto.DiaDiemId && x.HoatDong)
            ?? throw new ApiException(409, "VENUE_INACTIVE", "Địa điểm không hoạt động.");
        if (diaDiem.SucChua < suKien.SoNguoi) throw new ApiException(409, "VENUE_CAPACITY_TOO_SMALL", "Sức chứa địa điểm không đủ.");
        var conflict = await db.DatDiaDiems.AnyAsync(x => x.DiaDiemId == dto.DiaDiemId && x.TrangThai != TrangThaiDatCho.TuChoi && x.TrangThai != TrangThaiDatCho.DaHuy && dto.BatDau < x.KetThuc && dto.KetThuc > x.BatDau);
        if (conflict) throw new ApiException(409, "VENUE_TIME_CONFLICT", "Địa điểm đã được đặt trong khoảng thời gian này.");
        var hours = (decimal)(dto.KetThuc - dto.BatDau).TotalHours;
        var tongTien = Math.Round(diaDiem.GiaThue * Math.Max(hours, 1), 2);
        if (dto.TienCoc > tongTien) throw new ApiException(400, "VALIDATION_ERROR", "Tiền cọc không được vượt tổng tiền.");
        var dat = new DatDiaDiem { DiaDiemId = dto.DiaDiemId, SuKienId = dto.SuKienId, BatDau = dto.BatDau, KetThuc = dto.KetThuc, TienCoc = dto.TienCoc, TongTien = tongTien };
        db.DatDiaDiems.Add(dat); await db.SaveChangesAsync(); await tx.CommitAsync(); return dat;
    }

    public async Task<List<DatDiaDiem>> DanhSachDat() => await db.DatDiaDiems.AsNoTracking().Include(x => x.DiaDiem).Include(x => x.SuKien).OrderByDescending(x => x.Id).ToListAsync();

    public async Task XacNhan(int id, bool dongY)
    {
        var x = await db.DatDiaDiems.FindAsync(id) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đặt địa điểm.");
        if (x.TrangThai != TrangThaiDatCho.ChoXacNhan) throw new ApiException(409, "BOOKING_INVALID_STATE", "Đặt địa điểm không ở trạng thái chờ xác nhận.");
        x.TrangThai = dongY ? TrangThaiDatCho.DaXacNhan : TrangThaiDatCho.TuChoi; await db.SaveChangesAsync();
    }

    public async Task Huy(int userId, int id)
    {
        var x = await db.DatDiaDiems.Include(x => x.SuKien).ThenInclude(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đặt địa điểm.");
        if (x.SuKien.BanToChuc.TaiKhoanId != userId) throw new ApiException(403, "FORBIDDEN", "Không có quyền hủy.");
        if (x.TrangThai is TrangThaiDatCho.HoanThanh or TrangThaiDatCho.DaHuy or TrangThaiDatCho.TuChoi) throw new ApiException(409, "BOOKING_INVALID_STATE", "Không thể hủy đặt địa điểm.");
        x.TrangThai = TrangThaiDatCho.DaHuy; await db.SaveChangesAsync();
    }
    static void Validate(DiaDiemDto dto)
    {
        if (dto.SucChua <= 0 || dto.GiaThue < 0) throw new ApiException(400, "VALIDATION_ERROR", "Sức chứa hoặc giá thuê không hợp lệ.");
    }
}
