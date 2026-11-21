using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Requests;
using QLSK.Features.Catalog;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Tickets;

public sealed partial class OrderService
{
    private const long PaymentCodeBase = 2_000_000_000L;

    private async Task Expire(int session, DateTime now, CancellationToken ct)
    {
        // Caller holds the session lock. The filtered unique seat index must also release expired rows.
        await db.GiuGhe.Where(h => h.SuatDienId == session && h.TrangThai == TrangThaiGiuGhe.DangGiu && h.HetHan <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.TrangThai, TrangThaiGiuGhe.HetHan), ct);
        await db.SuDungVoucher.Where(v => v.TrangThai == TrangThaiSuDungVoucher.DangGiu && v.HetHanGiuLuot <= now &&
            db.DonHang.Any(o => o.Id == v.DonHangId && o.SuatDienId == session))
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.TrangThai, TrangThaiSuDungVoucher.DaNha), ct);
        await db.DonHang.Where(o => o.SuatDienId == session && o.TrangThai == TrangThaiDonHang.ChoThanhToan && o.HanThanhToan <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.TrangThai, TrangThaiDonHang.HetHan), ct);
    }

    public async Task<RentalCheckoutDto> Checkout(TaiKhoan actor, int id, CancellationToken ct)
    {
        if (!payOs.Configured) throw new ApiException(503, "Chưa cấu hình PayOS.");
        GiaoDichThanhToan payment;
        DateTime deadline;
        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            var session = await db.DonHang.Where(o => o.Id == id && o.TaiKhoanId == actor.Id).Select(o => (int?)o.SuatDienId).SingleOrDefaultAsync(ct) ?? throw Missing();
            await SqlOperations.Lock(db, $"qlsk:session:{session}", ct);
            await Expire(session, DateTime.UtcNow, ct);
            var order = await db.DonHang.SingleAsync(o => o.Id == id, ct);
            if (order.TrangThai != TrangThaiDonHang.ChoThanhToan || order.HanThanhToan <= DateTime.UtcNow || order.TongTien <= 0)
                throw Conflict("Đơn không còn hiệu lực để thanh toán.");
            deadline = order.HanThanhToan;
            payment = await db.GiaoDichThanhToan.SingleOrDefaultAsync(p => p.DonHangId == id && p.PhuongThuc == PhuongThucThanhToan.PayOS, ct)
                ?? new GiaoDichThanhToan { DonHangId = id, SoTienYeuCau = order.TongTien,
                    PhuongThuc = PhuongThucThanhToan.PayOS, TrangThai = TrangThaiGiaoDichThanhToan.ChoThanhToan };
            if (payment.Id == 0) { db.GiaoDichThanhToan.Add(payment); await db.SaveChangesAsync(ct); }
            payment.MaDonNhaCungCap = (PaymentCodeBase + payment.Id).ToString(CultureInfo.InvariantCulture);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        // Persist the deterministic intent before the network call. A timeout retry recovers the same link.
        var code = PaymentCodeBase + payment.Id;
        var url = await payOs.ExistingCheckout(code, payment.SoTienYeuCau, ct)
            ?? await payOs.Checkout(code, payment.SoTienYeuCau, deadline, ct, "VE");
        return new(payment.Id, url);
    }

    public async Task<OrderDto> Reconcile(TaiKhoan actor, int id, CancellationToken ct)
    {
        if (!await db.DonHang.AnyAsync(o => o.Id == id && o.TaiKhoanId == actor.Id, ct)) throw Missing();
        var payment = await db.GiaoDichThanhToan.AsNoTracking().SingleOrDefaultAsync(p => p.DonHangId == id && p.PhuongThuc == PhuongThucThanhToan.PayOS, ct)
            ?? throw Conflict("Đơn chưa mở thanh toán PayOS.");
        var status = await payOs.Status(PaymentCodeBase + payment.Id, ct);
        await ApplyPayment(payment.Id, status, null, ct);
        return await Detail(actor, id, ct);
    }

    public async Task PaymentWebhook(JsonElement payload, CancellationToken ct)
    {
        var verified = payOs.VerifyWebhook(payload);
        if (verified.Code <= PaymentCodeBase || verified.Code > PaymentCodeBase + int.MaxValue) throw Missing();
        var id = (int)(verified.Code - PaymentCodeBase);
        if (!await db.GiaoDichThanhToan.AnyAsync(p => p.Id == id && p.DonHangId != null && p.MaDonNhaCungCap == verified.Code.ToString(), ct)) throw Missing();
        var status = await payOs.Status(verified.Code, ct);
        await ApplyPayment(id, status, verified.EventId, ct);
    }

    private async Task ApplyPayment(int id, PayOsStatus status, string? eventId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var orderId = await db.GiaoDichThanhToan.Where(p => p.Id == id).Select(p => p.DonHangId).SingleOrDefaultAsync(ct) ?? throw Missing();
        var session = await db.DonHang.Where(o => o.Id == orderId).Select(o => new { o.SuatDienId, EventId = db.SuatDien.Where(s => s.Id == o.SuatDienId).Select(s => s.SuKienId).First() }).SingleAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{session.EventId}", ct);
        await SqlOperations.Lock(db, $"qlsk:session:{session.SuatDienId}", ct);
        await SqlOperations.Lock(db, $"qlsk:order-payment:{id}", ct);
        var payment = await db.GiaoDichThanhToan.SingleAsync(p => p.Id == id, ct);
        var order = await db.DonHang.SingleAsync(o => o.Id == orderId, ct);
        if (eventId != null && await db.WebhookThanhToan.AnyAsync(w => w.MaSuKienWebhook == eventId, ct)) return;
        if (payment.TrangThai is TrangThaiGiaoDichThanhToan.ThanhCong or TrangThaiGiaoDichThanhToan.ThuTrungCanHoan) return;
        var now = DateTime.UtcNow;
        await Expire(order.SuatDienId, now, ct);
        if (status.Status != "PAID")
        {
            if (status.Status is "CANCELLED" or "EXPIRED")
            {
                payment.TrangThai = TrangThaiGiaoDichThanhToan.HetHan;
                await Release(order.Id, ct);
                order.TrangThai = TrangThaiDonHang.HetHan;
            }
        }
        else
        {
            payment.SoTienThucThu = status.Paid;
            payment.NgayThanhToan = status.PaidAt;
            payment.MaGiaoDich = status.Reference;
            var exact = status.Amount == order.TongTien && status.Paid == order.TongTien && status.PaidAt.HasValue && !string.IsNullOrWhiteSpace(status.Reference);
            if (!exact)
            {
                payment.TrangThai = TrangThaiGiaoDichThanhToan.NghiVan;
                await Release(order.Id, ct);
                order.TrangThai = TrangThaiDonHang.DaThuTienHetCho;
            }
            else
            {
                var holds = await (from h in db.GiuGhe join line in db.ChiTietDonHang on h.ChiTietDonHangId equals line.Id
                                   where line.DonHangId == order.Id select new { Hold = h, Line = line }).ToListAsync(ct);
                var valid = order.TrangThai == TrangThaiDonHang.ChoThanhToan && now < order.HanThanhToan &&
                    status.PaidAt <= order.HanThanhToan && holds.Count > 0 && holds.All(h => h.Hold.TrangThai == TrangThaiGiuGhe.DangGiu && h.Hold.HetHan > now);
                var seatIds = holds.Where(h => h.Hold.GheId.HasValue).Select(h => h.Hold.GheId!.Value).ToArray();
                if (await Issued(order.SuatDienId).AnyAsync(v => v.GheId.HasValue && seatIds.Contains(v.GheId.Value), ct)) valid = false;
                var slot = await db.SuatDien.AsNoTracking().SingleAsync(s => s.Id == order.SuatDienId, ct);
                var evt = await catalog.Published().SingleOrDefaultAsync(e => e.Id == slot.SuKienId, ct);
                if (evt == null || !CatalogService.SaleOpen(slot, evt.TrangThai, now)) valid = false;
                payment.TrangThai = valid ? TrangThaiGiaoDichThanhToan.ThanhCong : TrangThaiGiaoDichThanhToan.ThuTrungCanHoan;
                order.TrangThai = valid ? TrangThaiDonHang.DaThanhToan : TrangThaiDonHang.DaThuTienHetCho;
                if (valid)
                {
                    foreach (var item in holds)
                    {
                        item.Hold.TrangThai = TrangThaiGiuGhe.DaChot;
                        db.Ve.Add(new Ve { SuatDienId = order.SuatDienId, LoaiVeId = item.Line.LoaiVeId, GheId = item.Hold.GheId,
                            ChiTietDonHangId = item.Line.Id, GiuGheId = item.Hold.Id,
                            MaVe = "QLSK-" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant(),
                            MaQR = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)), TrangThai = TrangThaiVe.DaPhatHanh, NgayPhatHanh = now });
                    }
                    await db.SuDungVoucher.Where(v => v.DonHangId == order.Id && v.TrangThai == TrangThaiSuDungVoucher.DangGiu)
                        .ExecuteUpdateAsync(s => s.SetProperty(v => v.TrangThai, TrangThaiSuDungVoucher.DaSuDung).SetProperty(v => v.DaChotLuc, now), ct);
                    SqlOperations.Notify(db, order.TaiKhoanId, "Vé đã phát hành", "Thanh toán đã xác minh cho Đơn #" + order.Id, "ticket");
                }
                else
                {
                    await Release(order.Id, ct);
                    var key = "late-order-payment:" + payment.Id;
                    if (!await db.YeuCauHoanTien.AnyAsync(r => r.KhoaChongLap == key, ct))
                        db.YeuCauHoanTien.Add(new YeuCauHoanTien { TaiKhoanId = order.TaiKhoanId, GiaoDichId = payment.Id,
                            KhoaChongLap = key, SoTienYeuCau = status.Paid, LoaiYeuCau = "Thu tiền sau giữ ghế",
                            LyDo = "Đã xác minh tiền thực thu; đơn hết hiệu lực hoặc ghế không còn. Chờ xét hoàn, chưa chuyển tiền.",
                            TrangThai = TrangThaiYeuCauHoanTien.ChoDuyet, NgayYeuCau = now });
                    SqlOperations.Notify(db, order.TaiKhoanId, "Thanh toán cần hoàn tiền", "Đơn #" + order.Id + " chưa phát hành vé. Đã tạo yêu cầu xét hoàn.", "refund");
                }
            }
        }
        if (eventId != null) db.WebhookThanhToan.Add(new WebhookThanhToan { GiaoDichId = payment.Id, MaSuKienWebhook = eventId,
            NoiDungDaLoc = JsonSerializer.Serialize(new { status.Status, status.Amount, status.Paid }), ThoiGianNhan = now,
            TrangThaiXuLy = payment.TrangThai == TrangThaiGiaoDichThanhToan.NghiVan ? TrangThaiXuLyWebhook.NghiVan : TrangThaiXuLyWebhook.DaXuLy });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task Release(int order, CancellationToken ct)
    {
        await db.GiuGhe.Where(h => h.TrangThai == TrangThaiGiuGhe.DangGiu && db.ChiTietDonHang.Any(l => l.Id == h.ChiTietDonHangId && l.DonHangId == order))
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.TrangThai, TrangThaiGiuGhe.DaNha), ct);
        await db.SuDungVoucher.Where(v => v.DonHangId == order && v.TrangThai == TrangThaiSuDungVoucher.DangGiu)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.TrangThai, TrangThaiSuDungVoucher.DaNha), ct);
    }
}
