using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

using LoaiVeEntity = QLSK.Domain.Entities.LoaiVe;
namespace QLSK.Features.LoaiVe;
public class LoaiVeService(ApplicationDbContext nguCanhDuLieu)
{
    public async Task<List<LoaiVeEntity>> LayLoaiVeTheoSuKien(int suKienId, int? taiKhoanId = null, bool laNguoiQuanLy = false)
    {
        var suKien = await nguCanhDuLieu.DanhSachSuKien.AsNoTracking()
            .Where(suKienDb => suKienDb.SuKienId == suKienId)
            .Select(suKienDb => new { suKienDb.TrangThai, ChuSoHuuId = suKienDb.BanToChuc.TaiKhoanId })
            .SingleOrDefaultAsync()
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        var laTrangThaiCongKhai = suKien.TrangThai is TrangThaiSuKien.DaDuyet or TrangThaiSuKien.DangDienRa or TrangThaiSuKien.DaKetThuc;
        if (!laTrangThaiCongKhai && !laNguoiQuanLy && suKien.ChuSoHuuId != taiKhoanId)
            throw new ApiException(403, "FORBIDDEN", "Không có quyền xem loại vé của sự kiện.");
        return await nguCanhDuLieu.DanhSachLoaiVe.AsNoTracking().Where(loaiVeDb => loaiVeDb.SuKienId == suKienId).OrderBy(loaiVeDb => loaiVeDb.GiaVe).ToListAsync();
    }

    public async Task<LoaiVeEntity> TaoLoaiVe(int taiKhoanId, int suKienId, LoaiVeDto duLieuLoaiVe, bool laQuanTriVien = false)
    {
        KiemTraDuLieuLoaiVe(duLieuLoaiVe);
        if (!laQuanTriVien) await KiemTraChuSoHuuSuKien(taiKhoanId, suKienId);
        else if (!await nguCanhDuLieu.DanhSachSuKien.AnyAsync(suKienDb => suKienDb.SuKienId == suKienId)) throw new ApiException(404, "NOT_FOUND", "Không tìm thấy sự kiện.");
        if (await nguCanhDuLieu.DanhSachLoaiVe.AnyAsync(loaiVeDb => loaiVeDb.SuKienId == suKienId && loaiVeDb.TenLoaiVe == duLieuLoaiVe.TenLoaiVe.Trim())) throw new ApiException(409, "TICKET_TYPE_EXISTS", "Tên loại vé đã tồn tại trong sự kiện.");
        var loaiVe = new LoaiVeEntity { SuKienId = suKienId, TenLoaiVe = duLieuLoaiVe.TenLoaiVe.Trim(), GiaVe = duLieuLoaiVe.GiaVe, SoLuong = duLieuLoaiVe.SoLuong, ThoiGianMoBan = duLieuLoaiVe.ThoiGianMoBan, ThoiGianDongBan = duLieuLoaiVe.ThoiGianDongBan };
        nguCanhDuLieu.DanhSachLoaiVe.Add(loaiVe); await nguCanhDuLieu.SaveChangesAsync(); return loaiVe;
    }

    public async Task<LoaiVeEntity> CapNhatLoaiVe(int taiKhoanId, int loaiVeId, LoaiVeDto duLieuLoaiVe, bool laQuanTriVien = false)
    {
        KiemTraDuLieuLoaiVe(duLieuLoaiVe);
        var loaiVe = await nguCanhDuLieu.DanhSachLoaiVe.Include(loaiVeDb => loaiVeDb.SuKien).ThenInclude(suKienDb => suKienDb.BanToChuc).SingleOrDefaultAsync(loaiVeDb => loaiVeDb.LoaiVeId == loaiVeId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy loại vé.");
        if (!laQuanTriVien && loaiVe.SuKien.BanToChuc.TaiKhoanId != taiKhoanId) throw new ApiException(403, "FORBIDDEN", "Không có quyền sửa loại vé.");
        if (duLieuLoaiVe.SoLuong < loaiVe.SoLuongDaBan + loaiVe.SoLuongDangGiu) throw new ApiException(409, "TICKET_LIMIT_EXCEEDED", "Số lượng mới nhỏ hơn số vé đã bán/đang giữ.");
        if (await nguCanhDuLieu.DanhSachLoaiVe.AnyAsync(loaiVeDb => loaiVeDb.LoaiVeId != loaiVeId && loaiVeDb.SuKienId == loaiVe.SuKienId && loaiVeDb.TenLoaiVe == duLieuLoaiVe.TenLoaiVe.Trim())) throw new ApiException(409, "TICKET_TYPE_EXISTS", "Tên loại vé đã tồn tại.");
        loaiVe.TenLoaiVe = duLieuLoaiVe.TenLoaiVe.Trim(); loaiVe.GiaVe = duLieuLoaiVe.GiaVe; loaiVe.SoLuong = duLieuLoaiVe.SoLuong; loaiVe.ThoiGianMoBan = duLieuLoaiVe.ThoiGianMoBan; loaiVe.ThoiGianDongBan = duLieuLoaiVe.ThoiGianDongBan;
        await nguCanhDuLieu.SaveChangesAsync(); return loaiVe;
    }

    public async Task MoBanLoaiVe(int taiKhoanId, int loaiVeId)
    {
        var loaiVe = await LayLoaiVeVaKiemTraChuSoHuu(taiKhoanId, loaiVeId);
        if (loaiVe.SuKien.TrangThai != TrangThaiSuKien.DaDuyet) throw new ApiException(409, "EVENT_NOT_APPROVED", "Sự kiện chưa được duyệt.");
        loaiVe.ThoiGianMoBan = DateTime.UtcNow;
        if (loaiVe.ThoiGianDongBan <= loaiVe.ThoiGianMoBan) loaiVe.ThoiGianDongBan = loaiVe.SuKien.ThoiGianBatDau;
        await nguCanhDuLieu.SaveChangesAsync();
    }

    public async Task DongBanLoaiVe(int taiKhoanId, int loaiVeId)
    {
        var loaiVe = await LayLoaiVeVaKiemTraChuSoHuu(taiKhoanId, loaiVeId);
        loaiVe.ThoiGianDongBan = DateTime.UtcNow;
        if (loaiVe.ThoiGianDongBan <= loaiVe.ThoiGianMoBan) loaiVe.ThoiGianMoBan = loaiVe.ThoiGianDongBan.AddSeconds(-1);
        await nguCanhDuLieu.SaveChangesAsync();
    }

    async Task KiemTraChuSoHuuSuKien(int taiKhoanId, int suKienId)
    {
        var laChuSoHuuSuKien = await nguCanhDuLieu.DanhSachSuKien.AnyAsync(suKienDb => suKienDb.SuKienId == suKienId && suKienDb.BanToChuc.TaiKhoanId == taiKhoanId);
        if (!laChuSoHuuSuKien) throw new ApiException(403, "FORBIDDEN", "Không phải sự kiện của bạn.");
    }
    async Task<LoaiVeEntity> LayLoaiVeVaKiemTraChuSoHuu(int taiKhoanId, int loaiVeId)
    {
        var loaiVe = await nguCanhDuLieu.DanhSachLoaiVe.Include(loaiVeDb => loaiVeDb.SuKien).ThenInclude(suKienDb => suKienDb.BanToChuc).SingleOrDefaultAsync(loaiVeDb => loaiVeDb.LoaiVeId == loaiVeId) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy loại vé.");
        if (loaiVe.SuKien.BanToChuc.TaiKhoanId != taiKhoanId) throw new ApiException(403, "FORBIDDEN", "Không có quyền.");
        return loaiVe;
    }
    static void KiemTraDuLieuLoaiVe(LoaiVeDto duLieuLoaiVe)
    {
        if (duLieuLoaiVe.GiaVe < 0 || duLieuLoaiVe.GiaVe != decimal.Truncate(duLieuLoaiVe.GiaVe) || duLieuLoaiVe.SoLuong <= 0 || duLieuLoaiVe.ThoiGianDongBan <= duLieuLoaiVe.ThoiGianMoBan) throw new ApiException(400, "VALIDATION_ERROR", "Thông tin loại vé không hợp lệ. Giá vé phải là số VND nguyên.");
    }
}
