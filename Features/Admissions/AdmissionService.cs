using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Accounts;
using QLSK.Features.Events;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Admissions;

public sealed partial class AdmissionService(AppDbContext db, EventService events)
{
    internal const string Scan = "checkin.scan";
    internal const string Manual = "checkin.manual";
    internal const string View = "attendees.view";
    internal static readonly string[] ScanOperations = ["work.scan.confirm"];
    internal static readonly string[] ManualOperations = ["work.manual.lookup", "work.manual.confirm"];
    internal static readonly string[] AttendeeOperations = ["work.attendees.list", "work.attendees.detail"];
    internal static readonly string[] CheckInLogOperations = ["work.checkins.list", "work.checkins.detail"];

    internal async Task<bool> Has(TaiKhoan actor, SuatDien slot, string permission, CancellationToken ct)
    {
        if (await events.Owns(actor, slot.SuKienId, ct)) return true;
        return await db.PhanCongSuKien.AnyAsync(g => g.TaiKhoanId == actor.Id && g.SuKienId == slot.SuKienId &&
            (!g.SuatDienId.HasValue || g.SuatDienId == slot.Id) && g.TrangThai == TrangThaiPhanCongSuKien.DangHieuLuc &&
            g.LoaiQuyen == permission, ct);
    }

    private async Task<SuatDien> Slot(int id, CancellationToken ct) =>
        await db.SuatDien.AsNoTracking().SingleOrDefaultAsync(s => s.Id == id, ct) ?? throw Missing();

    public async Task<CheckInDto> CheckIn(TaiKhoan actor, int id, CheckInRequest request, CancellationToken ct)
    {
        var method = request.Method switch {
            "QR" => HinhThucCheckIn.QuetQR, "MANUAL" => HinhThucCheckIn.ThuCong,
            _ => throw Invalid("Phương thức check-in không hợp lệ.")
        };
        var slot = await Slot(id, ct);
        var permission = method == HinhThucCheckIn.QuetQR ? Scan : Manual;
        var code = method == HinhThucCheckIn.QuetQR ? Required(request.Code, 128, "mã QR") : null;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{slot.SuKienId}", ct);
        await SqlOperations.Lock(db, $"qlsk:session:{slot.Id}", ct);
        if (!await Has(actor, slot, permission, ct)) throw Denied();
        var evt = await db.SuKien.AsNoTracking().SingleAsync(e => e.Id == slot.SuKienId, ct);
        var current = await Slot(id, ct);
        if (evt.TrangThai is not (TrangThaiSuKien.DaDuyet or TrangThaiSuKien.DangMoBan) ||
            current.TrangThai is not (TrangThaiSuatDien.DaXacNhan or TrangThaiSuatDien.DangMoBan or TrangThaiSuatDien.DongBan) ||
            DateTime.UtcNow >= current.ThoiGianKetThuc)
            throw Conflict("Suất diễn đã kết thúc, hoãn, hủy hoặc đang bị đình chỉ.");
        var row = code == null ? await db.Ve.SingleOrDefaultAsync(v => v.Id == request.TicketId && v.SuatDienId == slot.Id, ct)
            : await db.Ve.SingleOrDefaultAsync(v => v.MaQR == code && v.SuatDienId == slot.Id, ct);
        if (row == null) return new("Từ chối", null, null, "Mã không thuộc suất diễn đã chọn.");
        if (row.TrangThai == TrangThaiVe.DaCheckIn)
            return new("Vé đã check-in", row.MaVe, Stamp(row.DaCheckInLuc), "Mỗi vé chỉ được check-in một lần.");
        if (row.TrangThai != TrangThaiVe.DaPhatHanh) return new("Từ chối", row.MaVe, null, "Vé đã bị hủy hoặc thu hồi.");
        var utc = DateTime.UtcNow;
        var now = utc.AddTicks(-(utc.Ticks % TimeSpan.TicksPerMillisecond));
        row.DaCheckInLuc = now;
        row.TrangThai = TrangThaiVe.DaCheckIn;
        db.NhatKyCheckIn.Add(new NhatKyCheckIn {
            VeId = row.Id, NhanVienId = actor.Id, ThoiGianQuet = now, HinhThuc = method, KetQua = "ThanhCong"
        });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new("Đã check-in", row.MaVe, Stamp(now));
    }

    private sealed class AttendeeRow
    {
        public required Ve Ticket { get; init; }
        public string? Buyer { get; init; }
        public string? Guest { get; init; }
        public string? Seat { get; init; }
    }
    private IQueryable<AttendeeRow> Rows(int sessionId) =>
        from v in db.Ve.AsNoTracking() where v.SuatDienId == sessionId select new AttendeeRow { Ticket = v,
            Buyer = (from l in db.ChiTietDonHang join o in db.DonHang on l.DonHangId equals o.Id join b in db.TaiKhoan on o.TaiKhoanId equals b.Id
             where l.Id == v.ChiTietDonHangId select b.HoTen).FirstOrDefault(),
            Guest = db.VeMoi.Where(m => m.VeId == v.Id).Select(m => m.HoTenKhachMoi).FirstOrDefault(),
            Seat = db.Ghe.Where(g => g.Id == v.GheId).Select(g => g.MaGhe).FirstOrDefault() };

    public async Task<PageDto<AttendeeDto>> Lookup(TaiKhoan actor, int id, AttendeeLookupRequest request, CancellationToken ct)
    {
        var slot = await Slot(id, ct);
        if (!await Has(actor, slot, Manual, ct)) throw Denied();
        var token = Required(request.Query, 240, "mã đơn hoặc liên hệ");
        var email = AccountService.Email(token, false);
        var phone = AccountService.Phone(token, false);
        var orderNumber = token.StartsWith("Đơn #", StringComparison.OrdinalIgnoreCase) ? token[5..] : token.TrimStart('#');
        int? order = int.TryParse(orderNumber, out var candidate) && candidate > 0 ? candidate : null;
        var rows = await Rows(id).Where(r => r.Ticket.MaVe == token || db.ChiTietDonHang.Any(l => l.Id == r.Ticket.ChiTietDonHangId &&
            db.DonHang.Any(o => o.Id == l.DonHangId && (o.Id == order || db.TaiKhoan.Any(a => a.Id == o.TaiKhoanId &&
                ((email != null && a.Email == email) || (phone != null && a.SoDienThoai == phone)))))) ||
            db.VeMoi.Any(m => m.VeId == r.Ticket.Id && ((email != null && m.Email == email) || (phone != null && m.SoDienThoai == phone))))
            .OrderBy(r => r.Ticket.Id).Take(20).ToListAsync(ct);
        return new(rows.Select(r => Project(r, true)).ToArray());
    }

    public async Task<PageDto<AttendeeDto>> Attendees(TaiKhoan actor, int id, string? status, string? cursor, CancellationToken ct)
    {
        var slot = await Slot(id, ct);
        if (!await Has(actor, slot, View, ct)) throw Denied();
        var after = After(cursor);
        const int size = 30;
        var rows = Rows(id).Where(r => r.Ticket.Id > after);
        if (status != null)
        {
            var phase = Enum.GetValues<TrangThaiVe>().SingleOrDefault(p => State(p) == status);
            rows = rows.Where(r => r.Ticket.TrangThai == phase);
        }
        var page = await rows.OrderBy(r => r.Ticket.Id).Take(size + 1).ToListAsync(ct);
        return Page(page.Select(r => Project(r, false)).ToArray(), size, r => r.Id);
    }

    public async Task<AttendeeDto> Attendee(TaiKhoan actor, int id, int ticketId, CancellationToken ct)
    {
        if (!await Has(actor, await Slot(id, ct), View, ct)) throw Denied();
        var row = await Rows(id).SingleOrDefaultAsync(r => r.Ticket.Id == ticketId, ct) ?? throw Missing();
        return Project(row, false);
    }

    private static AttendeeDto Project(AttendeeRow r, bool check) => new(r.Ticket.Id, r.Ticket.Id,
        r.Guest ?? r.Buyer ?? "Người tham dự", r.Ticket.MaVe, r.Seat, State(r.Ticket.TrangThai),
        check && r.Ticket.TrangThai == TrangThaiVe.DaPhatHanh ? ["check_in"] : []);

    private static string State(TrangThaiVe state) => state switch {
        TrangThaiVe.DaPhatHanh => "ISSUED", TrangThaiVe.DaCheckIn => "CHECKED_IN", TrangThaiVe.DaThuHoi => "REVOKED", _ => "CANCELLED"
    };

    private sealed class LogRow
    {
        public required NhatKyCheckIn Entry { get; init; }
        public required string Operator { get; init; }
        public required string Code { get; init; }
    }
    private IQueryable<LogRow> Logs(int id) =>
        from log in db.NhatKyCheckIn.AsNoTracking() join v in db.Ve on log.VeId equals v.Id
        join actor in db.TaiKhoan on log.NhanVienId equals actor.Id where v.SuatDienId == id
        select new LogRow { Entry = log, Operator = actor.HoTen, Code = v.MaVe };

    public async Task<PageDto<CheckInLogDto>> Log(TaiKhoan actor, int id, string? cursor, CancellationToken ct)
    {
        if (!await Has(actor, await Slot(id, ct), View, ct)) throw Denied();
        const int size = 40;
        var after = After(cursor);
        var rows = await Logs(id).Where(r => r.Entry.Id > after).OrderBy(r => r.Entry.Id).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(LogEntry).ToArray(), size, r => r.Id);
    }

    public async Task<CheckInLogDto> LogEntry(TaiKhoan actor, int id, int entry, CancellationToken ct)
    {
        if (!await Has(actor, await Slot(id, ct), View, ct)) throw Denied();
        return LogEntry(await Logs(id).SingleOrDefaultAsync(r => r.Entry.Id == entry, ct) ?? throw Missing());
    }

    private static CheckInLogDto LogEntry(LogRow r) => new(r.Entry.Id, r.Code, r.Operator,
        r.Entry.HinhThuc == HinhThucCheckIn.QuetQR ? "QR" : "Thủ công", Stamp(r.Entry.ThoiGianQuet),
        r.Entry.KetQua == "ThanhCong" ? "Thành công" : r.Entry.KetQua);
}
