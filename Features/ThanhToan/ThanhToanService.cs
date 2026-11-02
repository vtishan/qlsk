using System.Data;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Common.Inventory;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Infrastructure.PayOS;

using OrderEntity = QLSK.Domain.Entities.DonVe;
using ThanhToanEntity = QLSK.Domain.Entities.ThanhToan;
using VeEntity = QLSK.Domain.Entities.Ve;

namespace QLSK.Features.ThanhToan;

public class ThanhToanService(ApplicationDbContext nguCanhDuLieu, PayOsService dichVuThanhToanPayOs)
{
    public async Task<object> BatDauThanhToanDonVe(int taiKhoanId, int donVeId)
    {
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var donVe = await nguCanhDuLieu.DanhSachDonVe
            .Include(donVeDb => donVeDb.ChiTietDonVe)
            .Include(donVeDb => donVeDb.DanhSachThanhToan)
            .Include(donVeDb => donVeDb.DanhSachVe)
            .SingleOrDefaultAsync(donVeDb => donVeDb.DonVeId == donVeId && donVeDb.TaiKhoanId == taiKhoanId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn vé.");
        if (donVe.TrangThai != TrangThaiDon.ChoThanhToan)
            throw new ApiException(409, "ORDER_INVALID_STATE", "Đơn không ở trạng thái chờ thanh toán.");
        if (donVe.ThoiGianHetHan <= DateTime.UtcNow)
            throw new ApiException(409, "ORDER_EXPIRED", "Đơn đã hết hạn.");

        if (donVe.TongTien == 0)
        {
            var ketQuaThanhToanMienPhi = await HoanTatDonVeMienPhi(donVe);
            await giaoDich.CommitAsync();
            return ketQuaThanhToanMienPhi;
        }

        var thanhToan = await TaoGiaoDichThanhToanTraPhi(donVe);
        await giaoDich.CommitAsync();
        return await TaoLienKetChoGiaoDich(donVe, thanhToan);
    }

    private async Task<object> HoanTatDonVeMienPhi(OrderEntity donVe)
    {
        await PhatVeChoDon(donVe);
        donVe.TrangThai = TrangThaiDon.ThanhCong;
        await nguCanhDuLieu.SaveChangesAsync();
        return new
        {
            paymentId = (int?)null,
            soTien = 0m,
            checkoutUrl = (string?)null,
            trangThai = TrangThaiThanhToan.ThanhCong
        };
    }

    private async Task<ThanhToanEntity> TaoGiaoDichThanhToanTraPhi(OrderEntity donVe)
    {
        foreach (var thanhToanDangXuLy in donVe.DanhSachThanhToan.Where(thanhToanDb => thanhToanDb.TrangThai == TrangThaiThanhToan.DangXuLy))
            thanhToanDangXuLy.TrangThai = TrangThaiThanhToan.ThatBai;
        await nguCanhDuLieu.SaveChangesAsync();

        var thanhToan = new ThanhToanEntity { DonVeId = donVe.DonVeId, SoTien = donVe.TongTien };
        nguCanhDuLieu.DanhSachThanhToan.Add(thanhToan);
        await nguCanhDuLieu.SaveChangesAsync();
        return thanhToan;
    }

    private async Task<object> TaoLienKetChoGiaoDich(OrderEntity donVe, ThanhToanEntity thanhToan)
    {
        try
        {
            var lienKetThanhToan = await dichVuThanhToanPayOs.TaoLienKetThanhToan(donVe.DonVeId, donVe.TongTien, $"DON {donVe.DonVeId}", donVe.ThoiGianHetHan);
            thanhToan.MaLienKet = lienKetThanhToan.MaLienKetThanhToan;
            await nguCanhDuLieu.SaveChangesAsync();
            return new { Id = thanhToan.ThanhToanId, thanhToan.SoTien, checkoutUrl = lienKetThanhToan.UrlThanhToan, thanhToan.TrangThai };
        }
        catch
        {
            thanhToan.TrangThai = TrangThaiThanhToan.ThatBai;
            await nguCanhDuLieu.SaveChangesAsync();
            throw;
        }
    }

    public async Task XuLyWebhookThanhToan(PayOsWebhook webhook)
    {
        var duLieuWebhook = dichVuThanhToanPayOs.XacThucVaDocWebhook(webhook);
        if (!webhook.ThanhCong || webhook.MaKetQua != "00" || duLieuWebhook.MaKetQua != "00")
            throw new ApiException(400, "PAYMENT_INVALID", "Webhook không phải giao dịch thành công.");
        if (string.IsNullOrWhiteSpace(duLieuWebhook.MaThamChieu))
            throw new ApiException(400, "PAYMENT_INVALID", "Webhook thiếu mã giao dịch.");

        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (await nguCanhDuLieu.DanhSachThanhToan.AnyAsync(thanhToanDb => thanhToanDb.MaGiaoDich == duLieuWebhook.MaThamChieu))
        {
            await giaoDich.CommitAsync();
            return;
        }

        var donVeId = checked((int)duLieuWebhook.MaDonVe);
        var donVe = await nguCanhDuLieu.DanhSachDonVe
            .Include(donVeDb => donVeDb.ChiTietDonVe)
            .Include(donVeDb => donVeDb.DanhSachThanhToan)
            .Include(donVeDb => donVeDb.DanhSachVe)
            .SingleOrDefaultAsync(donVeDb => donVeDb.DonVeId == donVeId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn từ webhook.");
        var thanhToan = TimThanhToanChoWebhook(donVe, duLieuWebhook.MaLienKetThanhToan);
        if (thanhToan == null)
        {
            thanhToan = new ThanhToanEntity
            {
                DonVeId = donVe.DonVeId,
                SoTien = donVe.TongTien,
                MaGiaoDich = duLieuWebhook.MaThamChieu,
                MaLienKet = duLieuWebhook.MaLienKetThanhToan,
                TrangThai = TrangThaiThanhToan.NghiVan
            };
            nguCanhDuLieu.DanhSachThanhToan.Add(thanhToan);
            await nguCanhDuLieu.SaveChangesAsync();
            await giaoDich.CommitAsync();
            return;
        }

        thanhToan.MaGiaoDich = duLieuWebhook.MaThamChieu;
        thanhToan.MaLienKet = duLieuWebhook.MaLienKetThanhToan;
        if (duLieuWebhook.SoTienGiaoDich != donVe.TongTien)
        {
            thanhToan.TrangThai = TrangThaiThanhToan.NghiVan;
            await nguCanhDuLieu.SaveChangesAsync();
            await giaoDich.CommitAsync();
            throw new ApiException(409, "PAYMENT_INVALID", "Số tiền webhook không khớp đơn hàng.");
        }
        if (donVe.TrangThai != TrangThaiDon.ChoThanhToan || donVe.ThoiGianHetHan <= DateTime.UtcNow)
        {
            thanhToan.TrangThai = TrangThaiThanhToan.NghiVan;
            await nguCanhDuLieu.SaveChangesAsync();
            await giaoDich.CommitAsync();
            return;
        }

        if (!await CoThePhatVe(donVe))
        {
            thanhToan.TrangThai = TrangThaiThanhToan.NghiVan;
            await nguCanhDuLieu.SaveChangesAsync();
            await giaoDich.CommitAsync();
            return;
        }

        await PhatVeChoDon(donVe, kiemTraTonVe: false);
        foreach (var thanhToanKhac in donVe.DanhSachThanhToan.Where(thanhToanDb => thanhToanDb.ThanhToanId != thanhToan.ThanhToanId && thanhToanDb.TrangThai == TrangThaiThanhToan.DangXuLy))
            thanhToanKhac.TrangThai = TrangThaiThanhToan.ThatBai;
        donVe.TrangThai = TrangThaiDon.ThanhCong;
        thanhToan.TrangThai = TrangThaiThanhToan.ThanhCong;
        thanhToan.NgayThanhToan = DateTime.UtcNow;
        await nguCanhDuLieu.SaveChangesAsync();
        await giaoDich.CommitAsync();
    }

    public async Task GhiNhanHoanTien(int thanhToanId, decimal soTienHoan, string maHoanTien)
    {
        await using var giaoDich = await nguCanhDuLieu.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var thanhToan = await nguCanhDuLieu.DanhSachThanhToan
            .Include(thanhToanDb => thanhToanDb.DonVe)
            .ThenInclude(donVeDb => donVeDb.DanhSachVe)
            .ThenInclude(veDaPhatHanhDb => veDaPhatHanhDb.LoaiVe)
            .SingleOrDefaultAsync(thanhToanDb => thanhToanDb.ThanhToanId == thanhToanId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy thanh toán.");
        if (thanhToan.TrangThai != TrangThaiThanhToan.ThanhCong)
            throw new ApiException(409, "PAYMENT_INVALID", "Thanh toán chưa thành công.");
        if (soTienHoan <= 0 || soTienHoan != decimal.Truncate(soTienHoan) || thanhToan.SoTienHoan + soTienHoan > thanhToan.SoTien)
            throw new ApiException(409, "REFUND_EXCEEDS_PAYMENT", "Số tiền hoàn phải là số VND nguyên và không vượt số tiền đã thanh toán.");

        var hoanToanPhan = thanhToan.SoTienHoan + soTienHoan == thanhToan.SoTien;
        if (hoanToanPhan && thanhToan.DonVe.DanhSachVe.Any(veDaPhatHanhDb => veDaPhatHanhDb.TrangThai == TrangThaiVe.DaCheckIn))
            throw new ApiException(409, "REFUND_CHECKED_IN_TICKET", "Không thể hoàn toàn phần khi đơn có vé đã check-in.");

        thanhToan.SoTienHoan += soTienHoan;
        thanhToan.MaHoanTien = maHoanTien.Trim();
        if (hoanToanPhan)
        {
            thanhToan.DonVe.TrangThai = TrangThaiDon.DaHoanTien;
            var danhSachVeHoanTien = thanhToan.DonVe.DanhSachVe.Where(veDaPhatHanhDb => veDaPhatHanhDb.TrangThai == TrangThaiVe.ChuaSuDung).ToList();
            foreach (var nhomTheoLoaiVe in danhSachVeHoanTien.GroupBy(veDaPhatHanhDb => veDaPhatHanhDb.LoaiVe))
                QuanLyTonVe.TraLaiSoLuongVeDaBan(nhomTheoLoaiVe.Key, nhomTheoLoaiVe.Count());
            foreach (var veDaPhatHanh in danhSachVeHoanTien)
                veDaPhatHanh.TrangThai = TrangThaiVe.DaHoanTien;
        }
        else
        {
            thanhToan.DonVe.TrangThai = TrangThaiDon.YeuCauHoanTien;
        }

        await nguCanhDuLieu.SaveChangesAsync();
        await giaoDich.CommitAsync();
    }

    private static ThanhToanEntity? TimThanhToanChoWebhook(OrderEntity donVe, string? maLienKetThanhToan)
    {
        if (!string.IsNullOrWhiteSpace(maLienKetThanhToan))
        {
            var thanhToanTheoLienKet = donVe.DanhSachThanhToan.SingleOrDefault(thanhToanDb => thanhToanDb.MaLienKet == maLienKetThanhToan);
            if (thanhToanTheoLienKet != null) return thanhToanTheoLienKet;
        }

        var thanhToanChuaCoLienKet = donVe.DanhSachThanhToan
            .Where(thanhToanDb => thanhToanDb.TrangThai == TrangThaiThanhToan.DangXuLy && string.IsNullOrWhiteSpace(thanhToanDb.MaLienKet))
            .ToList();
        return thanhToanChuaCoLienKet.Count == 1 ? thanhToanChuaCoLienKet[0] : null;
    }

    private async Task<bool> CoThePhatVe(OrderEntity donVe)
    {
        var danhSachLoaiVeId = donVe.ChiTietDonVe.Select(chiTietDonVeDb => chiTietDonVeDb.LoaiVeId).Distinct().OrderBy(loaiVeId => loaiVeId).ToList();
        var danhSachLoaiVe = await nguCanhDuLieu.DanhSachLoaiVe.Where(loaiVeDb => danhSachLoaiVeId.Contains(loaiVeDb.LoaiVeId)).OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).ToListAsync();
        if (danhSachLoaiVe.Count != danhSachLoaiVeId.Count) return false;
        return donVe.ChiTietDonVe.All(chiTietDonVeDb =>
        {
            var loaiVe = danhSachLoaiVe.Single(loaiVeDb => loaiVeDb.LoaiVeId == chiTietDonVeDb.LoaiVeId);
            return loaiVe.SoLuongDangGiu >= chiTietDonVeDb.SoLuong && loaiVe.SoLuongDaBan + chiTietDonVeDb.SoLuong <= loaiVe.SoLuong;
        });
    }

    private async Task PhatVeChoDon(OrderEntity donVe, bool kiemTraTonVe = true)
    {
        var danhSachLoaiVeId = donVe.ChiTietDonVe.Select(chiTietDonVeDb => chiTietDonVeDb.LoaiVeId).Distinct().OrderBy(loaiVeId => loaiVeId).ToList();
        var danhSachLoaiVe = await nguCanhDuLieu.DanhSachLoaiVe.Where(loaiVeDb => danhSachLoaiVeId.Contains(loaiVeDb.LoaiVeId)).OrderBy(loaiVeDb => loaiVeDb.LoaiVeId).ToListAsync();
        if (kiemTraTonVe && (danhSachLoaiVe.Count != danhSachLoaiVeId.Count || donVe.ChiTietDonVe.Any(chiTietDonVeDb =>
            {
                var loaiVe = danhSachLoaiVe.SingleOrDefault(loaiVeDb => loaiVeDb.LoaiVeId == chiTietDonVeDb.LoaiVeId);
                return loaiVe == null || loaiVe.SoLuongDangGiu < chiTietDonVeDb.SoLuong || loaiVe.SoLuongDaBan + chiTietDonVeDb.SoLuong > loaiVe.SoLuong;
            })))
            throw new ApiException(409, "TICKET_INVENTORY_INVALID", "Tồn vé không còn hợp lệ.");

        foreach (var chiTietDonVe in donVe.ChiTietDonVe)
        {
            var loaiVe = danhSachLoaiVe.Single(loaiVeDb => loaiVeDb.LoaiVeId == chiTietDonVe.LoaiVeId);
            QuanLyTonVe.ChuyenVeGiuSangDaBan(loaiVe, chiTietDonVe.SoLuong);
            for (var chiSoVe = 0; chiSoVe < chiTietDonVe.SoLuong; chiSoVe++)
            {
                var maQrDayDu = Guid.NewGuid().ToString("N").ToUpperInvariant();
                nguCanhDuLieu.DanhSachVe.Add(new VeEntity
                {
                    DonVeId = donVe.DonVeId,
                    LoaiVeId = chiTietDonVe.LoaiVeId,
                    MaVe = maQrDayDu[..12],
                    NoiDungMaQr = maQrDayDu
                });
            }
        }
    }
}
