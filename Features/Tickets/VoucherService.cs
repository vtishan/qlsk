using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Tickets;

public sealed record VoucherWorkDto(int Id, string Code, string ScopeLabel, string RuleLabel,
    DateTimeOffset? ActiveTo, string Status, string StatusLabel, IReadOnlyList<string> AllowedActions,
    IReadOnlyList<DetailFieldDto>? Fields = null)
{
    public string Name => Code;
}

public sealed class VoucherService(AppDbContext db)
{
    internal static readonly string[] ReadOperations = ["work.vouchers.list", "work.vouchers.detail"];
    private const int Size = 30;

    private IQueryable<SuKien> OwnedEvents(TaiKhoan actor) => db.SuKien.Where(e => db.HoSoBTC.Any(h =>
        h.Id == e.BTCId && h.TaiKhoanId == actor.Id && h.TrangThai == TrangThaiHoSoToChuc.DaDuyet));

    private IQueryable<Voucher> Scoped(TaiKhoan actor)
    {
        if (actor.VaiTro != VaiTroTaiKhoan.BanToChuc) throw Denied();
        var events = OwnedEvents(actor);
        // Every explicit restriction must belong to the organizer, including session-only vouchers.
        return db.Voucher.AsNoTracking().Where(v =>
            (!v.SuKienId.HasValue || events.Any(e => e.Id == v.SuKienId)) &&
            (!v.SuatDienId.HasValue || db.SuatDien.Any(s => s.Id == v.SuatDienId && events.Any(e => e.Id == s.SuKienId))));
    }

    public async Task<PageDto<VoucherWorkDto>> List(TaiKhoan actor, int? eventId, string? status, string? cursor, CancellationToken ct)
    {
        var query = Scoped(actor).Where(v => v.Id > After(cursor));
        if (eventId.HasValue)
        {
            if (!await OwnedEvents(actor).AnyAsync(e => e.Id == eventId, ct)) throw Missing();
            query = query.Where(v => (!v.SuKienId.HasValue || v.SuKienId == eventId) &&
                (!v.SuatDienId.HasValue || db.SuatDien.Any(s => s.Id == v.SuatDienId && s.SuKienId == eventId)));
        }
        if (status != null)
        {
            var phase = Enum.GetValues<TrangThaiVoucher>().SingleOrDefault(s => State(s).Code == status);
            if (phase == default) throw Invalid("Trạng thái khuyến mãi không hợp lệ.");
            query = query.Where(v => v.TrangThai == phase);
        }
        var rows = await Rows(query).OrderBy(r => r.Voucher.Id).Take(Size + 1).ToListAsync(ct);
        return Page(rows.Select(r => Project(r, false)).ToArray(), Size, r => r.Id);
    }

    public async Task<VoucherWorkDto> Detail(TaiKhoan actor, int id, CancellationToken ct) =>
        Project(await Rows(Scoped(actor)).SingleOrDefaultAsync(r => r.Voucher.Id == id, ct) ?? throw Missing(), true);

    private sealed class Row
    {
        public required Voucher Voucher { get; init; }
        public string? EventName { get; init; }
        public string? ZoneName { get; init; }
    }
    private IQueryable<Row> Rows(IQueryable<Voucher> vouchers) => vouchers.Select(v => new Row {
        Voucher = v,
        EventName = db.SuKien.Where(e => e.Id == v.SuKienId || db.SuatDien.Any(s => s.Id == v.SuatDienId && s.SuKienId == e.Id))
            .Select(e => e.TenSuKien).FirstOrDefault(),
        ZoneName = db.KhuVuc.Where(z => z.Id == v.KhuVucId).Select(z => z.TenKhuVuc).FirstOrDefault()
    });

    private static VoucherWorkDto Project(Row row, bool detail)
    {
        var v = row.Voucher;
        var state = State(v.TrangThai);
        var rule = v.LoaiGiam == LoaiGiamGia.PhanTram
            ? v.GiaTri.ToString("0.##", CultureInfo.InvariantCulture) + "% · tối đa " + Money(v.MucGiamToiDa ?? 0)
            : Money(v.GiaTri);
        var scope = row.EventName ?? "Toàn hệ thống";
        if (v.SuatDienId.HasValue) scope += " · Suất #" + v.SuatDienId;
        if (row.ZoneName != null) scope += " · " + row.ZoneName;
        return new(v.Id, v.MaVoucher, scope, rule, Stamp(v.NgayKetThuc), state.Code, state.Label, [], detail ? [
            new("Hiệu lực từ", Stamp(v.NgayBatDau)?.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm")),
            new("Đơn tối thiểu", Money(v.GiaTriDonToiThieu)), new("Tổng lượt", v.SoLuong.ToString(CultureInfo.InvariantCulture)),
            new("Lượt mỗi tài khoản", v.LuotMoiTaiKhoan.ToString(CultureInfo.InvariantCulture)), new("Nguồn tài trợ", v.NguonTaiTro)] : null);
    }
    private static string Money(decimal value) => value.ToString("0", CultureInfo.InvariantCulture) + " ₫";
    private static (string Code, string Label) State(TrangThaiVoucher state) => state switch {
        TrangThaiVoucher.Nhap => ("DRAFT", "Bản nháp"), TrangThaiVoucher.ChoDuyet => ("PENDING", "Chờ duyệt"),
        TrangThaiVoucher.HoatDong => ("ACTIVE", "Đang áp dụng"), TrangThaiVoucher.TamDung => ("PAUSED", "Tạm dừng"),
        TrangThaiVoucher.TuChoi => ("REJECTED", "Từ chối"), _ => ("EXPIRED", "Hết hạn")
    };
}

