using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Requests;

public sealed class RefundService(AppDbContext db, RequestService requests)
{
    internal const string ReviewPermission = "refunds.review";
    internal static readonly string[] UserOperations = ["requests.refunds.list", "requests.refunds.detail", "requests.refunds.create", "tickets.eligibleRefunds"];
    internal static readonly string[] ReviewOperations = ["work.refunds.list", "work.refunds.detail", "work.refunds.decide"];

    // Requests reserve the paid ticket share for manual policy review; they never assert a bank transfer.
    public async Task<PageDto<RefundEligibleDto>> Eligible(TaiKhoan actor, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddHours(72);
        var rows = await (from ticket in db.Ve.AsNoTracking()
                          join line in db.ChiTietDonHang on ticket.ChiTietDonHangId equals line.Id
                          join order in db.DonHang on line.DonHangId equals order.Id
                          join slot in db.SuatDien on ticket.SuatDienId equals slot.Id
                          join evt in db.SuKien on slot.SuKienId equals evt.Id
                          join payment in db.GiaoDichThanhToan on order.Id equals payment.DonHangId
                          where order.TaiKhoanId == actor.Id && ticket.TrangThai == TrangThaiVe.DaPhatHanh &&
                            evt.ChoPhepTuHuyVe && slot.ThoiGianBatDau >= cutoff && order.TienVe > order.TienGiam &&
                            payment.TrangThai == TrangThaiGiaoDichThanhToan.ThanhCong && payment.SoTienThucThu > 0 &&
                            !db.YeuCauHoanTien.Any(r => r.VeId == ticket.Id && r.TrangThai != TrangThaiYeuCauHoanTien.TuChoi)
                          orderby ticket.Id, payment.Id
                          select new RefundEligibleDto(ticket.Id, evt.TenSuKien + " · " + ticket.MaVe,
                            payment.Id, order.Id, "Đơn #" + order.Id, evt.TenSuKien,
                            db.Ghe.Where(g => g.Id == ticket.GheId).Select(g => g.MaGhe).FirstOrDefault()))
                          .Take(400).ToListAsync(ct);
        return new(rows.DistinctBy(r => r.Id).ToArray());
    }

    public async Task<int> Create(TaiKhoan actor, RefundCreateRequest request, CancellationToken ct)
    {
        var ids = request.TicketIds?.Distinct().Order().ToArray() ?? [];
        if (ids.Length is < 1 or > 10) throw Invalid("Chọn từ 1 đến 10 vé.");
        var reason = Required(request.Reason, 180, "lý do hoàn");
        var description = Optional(request.Description, 500, "mô tả");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:refund-payment:{request.TransactionId}", ct);
        var payment = await db.GiaoDichThanhToan.AsNoTracking().SingleOrDefaultAsync(p => p.Id == request.TransactionId, ct) ?? throw Missing();
        var order = await db.DonHang.AsNoTracking().SingleOrDefaultAsync(o => o.Id == payment.DonHangId && o.TaiKhoanId == actor.Id, ct) ?? throw Missing();
        var previous = await db.YeuCauHoanTien.Where(r => r.TaiKhoanId == actor.Id && r.GiaoDichId == payment.Id &&
            r.VeId.HasValue && ids.Contains(r.VeId.Value) && r.TrangThai != TrangThaiYeuCauHoanTien.TuChoi).ToListAsync(ct);
        if (previous.Count == ids.Length) return previous[0].Id;
        if (previous.Count != 0) throw Conflict("Một số vé đã có yêu cầu hoàn. Tải lại danh sách.");
        var eligible = (await Eligible(actor, ct)).Items;
        if (ids.Any(id => !eligible.Any(t => t.Id == id && t.TransactionId == payment.Id)))
            throw Conflict("Vé không đáp ứng điều kiện tự hủy trước 72 giờ hoặc không có giao dịch thực thu.");
        var shares = await (from ticket in db.Ve join line in db.ChiTietDonHang on ticket.ChiTietDonHangId equals line.Id
                            where ids.Contains(ticket.Id) && line.DonHangId == order.Id
                            select new { ticket.Id, line.DonGia }).ToListAsync(ct);
        var reserved = await db.YeuCauHoanTien.Where(r => r.GiaoDichId == payment.Id && r.TrangThai != TrangThaiYeuCauHoanTien.TuChoi)
            .SumAsync(r => r.SoTienDuyet ?? r.SoTienYeuCau, ct);
        var amounts = shares.Select(s => new { s.Id, Amount = decimal.Floor(s.DonGia * (order.TienVe - order.TienGiam) / order.TienVe) }).ToArray();
        if (amounts.Any(s => s.Amount <= 0) || reserved + amounts.Sum(s => s.Amount) > payment.SoTienThucThu)
            throw Conflict("Số tiền yêu cầu vượt số tiền thực thu còn có thể hoàn.");
        var records = amounts.Select(s => new YeuCauHoanTien {
            TaiKhoanId = actor.Id, GiaoDichId = payment.Id, VeId = s.Id,
            KhoaChongLap = "ticket:" + s.Id + ":" + Guid.NewGuid().ToString("N"),
            SoTienYeuCau = s.Amount, LoaiYeuCau = "Tự hủy vé · xét điều khoản",
            LyDo = reason + (description == null ? "" : " · " + description),
            TrangThai = TrangThaiYeuCauHoanTien.ChoDuyet, NgayYeuCau = DateTime.UtcNow
        }).ToArray();
        db.YeuCauHoanTien.AddRange(records);
        await db.SaveChangesAsync(ct);
        foreach (var record in records) SqlOperations.Audit(db, actor, "Gửi yêu cầu hoàn vé", nameof(YeuCauHoanTien), record.Id, reason);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return records[0].Id;
    }

    public async Task<PageDto<RefundDto>> List(TaiKhoan actor, bool review, string? status, string? cursor, CancellationToken ct)
    {
        if (review) AccountAccess.NeedPlatform(actor, ReviewPermission);
        const int size = 30;
        var after = After(cursor);
        var query = db.YeuCauHoanTien.AsNoTracking().Where(r => r.Id > after);
        if (!review) query = query.Where(r => r.TaiKhoanId == actor.Id);
        if (status != null)
        {
            var phase = status == "MANUAL_REVIEW" ? TrangThaiYeuCauHoanTien.ChoDuyet :
                Enum.GetValues<TrangThaiYeuCauHoanTien>().SingleOrDefault(p => State(p).Code == status);
            if (phase == default) return new([]);
            query = query.Where(r => r.TrangThai == phase);
        }
        var rows = await (from r in query join p in db.GiaoDichThanhToan on r.GiaoDichId equals p.Id orderby r.Id
                          select new { Refund = r, p.DonHangId }).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(r => Row(r.Refund, r.DonHangId)).ToArray(), size, r => r.Id);
    }

    public async Task<RefundDto> Detail(TaiKhoan actor, int id, CancellationToken ct)
    {
        var permitted = AccountAccess.Platform(actor, ReviewPermission);
        var r = await (from refund in db.YeuCauHoanTien.AsNoTracking()
                       join payment in db.GiaoDichThanhToan on refund.GiaoDichId equals payment.Id
                       where refund.Id == id && (permitted || refund.TaiKhoanId == actor.Id)
                       select new { Refund = refund, payment.DonHangId }).SingleOrDefaultAsync(ct) ?? throw Missing();
        var transfer = await db.GiaoDichHoanTien.AsNoTracking().Where(t => t.YeuCauHoanTienId == id)
            .OrderByDescending(t => t.Id).Select(t => (TrangThaiChuyenTien?)t.TrangThai).FirstOrDefaultAsync(ct);
        return Row(r.Refund, r.DonHangId) with {
            TransferStatusLabel = transfer switch {
                TrangThaiChuyenTien.ChoChuyen => "Chờ chuyển tiền", TrangThaiChuyenTien.DangChuyen => "Đang chuyển tiền",
                TrangThaiChuyenTien.ThanhCong => "Đã chuyển tiền", TrangThaiChuyenTien.ThatBai => "Chuyển tiền thất bại",
                _ => "Chưa lập lệnh chuyển"
            },
            AllowedActions = permitted && r.Refund.TaiKhoanId != actor.Id && r.Refund.TrangThai == TrangThaiYeuCauHoanTien.ChoDuyet
                ? ["decide"] : [],
            History = await requests.History(nameof(YeuCauHoanTien), id, ct)
        };
    }

    public async Task<RefundDto> Decide(TaiKhoan actor, int id, DecisionRequest request, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, ReviewPermission);
        if (request.Action is not ("approve" or "reject")) throw Invalid("Quyết định hoàn tiền không hợp lệ.");
        var reason = Required(request.Reason, 1000, "lý do");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var paymentId = await db.YeuCauHoanTien.Where(r => r.Id == id).Select(r => (int?)r.GiaoDichId).SingleOrDefaultAsync(ct) ?? throw Missing();
        await SqlOperations.Lock(db, $"qlsk:refund-payment:{paymentId}", ct);
        var refund = await db.YeuCauHoanTien.SingleAsync(r => r.Id == id, ct);
        if (refund.TaiKhoanId == actor.Id) throw Denied();
        if (refund.TrangThai != TrangThaiYeuCauHoanTien.ChoDuyet || request.ExpectedState != State(refund.TrangThai).Code)
            throw Conflict("Yêu cầu hoàn đã có quyết định hoặc trạng thái thay đổi.");
        if (request.Action == "approve")
        {
            if (refund.VeId.HasValue)
            {
                var ticket = await db.Ve.AsNoTracking().SingleOrDefaultAsync(v => v.Id == refund.VeId.Value, ct);
                if (ticket == null || ticket.TrangThai != TrangThaiVe.DaPhatHanh || ticket.DaCheckInLuc.HasValue)
                    throw Conflict("Vé đã check-in, bị hủy hoặc thu hồi. Cần xử lý qua khiếu nại.");
            }
            var paid = await db.GiaoDichThanhToan.Where(p => p.Id == paymentId &&
                (p.TrangThai == TrangThaiGiaoDichThanhToan.ThanhCong || p.TrangThai == TrangThaiGiaoDichThanhToan.ThuTrungCanHoan))
                .Select(p => p.SoTienThucThu).SingleOrDefaultAsync(ct);
            var reserved = await db.YeuCauHoanTien.Where(r => r.GiaoDichId == paymentId && r.Id != id &&
                (r.TrangThai == TrangThaiYeuCauHoanTien.DaDuyet || r.TrangThai == TrangThaiYeuCauHoanTien.DangChuyen ||
                 r.TrangThai == TrangThaiYeuCauHoanTien.HoanTat || r.TrangThai == TrangThaiYeuCauHoanTien.ThatBai))
                .SumAsync(r => r.SoTienDuyet ?? 0, ct);
            if (!paid.HasValue || refund.SoTienYeuCau <= 0 || reserved + refund.SoTienYeuCau > paid.Value)
                throw Conflict("Số tiền yêu cầu vượt số thực thu có thể hoàn.");
            refund.SoTienDuyet = refund.SoTienYeuCau;
            refund.TrangThai = TrangThaiYeuCauHoanTien.DaDuyet;
        }
        else refund.TrangThai = TrangThaiYeuCauHoanTien.TuChoi;
        refund.NguoiDuyetId = actor.Id;
        refund.NgayDuyet = DateTime.UtcNow;
        var label = State(refund.TrangThai).Label;
        SqlOperations.Audit(db, actor, label, nameof(YeuCauHoanTien), id, reason);
        SqlOperations.Notify(db, refund.TaiKhoanId, "Cập nhật yêu cầu hoàn tiền", label, "refund");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Detail(actor, id, ct);
    }

    private static RefundDto Row(YeuCauHoanTien r, int? orderId)
    {
        var state = State(r.TrangThai);
        return new(r.Id, "Yêu cầu hoàn #" + r.Id, Stamp(r.NgayYeuCau), state.Code, state.Label,
            orderId.HasValue ? $"Đơn #{orderId}" : null, r.SoTienDuyet ?? r.SoTienYeuCau, r.LyDo, null, r.LoaiYeuCau);
    }

    internal static (string Code, string Label) State(TrangThaiYeuCauHoanTien state) => state switch {
        TrangThaiYeuCauHoanTien.ChoDuyet => ("REQUESTED", "Chờ xét duyệt"), TrangThaiYeuCauHoanTien.DaDuyet => ("APPROVED", "Đã duyệt"),
        TrangThaiYeuCauHoanTien.DangChuyen => ("PROCESSING", "Đang chuyển"), TrangThaiYeuCauHoanTien.HoanTat => ("COMPLETED", "Đã hoàn"),
        TrangThaiYeuCauHoanTien.ThatBai => ("FAILED", "Chuyển tiền thất bại"), _ => ("REJECTED", "Từ chối")
    };
}
