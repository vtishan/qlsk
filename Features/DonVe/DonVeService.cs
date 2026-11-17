using QLSK.Common.Models;
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
    public async Task<DonVeEntity> TaoDonVaGiuVe(int taiKhoanId, TaoDonDto duLieuTaoDon, string? idempotencyKey = null)
    {
        if (duLieuTaoDon.ChiTietDatVe is null || duLieuTaoDon.ChiTietDatVe.Count == 0 || duLieuTaoDon.ChiTietDatVe.Count > 20) throw new ApiException(400, "VALIDATION_ERROR", "Đơn phải có ít nhất một loại vé.");
        if (duLieuTaoDon.ChiTietDatVe.GroupBy(mucDatVe => mucDatVe.LoaiVeId).Any(nhomLoaiVe => nhomLoaiVe.Count() > 1)) throw new ApiException(400, "VALIDATION_ERROR", "Không được lặp loại vé trong đơn.");
        if (duLieuTaoDon.ChiTietDatVe.Any(i => i.SoLuong < 1 || i.SoLuong > 20) || duLieuTaoDon.ChiTietDatVe.Sum(i => (long)i.SoLuong) > 20)
            throw new ApiException(400, "TICKET_LIMIT_EXCEEDED", "Mỗi đơn được đặt tối đa 20 vé.");
        if (idempotencyKey is not null && !System.Text.RegularExpressions.Regex.IsMatch(idempotencyKey, @"\A[A-Za-z0-9_-]{16,64}\z"))
            throw new ApiException(400, "VALIDATION_ERROR", "Idempotency-Key không hợp lệ.");
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, nguCanhDuLieu.RequestAborted);
        await TransactionLock.Acquire(nguCanhDuLieu, $"order-user:{taiKhoanId}");
        if (idempotencyKey is not null)
        {
            var existing = await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().Include(o => o.ChiTietDonVe)
                .SingleOrDefaultAsync(o => o.TaiKhoanId == taiKhoanId && o.IdempotencyKey == idempotencyKey, nguCanhDuLieu.RequestAborted);
            if (existing is not null)
            {
                if (existing.ChiTietDonVe.Count != duLieuTaoDon.ChiTietDatVe.Count || existing.ChiTietDonVe.Any(item =>
                    !duLieuTaoDon.ChiTietDatVe.Any(i => i.LoaiVeId == item.LoaiVeId && i.SoLuong == item.SoLuong)))
                    throw new ApiException(409, "IDEMPOTENCY_CONFLICT", "Mã yêu cầu đã được dùng cho nội dung khác.");
                await giaoDich.CommitAsync(nguCanhDuLieu.RequestAborted);
                return await LayChiTietDonVe(taiKhoanId, existing.DonVeId, false);
            }
        }
        var pendingLimit = Math.Clamp(cauHinhUngDung.GetValue("Order:MaxPendingOrders", 5), 1, 50);
        if (await nguCanhDuLieu.DanhSachDonVe.CountAsync(o => o.TaiKhoanId == taiKhoanId && o.TrangThai == TrangThaiDon.ChoThanhToan, nguCanhDuLieu.RequestAborted) >= pendingLimit)
            throw new ApiException(429, "PENDING_ORDER_LIMIT", "Bạn có nhiều đơn chờ thanh toán. Vui lòng thanh toán hoặc hủy đơn cũ.");
        var eventId = await nguCanhDuLieu.DanhSachLoaiVe.AsNoTracking().Where(t => t.LoaiVeId == duLieuTaoDon.ChiTietDatVe[0].LoaiVeId)
            .Select(t => (int?)t.SuKienId).SingleOrDefaultAsync(nguCanhDuLieu.RequestAborted) ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy loại vé.");
        await TransactionLock.Acquire(nguCanhDuLieu, $"inventory-event:{eventId}");
        var danhSachLoaiVeId = duLieuTaoDon.ChiTietDatVe.Select(mucDatVe => mucDatVe.LoaiVeId).OrderBy(loaiVeId => loaiVeId).ToList();
        var danhSachLoaiVe = await nguCanhDuLieu.DanhSachLoaiVe.Include(loaiVeDb => loaiVeDb.SuKien).Where(loaiVeDb => danhSachLoaiVeId.Contains(loaiVeDb.LoaiVeId)).OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).ToListAsync(nguCanhDuLieu.RequestAborted);
        if (danhSachLoaiVe.Count != danhSachLoaiVeId.Count) throw new ApiException(404, "NOT_FOUND", "Có loại vé không tồn tại.");
        var suKienId = danhSachLoaiVe[0].SuKienId;
        if (danhSachLoaiVe.Any(loaiVeDb => loaiVeDb.SuKienId != suKienId)) throw new ApiException(400, "VALIDATION_ERROR", "Các loại vé phải thuộc cùng sự kiện.");
        if (danhSachLoaiVe[0].SuKien.TrangThai != TrangThaiSuKien.DaDuyet) throw new ApiException(409, "EVENT_NOT_APPROVED", "Sự kiện chưa được duyệt.");
        var thoiDiemHienTai = DateTime.UtcNow;
        if (danhSachLoaiVe[0].SuKien.ThoiGianBatDau <= thoiDiemHienTai)
            throw new ApiException(409, "TICKET_NOT_ON_SALE", "Sự kiện đã bắt đầu.");
        var occupied = await nguCanhDuLieu.DanhSachLoaiVe.Where(t => t.SuKienId == suKienId)
            .SumAsync(t => (long)t.SoLuongDaBan + t.SoLuongDangGiu, nguCanhDuLieu.RequestAborted);
        if (occupied + duLieuTaoDon.ChiTietDatVe.Sum(i => i.SoLuong) > danhSachLoaiVe[0].SuKien.SoNguoi)
            throw new ApiException(409, "TICKET_SOLD_OUT", "Sự kiện không còn đủ sức chứa.");
        decimal tongTien = 0; int tongSoVe = 0;
        foreach (var loaiVe in danhSachLoaiVe)
        {
            var chiTietDatVe = duLieuTaoDon.ChiTietDatVe.Single(mucDatVe => mucDatVe.LoaiVeId == loaiVe.LoaiVeId);
            if (chiTietDatVe.SoLuong <= 0 || chiTietDatVe.SoLuong > 20) throw new ApiException(400, "TICKET_LIMIT_EXCEEDED", "Số lượng vé không hợp lệ.");
            if (thoiDiemHienTai < loaiVe.ThoiGianMoBan || thoiDiemHienTai > loaiVe.ThoiGianDongBan) throw new ApiException(409, "TICKET_NOT_ON_SALE", $"Loại vé {loaiVe.TenLoaiVe} chưa mở bán hoặc đã đóng bán.");
            if ((long)loaiVe.SoLuongDaBan + loaiVe.SoLuongDangGiu + chiTietDatVe.SoLuong > loaiVe.SoLuong) throw new ApiException(409, "TICKET_SOLD_OUT", $"Loại vé {loaiVe.TenLoaiVe} không đủ số lượng.");
            QuanLyTonVe.GiuSoLuongVeChoDon(loaiVe, chiTietDatVe.SoLuong);
            tongSoVe += chiTietDatVe.SoLuong; tongTien += loaiVe.GiaVe * chiTietDatVe.SoLuong;
        }
        var soPhutGiuVe = Math.Clamp(cauHinhUngDung.GetValue("Order:ExpireMinutes", 15), 1, 120);
        var donVe = new DonVeEntity { TaiKhoanId = taiKhoanId, IdempotencyKey = idempotencyKey, SuKienId = suKienId, SoLuong = tongSoVe, TongTien = tongTien, ThoiGianHetHan = thoiDiemHienTai.AddMinutes(soPhutGiuVe) };
        foreach (var loaiVe in danhSachLoaiVe)
        {
            var chiTietDatVe = duLieuTaoDon.ChiTietDatVe.Single(mucDatVe => mucDatVe.LoaiVeId == loaiVe.LoaiVeId);
            donVe.ChiTietDonVe.Add(new ChiTietDonVe { LoaiVeId = loaiVe.LoaiVeId, SoLuong = chiTietDatVe.SoLuong, DonGia = loaiVe.GiaVe, ThanhTien = loaiVe.GiaVe * chiTietDatVe.SoLuong });
        }
        nguCanhDuLieu.DanhSachDonVe.Add(donVe); await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted); await giaoDich.CommitAsync(nguCanhDuLieu.RequestAborted); return await LayChiTietDonVe(taiKhoanId, donVe.DonVeId, false);
    }

    public async Task<List<DonVeEntity>> LayDonVeCuaTaiKhoan(int taiKhoanId)
        => await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().Where(donVeDb => donVeDb.TaiKhoanId == taiKhoanId).Include(donVeDb => donVeDb.SuKien).Include(donVeDb => donVeDb.ChiTietDonVe).ThenInclude(chiTietDonVeDb => chiTietDonVeDb.LoaiVe).OrderByDescending(donVeDb => donVeDb.DonVeId).Paginate(nguCanhDuLieu).ToListAsync(nguCanhDuLieu.RequestAborted);

    public async Task<DonVeEntity> LayChiTietDonVe(int taiKhoanId, int donVeId, bool laNguoiQuanLy)
    {
        var donVe = await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().Include(donVeDb => donVeDb.SuKien).ThenInclude(suKienDb => suKienDb.BanToChuc).Include(donVeDb => donVeDb.ChiTietDonVe).ThenInclude(chiTietDonVeDb => chiTietDonVeDb.LoaiVe).Include(donVeDb => donVeDb.DanhSachThanhToan).Include(donVeDb => donVeDb.DanhSachVe).SingleOrDefaultAsync(donVeDb => donVeDb.DonVeId == donVeId, nguCanhDuLieu.RequestAborted)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn vé.");
        var laChuSoHuu = donVe.TaiKhoanId == taiKhoanId;
        var laBanToChuc = donVe.SuKien.BanToChuc.TaiKhoanId == taiKhoanId;
        if (!laChuSoHuu && !laBanToChuc && !laNguoiQuanLy) throw new ApiException(403, "FORBIDDEN", "Không có quyền xem đơn.");
        return donVe;
    }

    public async Task HuyDonVe(int taiKhoanId, int donVeId)
    {
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, nguCanhDuLieu.RequestAborted);
        await TransactionLock.Acquire(nguCanhDuLieu, $"order:{donVeId}");
        var donVe = await nguCanhDuLieu.DanhSachDonVe.Include(donVeDb => donVeDb.ChiTietDonVe).SingleOrDefaultAsync(donVeDb => donVeDb.DonVeId == donVeId && donVeDb.TaiKhoanId == taiKhoanId, nguCanhDuLieu.RequestAborted)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn vé.");
        if (donVe.TrangThai != TrangThaiDon.ChoThanhToan) throw new ApiException(409, "ORDER_INVALID_STATE", "Đơn không thể hủy.");
        await GiaiPhongVeDangGiuCuaDon(donVe);
        donVe.TrangThai = TrangThaiDon.DaHuy;
        await nguCanhDuLieu.SaveChangesAsync(nguCanhDuLieu.RequestAborted); await giaoDich.CommitAsync(nguCanhDuLieu.RequestAborted);
    }

    public async Task<int> XuLyCacDonVeHetHan(CancellationToken cancellationToken = default)
    {
        var thoiDiemHienTai = DateTime.UtcNow;
        var danhSachDonVeId = await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().Where(donVeDb => donVeDb.TrangThai == TrangThaiDon.ChoThanhToan && donVeDb.ThoiGianHetHan < thoiDiemHienTai).OrderBy(donVeDb => donVeDb.ThoiGianHetHan).ThenBy(donVeDb => donVeDb.DonVeId).Select(donVeDb => donVeDb.DonVeId).Take(100).ToListAsync(cancellationToken);
        var soDonDaXuLy = 0;
        foreach (var donVeId in danhSachDonVeId)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await TransactionLock.Acquire(nguCanhDuLieu, $"order:{donVeId}", cancellationToken);
            nguCanhDuLieu.ChangeTracker.Clear();
            var donVe = await nguCanhDuLieu.DanhSachDonVe.Include(donVeDb => donVeDb.ChiTietDonVe).SingleOrDefaultAsync(donVeDb => donVeDb.DonVeId == donVeId, cancellationToken);
            if (donVe == null || donVe.TrangThai != TrangThaiDon.ChoThanhToan || donVe.ThoiGianHetHan >= DateTime.UtcNow) { await giaoDich.RollbackAsync(cancellationToken); continue; }
            await GiaiPhongVeDangGiuCuaDon(donVe, cancellationToken); donVe.TrangThai = TrangThaiDon.HetHan; await nguCanhDuLieu.SaveChangesAsync(cancellationToken); await giaoDich.CommitAsync(cancellationToken); soDonDaXuLy++;
        }
        return soDonDaXuLy;
    }

    async Task GiaiPhongVeDangGiuCuaDon(DonVeEntity donVe, CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.CanBeCanceled) cancellationToken = nguCanhDuLieu.RequestAborted;
        await TransactionLock.Acquire(nguCanhDuLieu, $"inventory-event:{donVe.SuKienId}", cancellationToken);
        var danhSachLoaiVeId = donVe.ChiTietDonVe.Select(chiTietDonVeDb => chiTietDonVeDb.LoaiVeId).OrderBy(loaiVeId => loaiVeId).ToList();
        var danhSachLoaiVe = await nguCanhDuLieu.DanhSachLoaiVe.Where(loaiVeDb => danhSachLoaiVeId.Contains(loaiVeDb.LoaiVeId)).OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).ToListAsync(cancellationToken);
        foreach (var chiTietDonVe in donVe.ChiTietDonVe)
        {
            var loaiVe = danhSachLoaiVe.Single(loaiVeDb => loaiVeDb.LoaiVeId == chiTietDonVe.LoaiVeId);
            QuanLyTonVe.GiaiPhongVeDangGiu(loaiVe, chiTietDonVe.SoLuong);
        }
    }
}
