using System.Data;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Common.Inventory;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

using DonVeEntity = QLSK.Domain.Entities.DonVe;
namespace QLSK.Features.DonVe;
public class DonVeService(ApplicationDbContext nguCanhDuLieu, IConfiguration cauHinhUngDung)
{
    public async Task<DonVeEntity> TaoDonVaGiuVe(int taiKhoanId, TaoDonDto duLieuTaoDon)
    {
        if (duLieuTaoDon.ChiTietDatVe.Count == 0) throw new ApiException(400, "VALIDATION_ERROR", "Đơn phải có ít nhất một loại vé.");
        if (duLieuTaoDon.ChiTietDatVe.GroupBy(mucDatVe => mucDatVe.LoaiVeId).Any(nhomLoaiVe => nhomLoaiVe.Count() > 1)) throw new ApiException(400, "VALIDATION_ERROR", "Không được lặp loại vé trong đơn.");
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var danhSachLoaiVeId = duLieuTaoDon.ChiTietDatVe.Select(mucDatVe => mucDatVe.LoaiVeId).OrderBy(loaiVeId => loaiVeId).ToList();
        var danhSachLoaiVe = await nguCanhDuLieu.DanhSachLoaiVe.Include(loaiVeDb => loaiVeDb.SuKien).Where(loaiVeDb => danhSachLoaiVeId.Contains(loaiVeDb.LoaiVeId)).OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).ToListAsync();
        if (danhSachLoaiVe.Count != danhSachLoaiVeId.Count) throw new ApiException(404, "NOT_FOUND", "Có loại vé không tồn tại.");
        var suKienId = danhSachLoaiVe[0].SuKienId;
        if (danhSachLoaiVe.Any(loaiVeDb => loaiVeDb.SuKienId != suKienId)) throw new ApiException(400, "VALIDATION_ERROR", "Các loại vé phải thuộc cùng sự kiện.");
        if (danhSachLoaiVe[0].SuKien.TrangThai != TrangThaiSuKien.DaDuyet) throw new ApiException(409, "EVENT_NOT_APPROVED", "Sự kiện chưa được duyệt.");
        var thoiDiemHienTai = DateTime.UtcNow;
        decimal tongTien = 0; int tongSoVe = 0;
        foreach (var loaiVe in danhSachLoaiVe)
        {
            var chiTietDatVe = duLieuTaoDon.ChiTietDatVe.Single(mucDatVe => mucDatVe.LoaiVeId == loaiVe.LoaiVeId);
            if (chiTietDatVe.SoLuong <= 0 || chiTietDatVe.SoLuong > 20) throw new ApiException(400, "TICKET_LIMIT_EXCEEDED", "Số lượng vé không hợp lệ.");
            if (thoiDiemHienTai < loaiVe.ThoiGianMoBan || thoiDiemHienTai > loaiVe.ThoiGianDongBan) throw new ApiException(409, "TICKET_NOT_ON_SALE", $"Loại vé {loaiVe.TenLoaiVe} chưa mở bán hoặc đã đóng bán.");
            if (loaiVe.SoLuongDaBan + loaiVe.SoLuongDangGiu + chiTietDatVe.SoLuong > loaiVe.SoLuong) throw new ApiException(409, "TICKET_SOLD_OUT", $"Loại vé {loaiVe.TenLoaiVe} không đủ số lượng.");
            QuanLyTonVe.GiuSoLuongVeChoDon(loaiVe, chiTietDatVe.SoLuong);
            tongSoVe += chiTietDatVe.SoLuong; tongTien += loaiVe.GiaVe * chiTietDatVe.SoLuong;
        }
        var soPhutGiuVe = Math.Clamp(cauHinhUngDung.GetValue("Order:ExpireMinutes", 15), 1, 120);
        var donVe = new DonVeEntity { TaiKhoanId = taiKhoanId, SuKienId = suKienId, SoLuong = tongSoVe, TongTien = tongTien, ThoiGianHetHan = thoiDiemHienTai.AddMinutes(soPhutGiuVe) };
        foreach (var loaiVe in danhSachLoaiVe)
        {
            var chiTietDatVe = duLieuTaoDon.ChiTietDatVe.Single(mucDatVe => mucDatVe.LoaiVeId == loaiVe.LoaiVeId);
            donVe.ChiTietDonVe.Add(new ChiTietDonVe { LoaiVeId = loaiVe.LoaiVeId, SoLuong = chiTietDatVe.SoLuong, DonGia = loaiVe.GiaVe, ThanhTien = loaiVe.GiaVe * chiTietDatVe.SoLuong });
        }
        nguCanhDuLieu.DanhSachDonVe.Add(donVe); await nguCanhDuLieu.SaveChangesAsync(); await giaoDich.CommitAsync(); return await LayChiTietDonVe(taiKhoanId, donVe.DonVeId, false);
    }

    public async Task<List<DonVeEntity>> LayDonVeCuaTaiKhoan(int taiKhoanId)
        => await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().Where(donVeDb => donVeDb.TaiKhoanId == taiKhoanId).Include(donVeDb => donVeDb.SuKien).Include(donVeDb => donVeDb.ChiTietDonVe).ThenInclude(chiTietDonVeDb => chiTietDonVeDb.LoaiVe).OrderByDescending(donVeDb => donVeDb.DonVeId).ToListAsync();

    public async Task<DonVeEntity> LayChiTietDonVe(int taiKhoanId, int donVeId, bool laNguoiQuanLy)
    {
        var donVe = await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().Include(donVeDb => donVeDb.SuKien).ThenInclude(suKienDb => suKienDb.BanToChuc).Include(donVeDb => donVeDb.ChiTietDonVe).ThenInclude(chiTietDonVeDb => chiTietDonVeDb.LoaiVe).Include(donVeDb => donVeDb.DanhSachThanhToan).Include(donVeDb => donVeDb.DanhSachVe).SingleOrDefaultAsync(donVeDb => donVeDb.DonVeId == donVeId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn vé.");
        var laChuSoHuu = donVe.TaiKhoanId == taiKhoanId;
        var laBanToChuc = donVe.SuKien.BanToChuc.TaiKhoanId == taiKhoanId;
        if (!laChuSoHuu && !laBanToChuc && !laNguoiQuanLy) throw new ApiException(403, "FORBIDDEN", "Không có quyền xem đơn.");
        return donVe;
    }

    public async Task HuyDonVe(int taiKhoanId, int donVeId)
    {
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var donVe = await nguCanhDuLieu.DanhSachDonVe.Include(donVeDb => donVeDb.ChiTietDonVe).SingleOrDefaultAsync(donVeDb => donVeDb.DonVeId == donVeId && donVeDb.TaiKhoanId == taiKhoanId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn vé.");
        if (donVe.TrangThai != TrangThaiDon.ChoThanhToan) throw new ApiException(409, "ORDER_INVALID_STATE", "Đơn không thể hủy.");
        await GiaiPhongVeDangGiuCuaDon(donVe);
        donVe.TrangThai = TrangThaiDon.DaHuy;
        await nguCanhDuLieu.SaveChangesAsync(); await giaoDich.CommitAsync();
    }

    public async Task<int> XuLyCacDonVeHetHan()
    {
        var thoiDiemHienTai = DateTime.UtcNow;
        var danhSachDonVeId = await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().Where(donVeDb => donVeDb.TrangThai == TrangThaiDon.ChoThanhToan && donVeDb.ThoiGianHetHan < thoiDiemHienTai).OrderBy(donVeDb => donVeDb.ThoiGianHetHan).ThenBy(donVeDb => donVeDb.DonVeId).Select(donVeDb => donVeDb.DonVeId).Take(100).ToListAsync();
        var soDonDaXuLy = 0;
        foreach (var donVeId in danhSachDonVeId)
        {
            await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var donVe = await nguCanhDuLieu.DanhSachDonVe.Include(donVeDb => donVeDb.ChiTietDonVe).SingleOrDefaultAsync(donVeDb => donVeDb.DonVeId == donVeId);
            if (donVe == null || donVe.TrangThai != TrangThaiDon.ChoThanhToan || donVe.ThoiGianHetHan >= DateTime.UtcNow) { await giaoDich.RollbackAsync(); continue; }
            await GiaiPhongVeDangGiuCuaDon(donVe); donVe.TrangThai = TrangThaiDon.HetHan; await nguCanhDuLieu.SaveChangesAsync(); await giaoDich.CommitAsync(); soDonDaXuLy++;
        }
        return soDonDaXuLy;
    }

    async Task GiaiPhongVeDangGiuCuaDon(DonVeEntity donVe)
    {
        var danhSachLoaiVeId = donVe.ChiTietDonVe.Select(chiTietDonVeDb => chiTietDonVeDb.LoaiVeId).OrderBy(loaiVeId => loaiVeId).ToList();
        var danhSachLoaiVe = await nguCanhDuLieu.DanhSachLoaiVe.Where(loaiVeDb => danhSachLoaiVeId.Contains(loaiVeDb.LoaiVeId)).OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).ToListAsync();
        foreach (var chiTietDonVe in donVe.ChiTietDonVe)
        {
            var loaiVe = danhSachLoaiVe.Single(loaiVeDb => loaiVeDb.LoaiVeId == chiTietDonVe.LoaiVeId);
            QuanLyTonVe.GiaiPhongVeDangGiu(loaiVe, chiTietDonVe.SoLuong);
        }
    }
}
