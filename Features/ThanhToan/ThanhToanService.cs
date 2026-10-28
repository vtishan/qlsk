using System.Data;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Infrastructure.PayOS;

using OrderEntity = QLSK.Domain.Entities.DonVe;
using ThanhToanEntity = QLSK.Domain.Entities.ThanhToan;
using VeEntity = QLSK.Domain.Entities.Ve;

namespace QLSK.Features.ThanhToan;

public class ThanhToanService(ApplicationDbContext db, PayOsService payos)
{
    public async Task<object> Tao(int userId, int donId)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var order = await db.DonVes
            .Include(x => x.ChiTiets)
            .Include(x => x.ThanhToans)
            .Include(x => x.Ves)
            .SingleOrDefaultAsync(x => x.Id == donId && x.TaiKhoanId == userId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn vé.");
        if (order.TrangThai != TrangThaiDon.ChoThanhToan)
            throw new ApiException(409, "ORDER_INVALID_STATE", "Đơn không ở trạng thái chờ thanh toán.");
        if (order.HetHan <= DateTime.UtcNow)
            throw new ApiException(409, "ORDER_EXPIRED", "Đơn đã hết hạn.");

        if (order.TongTien == 0)
        {
            await PhatVe(order);
            order.TrangThai = TrangThaiDon.ThanhCong;
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return new
            {
                paymentId = (int?)null,
                soTien = 0m,
                checkoutUrl = (string?)null,
                trangThai = TrangThaiThanhToan.ThanhCong
            };
        }

        foreach (var pending in order.ThanhToans.Where(x => x.TrangThai == TrangThaiThanhToan.DangXuLy))
            pending.TrangThai = TrangThaiThanhToan.ThatBai;
        await db.SaveChangesAsync();

        var payment = new ThanhToanEntity { DonVeId = order.Id, SoTien = order.TongTien };
        db.ThanhToans.Add(payment);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        try
        {
            var link = await payos.Tao(order.Id, order.TongTien, $"DON {order.Id}", order.HetHan);
            payment.MaLienKet = link.PaymentLinkId;
            await db.SaveChangesAsync();
            return new { payment.Id, payment.SoTien, checkoutUrl = link.CheckoutUrl, payment.TrangThai };
        }
        catch
        {
            payment.TrangThai = TrangThaiThanhToan.ThatBai;
            await db.SaveChangesAsync();
            throw;
        }
    }

    public async Task XuLyWebhook(PayOsWebhook webhook)
    {
        var data = payos.Verify(webhook);
        if (!webhook.Success || webhook.Code != "00" || data.Code != "00")
            throw new ApiException(400, "PAYMENT_INVALID", "Webhook không phải giao dịch thành công.");
        if (string.IsNullOrWhiteSpace(data.Reference))
            throw new ApiException(400, "PAYMENT_INVALID", "Webhook thiếu mã giao dịch.");

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (await db.ThanhToans.AnyAsync(x => x.MaGiaoDich == data.Reference))
        {
            await tx.CommitAsync();
            return;
        }

        var orderId = checked((int)data.OrderCode);
        var order = await db.DonVes
            .Include(x => x.ChiTiets)
            .Include(x => x.ThanhToans)
            .Include(x => x.Ves)
            .SingleOrDefaultAsync(x => x.Id == orderId)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy đơn từ webhook.");
        var payment = TimThanhToan(order, data.PaymentLinkId);
        if (payment == null)
        {
            payment = new ThanhToanEntity
            {
                DonVeId = order.Id,
                SoTien = order.TongTien,
                MaGiaoDich = data.Reference,
                MaLienKet = data.PaymentLinkId,
                TrangThai = TrangThaiThanhToan.NghiVan
            };
            db.ThanhToans.Add(payment);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return;
        }

        payment.MaGiaoDich = data.Reference;
        payment.MaLienKet = data.PaymentLinkId;
        if (data.Amount != order.TongTien)
        {
            payment.TrangThai = TrangThaiThanhToan.NghiVan;
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            throw new ApiException(409, "PAYMENT_INVALID", "Số tiền webhook không khớp đơn hàng.");
        }
        if (order.TrangThai != TrangThaiDon.ChoThanhToan || order.HetHan <= DateTime.UtcNow)
        {
            payment.TrangThai = TrangThaiThanhToan.NghiVan;
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return;
        }

        if (!await CoThePhatVe(order))
        {
            payment.TrangThai = TrangThaiThanhToan.NghiVan;
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return;
        }

        await PhatVe(order, false);
        foreach (var other in order.ThanhToans.Where(x => x.Id != payment.Id && x.TrangThai == TrangThaiThanhToan.DangXuLy))
            other.TrangThai = TrangThaiThanhToan.ThatBai;
        order.TrangThai = TrangThaiDon.ThanhCong;
        payment.TrangThai = TrangThaiThanhToan.ThanhCong;
        payment.NgayThanhToan = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task HoanTien(int id, decimal soTien, string maHoanTien)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var payment = await db.ThanhToans
            .Include(x => x.DonVe)
            .ThenInclude(x => x.Ves)
            .ThenInclude(x => x.LoaiVe)
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ApiException(404, "NOT_FOUND", "Không tìm thấy thanh toán.");
        if (payment.TrangThai != TrangThaiThanhToan.ThanhCong)
            throw new ApiException(409, "PAYMENT_INVALID", "Thanh toán chưa thành công.");
        if (soTien <= 0 || soTien != decimal.Truncate(soTien) || payment.SoTienHoan + soTien > payment.SoTien)
            throw new ApiException(409, "REFUND_EXCEEDS_PAYMENT", "Số tiền hoàn phải là số VND nguyên và không vượt số tiền đã thanh toán.");

        var hoanToanPhan = payment.SoTienHoan + soTien == payment.SoTien;
        if (hoanToanPhan && payment.DonVe.Ves.Any(x => x.TrangThai == TrangThaiVe.DaCheckIn))
            throw new ApiException(409, "REFUND_CHECKED_IN_TICKET", "Không thể hoàn toàn phần khi đơn có vé đã check-in.");

        payment.SoTienHoan += soTien;
        payment.MaHoanTien = maHoanTien.Trim();
        if (hoanToanPhan)
        {
            payment.DonVe.TrangThai = TrangThaiDon.DaHoanTien;
            var tickets = payment.DonVe.Ves.Where(x => x.TrangThai == TrangThaiVe.ChuaSuDung).ToList();
            foreach (var group in tickets.GroupBy(x => x.LoaiVe))
                group.Key.DaBan = Math.Max(0, group.Key.DaBan - group.Count());
            foreach (var ticket in tickets)
                ticket.TrangThai = TrangThaiVe.DaHoanTien;
        }
        else
        {
            payment.DonVe.TrangThai = TrangThaiDon.YeuCauHoanTien;
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private static ThanhToanEntity? TimThanhToan(OrderEntity order, string? paymentLinkId)
    {
        if (!string.IsNullOrWhiteSpace(paymentLinkId))
        {
            var exact = order.ThanhToans.SingleOrDefault(x => x.MaLienKet == paymentLinkId);
            if (exact != null) return exact;
        }

        var khongCoLienKet = order.ThanhToans
            .Where(x => x.TrangThai == TrangThaiThanhToan.DangXuLy && string.IsNullOrWhiteSpace(x.MaLienKet))
            .ToList();
        return khongCoLienKet.Count == 1 ? khongCoLienKet[0] : null;
    }

    private async Task<bool> CoThePhatVe(OrderEntity order)
    {
        var ids = order.ChiTiets.Select(x => x.LoaiVeId).Distinct().OrderBy(x => x).ToList();
        var types = await db.LoaiVes.Where(x => ids.Contains(x.Id)).OrderBy(x => x.Id).ToListAsync();
        if (types.Count != ids.Count) return false;
        return order.ChiTiets.All(detail =>
        {
            var type = types.Single(x => x.Id == detail.LoaiVeId);
            return type.DangGiu >= detail.SoLuong && type.DaBan + detail.SoLuong <= type.SoLuong;
        });
    }

    private async Task PhatVe(OrderEntity order, bool validate = true)
    {
        var ids = order.ChiTiets.Select(x => x.LoaiVeId).Distinct().OrderBy(x => x).ToList();
        var types = await db.LoaiVes.Where(x => ids.Contains(x.Id)).OrderBy(x => x.Id).ToListAsync();
        if (validate && (types.Count != ids.Count || order.ChiTiets.Any(detail =>
            {
                var type = types.SingleOrDefault(x => x.Id == detail.LoaiVeId);
                return type == null || type.DangGiu < detail.SoLuong || type.DaBan + detail.SoLuong > type.SoLuong;
            })))
            throw new ApiException(409, "TICKET_INVENTORY_INVALID", "Tồn vé không còn hợp lệ.");

        foreach (var detail in order.ChiTiets)
        {
            var type = types.Single(x => x.Id == detail.LoaiVeId);
            type.DangGiu -= detail.SoLuong;
            type.DaBan += detail.SoLuong;
            for (var i = 0; i < detail.SoLuong; i++)
            {
                var code = Guid.NewGuid().ToString("N").ToUpperInvariant();
                db.Ves.Add(new VeEntity
                {
                    DonVeId = order.Id,
                    LoaiVeId = detail.LoaiVeId,
                    MaVe = code[..12],
                    MaQR = code
                });
            }
        }
    }
}
