using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Finance;

public sealed partial class FinanceService(AppDbContext db)
{
    internal const string Permission = "finance.review";
    internal static readonly string[] FinanceOperations = [
        "work.payments.list", "work.payments.detail", "work.settlements.approve", "work.settlements.create"];
    internal static readonly string[] OrganizerOperations = ["work.settlements.respond"];
    internal static readonly string[] AdminOperations = ["work.contracts.create"];
    internal static readonly string[] ReadOperations = ["work.contracts.list", "work.contracts.detail",
        "work.settlements.list", "work.settlements.detail", "work.payouts.list", "work.payouts.detail"];
    private const int Size = 30;

    private static bool Finance(TaiKhoan actor) => AccountAccess.Platform(actor, Permission);
    private static void CheckFinancialScope(TaiKhoan actor)
    {
        if (!Finance(actor) && actor.VaiTro != VaiTroTaiKhoan.BanToChuc) throw Denied();
    }
    private IQueryable<HoSoBTC> Owned(TaiKhoan actor) => db.HoSoBTC.Where(o => o.TaiKhoanId == actor.Id && o.TrangThai == TrangThaiHoSoToChuc.DaDuyet);
    private IQueryable<SuKien> Events(TaiKhoan actor) => Finance(actor) ? db.SuKien :
        db.SuKien.Where(e => Owned(actor).Any(o => o.Id == e.BTCId));

    public async Task<PageDto<PaymentDto>> Payments(TaiKhoan actor, string? status, string? cursor, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, Permission);
        var after = After(cursor);
        var query = db.GiaoDichThanhToan.AsNoTracking().Where(p => p.Id > after);
        if (status != null)
        {
            var code = Enum.GetValues<TrangThaiGiaoDichThanhToan>().SingleOrDefault(p => PaymentStatus(p).Code == status);
            if (code == default) throw Invalid("Trạng thái giao dịch không hợp lệ.");
            query = query.Where(p => p.TrangThai == code);
        }
        var rows = await query.OrderBy(p => p.Id).Take(Size + 1).ToListAsync(ct);
        return Page(rows.Select(p => PaymentRow(p, false)).ToArray(), Size, p => p.Id);
    }

    public async Task<PaymentDto> Payment(TaiKhoan actor, int id, CancellationToken ct)
    {
        var privileged = Finance(actor);
        var item = await db.GiaoDichThanhToan.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id && (privileged ||
            db.DonHang.Any(o => o.Id == p.DonHangId && o.TaiKhoanId == actor.Id) ||
            db.YeuCauThue.Any(r => r.Id == p.YeuCauThueId && r.TaiKhoanId == actor.Id)), ct) ?? throw Missing();
        return PaymentRow(item, true);
    }

    private static PaymentDto PaymentRow(GiaoDichThanhToan p, bool detail)
    {
        var state = PaymentStatus(p.TrangThai);
        return new(p.Id, $"Giao dịch #{p.Id}", p.DonHangId.HasValue ? $"Đơn #{p.DonHangId}" : $"Thuê #{p.YeuCauThueId}",
            p.SoTienThucThu ?? p.SoTienYeuCau, Stamp(p.NgayThanhToan), state.Code, state.Label,
            detail ? [new("Số tiền yêu cầu", Money(p.SoTienYeuCau)), new("Số tiền thực thu", p.SoTienThucThu.HasValue ? Money(p.SoTienThucThu.Value) : null),
                new("Phương thức", p.PhuongThuc == PhuongThucThanhToan.MienPhi ? "Miễn phí" : p.PhuongThuc.ToString()),
                new("Mã giao dịch đối tác", p.MaGiaoDich)] : null);
    }

    private static (string Code, string Label) PaymentStatus(TrangThaiGiaoDichThanhToan status) => status switch {
        TrangThaiGiaoDichThanhToan.ThanhCong => ("SUCCESS", "Thành công"), TrangThaiGiaoDichThanhToan.ThatBai => ("FAILED", "Thất bại"),
        TrangThaiGiaoDichThanhToan.HetHan => ("EXPIRED", "Hết hạn"), TrangThaiGiaoDichThanhToan.NghiVan => ("REVIEW", "Cần đối soát"),
        TrangThaiGiaoDichThanhToan.ThuTrungCanHoan => ("PAID_OUT_OF_STOCK", "Cần xử lý khoản đã thu"), _ => ("PENDING", "Đang chờ")
    };

    public Task<PageDto<ContractDto>> Contracts(TaiKhoan actor, string? cursor, CancellationToken ct) => ContractRows(actor, After(cursor), null, ct);
    public async Task<ContractDto> Contract(TaiKhoan actor, int id, CancellationToken ct) =>
        (await ContractRows(actor, 0, id, ct)).Items.SingleOrDefault() ?? throw Missing();

    private async Task<PageDto<ContractDto>> ContractRows(TaiKhoan actor, int after, int? id, CancellationToken ct)
    {
        CheckFinancialScope(actor);
        var query = db.HopDong.AsNoTracking();
        if (!Finance(actor)) query = query.Where(c => Owned(actor).Any(h => h.Id == c.BTCId));
        var rows = await (from c in query join h in db.HoSoBTC on c.BTCId equals h.Id
                          where c.Id > after && (!id.HasValue || c.Id == id) orderby c.Id
                          select new { Contract = c, h.TenToChuc }).Take(Size + 1).ToListAsync(ct);
        return Page(rows.Select(r =>
        {
            var c = r.Contract;
            var rate = c.LoaiPhi == LoaiPhiHopDong.PhanTram ? c.TyLePhi?.ToString(CultureInfo.InvariantCulture) + "%" : Money(c.PhiMoiVe ?? 0) + " / vé";
            return new ContractDto(c.Id, $"Hợp đồng #{c.Id}", r.TenToChuc, rate, Stamp(c.HieuLucTu), Stamp(c.HieuLucDen), c.TrangThai.ToString(),
                id.HasValue ? [new("Điều khoản", c.NoiDung), new("Sự kiện riêng", c.SuKienId?.ToString())] : null,
                ContractStatusLabel(c.TrangThai));
        }).ToArray(), Size, c => c.Id);
    }

    private static string ContractStatusLabel(TrangThaiHopDong status) => status switch
    {
        TrangThaiHopDong.Nhap => "Bản nháp",
        TrangThaiHopDong.CoHieuLuc => "Có hiệu lực",
        TrangThaiHopDong.HetHan => "Hết hạn",
        TrangThaiHopDong.DaHuy => "Đã hủy",
        _ => "Không xác định"
    };

    public async Task<int> CreateContract(TaiKhoan actor, ContractCreateRequest request, CancellationToken ct)
    {
        if (!AccountAccess.Admin(actor)) throw Denied();
        var type = request.FeeType switch {
            "PERCENT" => LoaiPhiHopDong.PhanTram, "FIXED" => LoaiPhiHopDong.MoiVe, _ => throw Invalid("Chọn cách tính phí.")
        };
        if (request.FeeValue < 0 || (type == LoaiPhiHopDong.PhanTram && (request.FeeValue > 100 || decimal.Round(request.FeeValue, 2) != request.FeeValue)) ||
            (type == LoaiPhiHopDong.MoiVe && decimal.Truncate(request.FeeValue) != request.FeeValue) || request.To <= request.From ||
            request.To <= DateTimeOffset.UtcNow)
            throw Invalid("Mức phí và thời gian hiệu lực không hợp lệ.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:contract:{request.OrganizerId}", ct);
        if (!await db.HoSoBTC.AnyAsync(h => h.Id == request.OrganizerId && h.TrangThai == TrangThaiHoSoToChuc.DaDuyet, ct)) throw Missing();
        var from = request.From.UtcDateTime;
        var to = request.To.UtcDateTime;
        if (await db.HopDong.AnyAsync(c => c.BTCId == request.OrganizerId && c.SuKienId == null && c.TrangThai == TrangThaiHopDong.CoHieuLuc &&
            c.HieuLucTu < to && (!c.HieuLucDen.HasValue || c.HieuLucDen > from), ct))
            throw Conflict("Ban tổ chức đã có hợp đồng phí trùng thời gian hiệu lực.");
        var row = new HopDong { BTCId = request.OrganizerId, LoaiPhi = type,
            TyLePhi = type == LoaiPhiHopDong.PhanTram ? request.FeeValue : null,
            PhiMoiVe = type == LoaiPhiHopDong.MoiVe ? request.FeeValue : null, NgayKy = DateTime.UtcNow,
            HieuLucTu = from, HieuLucDen = to, TrangThai = TrangThaiHopDong.CoHieuLuc,
            NoiDung = "Biểu phí QLSK đã được quản trị xác nhận." };
        db.HopDong.Add(row);
        await db.SaveChangesAsync(ct);
        SqlOperations.Audit(db, actor, "Lập hợp đồng phí", nameof(HopDong), row.Id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return row.Id;
    }

    public Task<PageDto<SettlementDto>> Settlements(TaiKhoan actor, int? eventId, string? status, string? cursor, CancellationToken ct) =>
        SettlementRows(actor, After(cursor), null, eventId, status, ct);
    public async Task<SettlementDto> Settlement(TaiKhoan actor, int id, CancellationToken ct) =>
        (await SettlementRows(actor, 0, id, null, null, ct)).Items.SingleOrDefault() ?? throw Missing();

    private async Task<PageDto<SettlementDto>> SettlementRows(TaiKhoan actor, int after, int? id, int? eventId, string? status, CancellationToken ct)
    {
        CheckFinancialScope(actor);
        var query = db.DoiSoat.AsNoTracking().Where(d => d.Id > after && (!id.HasValue || d.Id == id) && (!eventId.HasValue || d.SuKienId == eventId));
        if (status is "MANAGER_APPROVED" or "PAYOUT_PENDING")
            query = query.Where(d => d.TrangThai == TrangThaiDoiSoat.ChoGiaiNgan &&
                db.GiaiNgan.Any(p => p.DoiSoatId == d.Id) == (status == "PAYOUT_PENDING"));
        else if (status != null)
        {
            var phase = status switch {
                "DRAFT" => TrangThaiDoiSoat.ChoXacNhan, "ORGANIZER_CONFIRMED" => TrangThaiDoiSoat.DaXacNhan,
                "DISPUTED" => TrangThaiDoiSoat.TranhChap, "PAID" => TrangThaiDoiSoat.HoanTat,
                _ => throw Invalid("Trạng thái đối soát không hợp lệ.")
            };
            query = query.Where(d => d.TrangThai == phase);
        }
        var rows = await (from d in query join e in Events(actor) on d.SuKienId equals e.Id orderby d.Id
            select new { Value = d, e.TenSuKien, e.BTCId, HasPayout = db.GiaiNgan.Any(p => p.DoiSoatId == d.Id) })
            .Take(Size + 1).ToListAsync(ct);
        var owners = await Owned(actor).Select(o => o.Id).ToListAsync(ct);
        return Page(rows.Select(r =>
        {
            var d = r.Value;
            var phase = SettlementStatus(d.TrangThai, r.HasPayout);
            var actions = new List<string>();
            if (id.HasValue && d.TrangThai == TrangThaiDoiSoat.ChoXacNhan && owners.Contains(r.BTCId)) actions.Add("respond");
            if (id.HasValue && d.TrangThai == TrangThaiDoiSoat.DaXacNhan && Finance(actor) && !owners.Contains(r.BTCId)) actions.Add("approve");
            return new SettlementDto(d.Id, $"Đối soát #{d.Id}", r.TenSuKien, d.TienBTCNhan, SettlementLockedAt(d), phase.Code, phase.Label, actions,
                id.HasValue ? [new("Doanh thu vé thực thu", Money(d.TongDoanhThu)), new("Đã hoàn", Money(d.TongTienHoan)),
                    new("Phí nền tảng", Money(d.PhiNenTang)), new("Điều chỉnh", Money(d.TienDieuChinh)),
                    new("Từ ngày", d.TuNgay.ToString("dd/MM/yyyy")), new("Đến ngày", d.DenNgay.ToString("dd/MM/yyyy")),
                    new("Hạn BTC phản hồi", new DateTimeOffset(DateTime.SpecifyKind(d.HanXacNhan, DateTimeKind.Utc)).ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm"))] : null);
        }).ToArray(), Size, d => d.Id);
    }

    public Task<SettlementDto> Respond(TaiKhoan actor, int id, DecisionRequest request, CancellationToken ct) =>
        ChangeSettlement(actor, id, request, false, ct);
    public Task<SettlementDto> Approve(TaiKhoan actor, int id, DecisionRequest request, CancellationToken ct) =>
        ChangeSettlement(actor, id, request, true, ct);

    private async Task<SettlementDto> ChangeSettlement(TaiKhoan actor, int id, DecisionRequest request, bool management, CancellationToken ct)
    {
        var note = Required(request.Reason, 1000, "lý do đối soát");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:settlement:{id}", ct);
        var row = await db.DoiSoat.SingleOrDefaultAsync(d => d.Id == id, ct) ?? throw Missing();
        var owns = await db.SuKien.AnyAsync(e => e.Id == row.SuKienId && Owned(actor).Any(h => h.Id == e.BTCId), ct);
        if (management ? !Finance(actor) || owns : !owns) throw Denied();
        if (request.ExpectedState != SettlementStatus(row.TrangThai, false).Code ||
            row.TrangThai != (management ? TrangThaiDoiSoat.DaXacNhan : TrangThaiDoiSoat.ChoXacNhan))
            throw Conflict("Bảng đối soát đã thay đổi.");
        if (management)
        {
            row.TrangThai = request.Action switch {
                "approve" => TrangThaiDoiSoat.ChoGiaiNgan, "reject" => TrangThaiDoiSoat.TranhChap,
                _ => throw Invalid("Quyết định đối soát không hợp lệ.")
            };
            row.NguoiDuyetId = actor.Id;
            row.NgayDuyet = DateTime.UtcNow;
        }
        else
        {
            if (row.HanXacNhan <= DateTime.UtcNow) throw Conflict("Đã hết hạn phản hồi đối soát.");
            row.TrangThai = request.Action switch {
                "confirm" => TrangThaiDoiSoat.DaXacNhan, "dispute" => TrangThaiDoiSoat.TranhChap,
                _ => throw Invalid("Phản hồi đối soát không hợp lệ.")
            };
        }
        SqlOperations.Audit(db, actor, SettlementStatus(row.TrangThai, false).Label, nameof(DoiSoat), id, note);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Settlement(actor, id, ct);
    }

    private static (string Code, string Label) SettlementStatus(TrangThaiDoiSoat status, bool payout) => status switch {
        TrangThaiDoiSoat.DaXacNhan => ("ORGANIZER_CONFIRMED", "BTC xác nhận"), TrangThaiDoiSoat.TranhChap => ("DISPUTED", "Đang tranh chấp"),
        TrangThaiDoiSoat.ChoGiaiNgan => payout ? ("PAYOUT_PENDING", "Chờ chi trả") : ("MANAGER_APPROVED", "Quản lý đã duyệt"),
        TrangThaiDoiSoat.HoanTat => ("PAID", "Đã chi trả"), _ => ("DRAFT", "Chờ BTC xác nhận")
    };

    public Task<PageDto<PayoutDto>> Payouts(TaiKhoan actor, string? status, string? cursor, CancellationToken ct) =>
        PayoutRows(actor, After(cursor), null, status, ct);
    public async Task<PayoutDto> Payout(TaiKhoan actor, int id, CancellationToken ct) =>
        (await PayoutRows(actor, 0, id, null, ct)).Items.SingleOrDefault() ?? throw Missing();

    private async Task<PageDto<PayoutDto>> PayoutRows(TaiKhoan actor, int after, int? id, string? status, CancellationToken ct)
    {
        CheckFinancialScope(actor);
        var query = db.GiaiNgan.AsNoTracking().Where(p => p.Id > after && (!id.HasValue || p.Id == id));
        if (status != null)
        {
            var phase = Enum.GetValues<TrangThaiChuyenTien>().SingleOrDefault(s => PayoutStatus(s).Code == status);
            if (phase == default) throw Invalid("Trạng thái giải ngân không hợp lệ.");
            query = query.Where(p => p.TrangThai == phase);
        }
        var rows = await (from p in query join d in db.DoiSoat on p.DoiSoatId equals d.Id
            join e in Events(actor) on d.SuKienId equals e.Id join o in db.HoSoBTC on e.BTCId equals o.Id orderby p.Id
            select new { Payout = p, o.TenToChuc }).Take(Size + 1).ToListAsync(ct);
        return Page(rows.Select(r =>
        {
            var p = r.Payout;
            var phase = PayoutStatus(p.TrangThai);
            return new PayoutDto(p.Id, $"Giải ngân #{p.Id}", r.TenToChuc, p.SoTien, Stamp(p.NgayGiaiNgan), phase.Code, phase.Label,
                id.HasValue ? [new("Kỳ đối soát", p.DoiSoatId.ToString()), new("Mã giao dịch", p.MaGiaoDich)] : null);
        }).ToArray(), Size, p => p.Id);
    }

    private static (string Code, string Label) PayoutStatus(TrangThaiChuyenTien status) => status switch {
        TrangThaiChuyenTien.DangChuyen => ("PROCESSING", "Đang chuyển"), TrangThaiChuyenTien.ThanhCong => ("SUCCESS", "Đã chuyển"),
        TrangThaiChuyenTien.ThatBai => ("FAILED", "Thất bại"), _ => ("PENDING_APPROVAL", "Chờ chuyển")
    };
    private static string Money(decimal amount) => amount.ToString("0", CultureInfo.InvariantCulture) + " ₫";
}
