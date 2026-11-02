using System.Data;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

using DiaDiemEntity = QLSK.Domain.Entities.DiaDiem;
namespace QLSK.Features.DiaDiem;
public class DiaDiemService(ApplicationDbContext nguCanhDuLieu)
{
    public async Task<List<DiaDiemEntity>> LayDanhSachDiaDiem(bool baoGomNgungHoatDong = false)
        => await nguCanhDuLieu.DanhSachDiaDiem.AsNoTracking().Where(diaDiemDb => baoGomNgungHoatDong || diaDiemDb.HoatDong).OrderBy(diaDiemDb => diaDiemDb.TenDiaDiem).ToListAsync();

    public async Task<DiaDiemEntity> TaoDiaDiem(DiaDiemDto duLieuDiaDiem)
    {
        KiemTraDuLieuDiaDiem(duLieuDiaDiem);
        var diaDiem = new DiaDiemEntity { TenDiaDiem = duLieuDiaDiem.TenDiaDiem.Trim(), DiaChi = duLieuDiaDiem.DiaChi.Trim(), SucChua = duLieuDiaDiem.SucChua, GiaThue = duLieuDiaDiem.GiaThue, HoatDong = duLieuDiaDiem.HoatDong };
        diaDiem.HinhAnh = duLieuDiaDiem.HinhAnh;
        nguCanhDuLieu.DanhSachDiaDiem.Add(diaDiem); await nguCanhDuLieu.SaveChangesAsync(); return diaDiem;
    }

    public async Task<DiaDiemEntity> CapNhatDiaDiem(int diaDiemId, DiaDiemDto duLieuDiaDiem)
    {
        KiemTraDuLieuDiaDiem(duLieuDiaDiem);
        var diaDiem = await nguCanhDuLieu.DanhSachDiaDiem.FindAsync(diaDiemId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy địa điểm.");
        diaDiem.TenDiaDiem = duLieuDiaDiem.TenDiaDiem.Trim(); diaDiem.DiaChi = duLieuDiaDiem.DiaChi.Trim(); diaDiem.SucChua = duLieuDiaDiem.SucChua; diaDiem.GiaThue = duLieuDiaDiem.GiaThue; diaDiem.HoatDong = duLieuDiaDiem.HoatDong;
        diaDiem.HinhAnh = duLieuDiaDiem.HinhAnh;
        await nguCanhDuLieu.SaveChangesAsync(); return diaDiem;
    }

    public async Task<DatDiaDiem> DatDiaDiemChoSuKien(int taiKhoanId, DatDiaDiemDto duLieuDatDiaDiem)
    {
        if (duLieuDatDiaDiem.ThoiGianKetThuc <= duLieuDatDiaDiem.ThoiGianBatDau) throw new ApiException(400, "VALIDATION_ERROR", "Thời gian đặt không hợp lệ.");
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var suKien = await nguCanhDuLieu.DanhSachSuKien.Include(suKienDb => suKienDb.BanToChuc).SingleOrDefaultAsync(suKienDb => suKienDb.SuKienId == duLieuDatDiaDiem.SuKienId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (suKien.BanToChuc.TaiKhoanId != taiKhoanId) throw new ApiException(403, "FORBIDDEN", "Không phải sự kiện của bạn.");
        if (await nguCanhDuLieu.DanhSachDatDiaDiem.AnyAsync(datDiaDiemDb => datDiaDiemDb.SuKienId == duLieuDatDiaDiem.SuKienId)) throw new ApiException(409, "VENUE_ALREADY_BOOKED", "Sự kiện đã có đặt địa điểm.");
        var diaDiem = await nguCanhDuLieu.DanhSachDiaDiem.SingleOrDefaultAsync(diaDiemDb => diaDiemDb.DiaDiemId == duLieuDatDiaDiem.DiaDiemId && diaDiemDb.HoatDong)
            ?? throw new ApiException(409, "VENUE_INACTIVE", "Địa điểm không hoạt động.");
        if (diaDiem.SucChua < suKien.SoNguoi) throw new ApiException(409, "VENUE_CAPACITY_TOO_SMALL", "Sức chứa địa điểm không đủ.");
        var trungLichDat = await nguCanhDuLieu.DanhSachDatDiaDiem.AnyAsync(datDiaDiemDb => datDiaDiemDb.DiaDiemId == duLieuDatDiaDiem.DiaDiemId && datDiaDiemDb.TrangThai != TrangThaiDatCho.TuChoi && datDiaDiemDb.TrangThai != TrangThaiDatCho.DaHuy && duLieuDatDiaDiem.ThoiGianBatDau < datDiaDiemDb.ThoiGianKetThuc && duLieuDatDiaDiem.ThoiGianKetThuc > datDiaDiemDb.ThoiGianBatDau);
        if (trungLichDat) throw new ApiException(409, "VENUE_TIME_CONFLICT", "Địa điểm đã được đặt trong khoảng thời gian này.");
        var soGioThue = (decimal)(duLieuDatDiaDiem.ThoiGianKetThuc - duLieuDatDiaDiem.ThoiGianBatDau).TotalHours;
        var tongTien = Math.Round(diaDiem.GiaThue * Math.Max(soGioThue, 1), 2);
        if (duLieuDatDiaDiem.TienCoc > tongTien) throw new ApiException(400, "VALIDATION_ERROR", "Tiền cọc không được vượt tổng tiền.");
        var datDiaDiem = new DatDiaDiem { DiaDiemId = duLieuDatDiaDiem.DiaDiemId, SuKienId = duLieuDatDiaDiem.SuKienId, ThoiGianBatDau = duLieuDatDiaDiem.ThoiGianBatDau, ThoiGianKetThuc = duLieuDatDiaDiem.ThoiGianKetThuc, TienCoc = duLieuDatDiaDiem.TienCoc, TongTien = tongTien };
        nguCanhDuLieu.DanhSachDatDiaDiem.Add(datDiaDiem); await nguCanhDuLieu.SaveChangesAsync(); await giaoDich.CommitAsync(); return datDiaDiem;
    }

    public async Task<List<DatDiaDiem>> LayDanhSachDatDiaDiem() => await nguCanhDuLieu.DanhSachDatDiaDiem.AsNoTracking().Include(datDiaDiemDb => datDiaDiemDb.DiaDiem).Include(datDiaDiemDb => datDiaDiemDb.SuKien).OrderByDescending(datDiaDiemDb => datDiaDiemDb.DatDiaDiemId).ToListAsync();

    public Task XacNhanDatDiaDiem(int datDiaDiemId) => CapNhatQuyetDinhDatDiaDiem(datDiaDiemId, TrangThaiDatCho.DaXacNhan);

    public Task TuChoiDatDiaDiem(int datDiaDiemId) => CapNhatQuyetDinhDatDiaDiem(datDiaDiemId, TrangThaiDatCho.TuChoi);

    private async Task CapNhatQuyetDinhDatDiaDiem(int datDiaDiemId, TrangThaiDatCho trangThai)
    {
        var datDiaDiem = await nguCanhDuLieu.DanhSachDatDiaDiem.FindAsync(datDiaDiemId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đặt địa điểm.");
        if (datDiaDiem.TrangThai != TrangThaiDatCho.ChoXacNhan) throw new ApiException(409, "BOOKING_INVALID_STATE", "Đặt địa điểm không ở trạng thái chờ xác nhận.");
        datDiaDiem.TrangThai = trangThai; await nguCanhDuLieu.SaveChangesAsync();
    }

    public async Task HuyDatDiaDiem(int taiKhoanId, int datDiaDiemId)
    {
        var datDiaDiem = await nguCanhDuLieu.DanhSachDatDiaDiem.Include(datDiaDiemDb => datDiaDiemDb.SuKien).ThenInclude(suKienDb => suKienDb.BanToChuc).SingleOrDefaultAsync(datDiaDiemDb => datDiaDiemDb.DatDiaDiemId == datDiaDiemId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đặt địa điểm.");
        if (datDiaDiem.SuKien.BanToChuc.TaiKhoanId != taiKhoanId) throw new ApiException(403, "FORBIDDEN", "Không có quyền hủy.");
        if (datDiaDiem.TrangThai is TrangThaiDatCho.HoanThanh or TrangThaiDatCho.DaHuy or TrangThaiDatCho.TuChoi) throw new ApiException(409, "BOOKING_INVALID_STATE", "Không thể hủy đặt địa điểm.");
        datDiaDiem.TrangThai = TrangThaiDatCho.DaHuy; await nguCanhDuLieu.SaveChangesAsync();
    }
    static void KiemTraDuLieuDiaDiem(DiaDiemDto duLieuDiaDiem)
    {
        if (duLieuDiaDiem.SucChua <= 0 || duLieuDiaDiem.GiaThue < 0) throw new ApiException(400, "VALIDATION_ERROR", "Sức chứa hoặc giá thuê không hợp lệ.");
    }
}
