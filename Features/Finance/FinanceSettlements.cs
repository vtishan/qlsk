using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Finance;

public sealed partial class FinanceService
{
    private static DateTimeOffset? SettlementLockedAt(DoiSoat row)
    {
        using var json = JsonDocument.Parse(row.ChiTietDoiSoat);
        return json.RootElement.TryGetProperty("lockedAt", out var value) && value.ValueKind == JsonValueKind.String && value.TryGetDateTimeOffset(out var time)
            ? time : Stamp(row.NgayDuyet);
    }
    private async Task<int> SettlementSetting(string key, int fallback, int min, int max, CancellationToken ct)
    {
        var value = await db.CauHinhHeThong.AsNoTracking().Where(c => c.Khoa == key).Select(c => c.GiaTri).SingleOrDefaultAsync(ct);
        if (value == null) return fallback;
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number < min || number > max)
            throw Conflict("Cấu hình kỳ đối soát không hợp lệ. Quản trị viên cần kiểm tra.");
        return number;
    }

    public async Task<PageDto<DetailFieldDto>> EligibleSettlements(TaiKhoan actor, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, Permission);
        var days = await SettlementSetting("settlement.closeDays", 3, 0, 365, ct);
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var rows = await db.SuKien.AsNoTracking().Where(e => e.TrangThai == TrangThaiSuKien.HoanTat &&
            db.SuatDien.Any(s => s.SuKienId == e.Id) && !db.SuatDien.Any(s => s.SuKienId == e.Id && s.ThoiGianKetThuc > cutoff))
            .OrderByDescending(e => e.Id).Take(200).Select(e => new DetailFieldDto(e.Id.ToString(), e.TenSuKien)).ToListAsync(ct);
        return new(rows);
    }

    public async Task<int> CreateSettlement(TaiKhoan actor, SettlementCreateRequest request, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, Permission);
        var now = DateTime.UtcNow;
        if (request.From.Year < 1970 || request.To.Year > 9998 || request.From > request.To || request.To > VietnamDate(now)) throw Invalid("Chọn kỳ ngày đã kết thúc theo giờ Việt Nam.");
        // Serializable range reads protect receipts/refunds against writers that do not share event application locks.
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await SqlOperations.Lock(db, $"qlsk:event:{request.EventId}", ct);
        var evt = await db.SuKien.SingleOrDefaultAsync(e => e.Id == request.EventId, ct) ?? throw Missing();
        if (await Owned(actor).AnyAsync(o => o.Id == evt.BTCId, ct)) throw Denied();
        var days = await SettlementSetting("settlement.closeDays", 3, 0, 365, ct);
        var hours = await SettlementSetting("settlement.responseHours", 48, 1, 720, ct);
        var ends = await db.SuatDien.Where(s => s.SuKienId == evt.Id).Select(s => s.ThoiGianKetThuc).ToListAsync(ct);
        if (evt.TrangThai != TrangThaiSuKien.HoanTat || ends.Count == 0 || ends.Max().AddDays(days) > now)
            throw Conflict("Sự kiện chưa hoàn tất hoặc chưa hết thời hạn khóa sổ.");
        if (await db.DoiSoat.AnyAsync(d => d.SuKienId == evt.Id && d.TuNgay <= request.To && d.DenNgay >= request.From, ct))
            throw Conflict("Kỳ này trùng hoặc chồng lấn với bảng đối soát đã lập.");
        var orderIds = db.DonHang.Where(o => db.SuatDien.Any(s => s.Id == o.SuatDienId && s.SuKienId == evt.Id)).Select(o => o.Id);
        var payments = db.GiaoDichThanhToan.Where(p => p.DonHangId.HasValue && orderIds.Contains(p.DonHangId.Value));
        if (await payments.AnyAsync(p => p.TrangThai == TrangThaiGiaoDichThanhToan.NghiVan || p.TrangThai == TrangThaiGiaoDichThanhToan.ThuTrungCanHoan ||
            p.TrangThai == TrangThaiGiaoDichThanhToan.ChoThanhToan && p.SoTienThucThu > 0, ct))
            throw Conflict("Sự kiện còn khoản thu cần đối soát hoặc xử lý hoàn.");
        var paymentIds = payments.Select(p => p.Id);
        var refunds = db.YeuCauHoanTien.Where(r => paymentIds.Contains(r.GiaoDichId));
        if (await refunds.AnyAsync(r => r.TrangThai != TrangThaiYeuCauHoanTien.HoanTat && r.TrangThai != TrangThaiYeuCauHoanTien.TuChoi, ct) ||
            await db.KhieuNai.AnyAsync(k => (k.SuKienId == evt.Id || db.Ve.Any(v => v.Id == k.VeId &&
                db.SuatDien.Any(s => s.Id == v.SuatDienId && s.SuKienId == evt.Id))) &&
                (k.TrangThai == TrangThaiKhieuNai.ChoXuLy || k.TrangThai == TrangThaiKhieuNai.DangXuLy), ct))
            throw Conflict("Cần xử lý hết yêu cầu hoàn tiền và khiếu nại trước khi khóa sổ.");
        var periodStart = request.From.ToDateTime(TimeOnly.MinValue).AddHours(-7);
        var periodEnd = request.To.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-7);
        var receipts = await (from p in payments join o in db.DonHang on p.DonHangId equals o.Id
            where p.TrangThai == TrangThaiGiaoDichThanhToan.ThanhCong && p.NgayThanhToan >= periodStart && p.NgayThanhToan < periodEnd
            select new { Payment = p, Order = o }).ToListAsync(ct);
        if (receipts.Count == 0) throw Conflict("Kỳ đã chọn không có khoản thu đã xác minh.");
        var entries = new List<object>();
        decimal gross = 0, refunded = 0, fee = 0;
        foreach (var row in receipts)
        {
            var p = row.Payment;
            var o = row.Order;
            if (p.SoTienThucThu != o.TongTien || p.SoTienYeuCau != o.TongTien || p.NgayThanhToan == null ||
                p.PhuongThuc != PhuongThucThanhToan.MienPhi && string.IsNullOrWhiteSpace(p.MaGiaoDich))
                throw Conflict("Khoản thu thiếu chứng cứ hoặc không khớp số tiền đơn.");
            var ticketRevenue = o.TienVe - o.TienGiam;
            if (ticketRevenue < 0 || ticketRevenue + o.PhiDichVu != p.SoTienThucThu || o.PhiNenTang < 0 || o.PhiNenTang > ticketRevenue)
                throw Conflict("Số tiền vé, phí hoặc giảm giá của đơn không nhất quán.");
            var contract = o.HopDongId.HasValue ? await db.HopDong.SingleOrDefaultAsync(c => c.Id == o.HopDongId, ct) : null;
            if (ticketRevenue > 0 && (contract == null || contract.BTCId != evt.BTCId || contract.SuKienId.HasValue && contract.SuKienId != evt.Id ||
                contract.HieuLucTu > o.NgayTao || contract.HieuLucDen.HasValue && contract.HieuLucDen <= o.NgayTao))
                throw Conflict("Đơn có doanh thu thiếu hợp đồng phí đã chốt. Không tính lại bằng hợp đồng hiện tại.");
            var requests = await refunds.Where(r => r.GiaoDichId == p.Id && r.TrangThai == TrangThaiYeuCauHoanTien.HoanTat).ToListAsync(ct);
            decimal returned = 0;
            foreach (var refund in requests)
            {
                var transfers = await db.GiaoDichHoanTien.Where(t => t.YeuCauHoanTienId == refund.Id && t.TrangThai == TrangThaiChuyenTien.ThanhCong).ToListAsync(ct);
                if (!refund.SoTienDuyet.HasValue || transfers.Count == 0 || transfers.Any(t => t.NgayXuLy == null || string.IsNullOrWhiteSpace(t.MaGiaoDichHoan) && string.IsNullOrWhiteSpace(t.ChungTu)) ||
                    transfers.Sum(t => t.SoTien) != refund.SoTienDuyet) throw Conflict("Khoản hoàn hoàn tất thiếu chứng từ chuyển tiền thực tế.");
                returned += transfers.Sum(t => t.SoTien);
            }
            if (returned > ticketRevenue) throw Conflict("Khoản hoàn vượt doanh thu vé. Cần tài chính kiểm tra khoản phí hoàn.");
            gross += ticketRevenue;
            refunded += returned;
            fee += o.PhiNenTang;
            entries.Add(new { orderId = o.Id, paymentId = p.Id, receipt = p.SoTienThucThu, serviceFee = o.PhiDichVu,
                ticketRevenue, refunded = returned, platformFee = o.PhiNenTang, contractId = o.HopDongId,
                contract = contract == null ? null : new { contract.LoaiPhi, contract.TyLePhi, contract.PhiMoiVe, contract.HieuLucTu, contract.HieuLucDen } });
        }
        var draft = new DoiSoat { SuKienId = evt.Id, PhienBan = 1, TuNgay = request.From, DenNgay = request.To,
            TongDoanhThu = gross, TongTienHoan = refunded, PhiNenTang = fee, TienDieuChinh = 0, TienBTCNhan = gross - refunded - fee,
            HanXacNhan = now.AddHours(hours), TrangThai = TrangThaiDoiSoat.ChoXacNhan,
            ChiTietDoiSoat = JsonSerializer.Serialize(new { lockedAt = now, closeDays = days, responseHours = hours,
                basis = "Doanh thu vé sau giảm; không bao gồm phí dịch vụ; phí theo bản chốt tại từng đơn; điều chỉnh bằng 0", entries }) };
        db.DoiSoat.Add(draft);
        await db.SaveChangesAsync(ct);
        SqlOperations.Audit(db, actor, "Khóa sổ kỳ đối soát", nameof(DoiSoat), draft.Id);
        var owner = await db.HoSoBTC.Where(h => h.Id == evt.BTCId).Select(h => h.TaiKhoanId).SingleAsync(ct);
        SqlOperations.Notify(db, owner, "Đối soát chờ xác nhận", evt.TenSuKien + " · Hạn phản hồi " + draft.HanXacNhan.ToString("O"), "settlement");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return draft.Id;
    }
}
