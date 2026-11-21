using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Catalog;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Events;

public sealed class ScheduleService(AppDbContext db, EventService events)
{
    internal static int? VersionOf(SuatDien slot) =>
        int.TryParse(CatalogService.JsonText(slot.SoDoChot).GetValueOrDefault("versionId"), out var id) ? id : null;
    internal static string NameOf(SuatDien slot) =>
        CatalogService.JsonText(slot.SoDoChot).GetValueOrDefault("name") ?? $"Suất {slot.Id}";
    internal static int? RentalOf(SuatDien slot)
    {
        if (string.IsNullOrWhiteSpace(slot.SoDoChot)) return null;
        using var snapshot = JsonDocument.Parse(slot.SoDoChot);
        if (snapshot.RootElement.ValueKind != JsonValueKind.Object ||
            !snapshot.RootElement.TryGetProperty("rentalId", out var rental)) return null;
        if (rental.ValueKind == JsonValueKind.Number && rental.TryGetInt32(out var id)) return id;
        return rental.ValueKind == JsonValueKind.String && int.TryParse(rental.GetString(), out id) ? id : null;
    }

    internal static async Task<YeuCauThue?> ValidateRental(AppDbContext db, int organizerAccountId, int? rentalId,
        int venueId, DateTime from, DateTime to, CancellationToken ct)
    {
        if (!rentalId.HasValue) return null;
        var rental = await db.YeuCauThue.AsNoTracking().SingleOrDefaultAsync(r => r.Id == rentalId.Value &&
            r.TaiKhoanId == organizerAccountId && r.TrangThai == TrangThaiYeuCauThue.DaXacNhan &&
            r.DiaDiemId == venueId && r.ThoiGianBatDau <= from && r.ThoiGianKetThuc >= to, ct);
        return rental ?? throw Invalid("Chọn hồ sơ thuê đã xác nhận của Ban tổ chức, đúng địa điểm và bao trọn giờ diễn.");
    }

    public async Task<PageDto<ConfirmedRentalOptionDto>> ConfirmedRentals(TaiKhoan actor, int eventId, CancellationToken ct)
    {
        await events.NeedOwner(actor, eventId, ct);
        var now = DateTime.UtcNow;
        var rows = await (from r in db.YeuCauThue.AsNoTracking()
                          join v in db.DiaDiem on r.DiaDiemId equals v.Id
                          where r.TaiKhoanId == actor.Id && r.TrangThai == TrangThaiYeuCauThue.DaXacNhan &&
                              r.ThoiGianKetThuc > now && v.TrangThai == TrangThaiDiaDiem.HoatDong
                          orderby r.ThoiGianBatDau, r.Id
                          select new { r.Id, r.DiaDiemId, v.TenDiaDiem, r.ThoiGianBatDau, r.ThoiGianKetThuc,
                              r.TongTien, r.TienCoc }).ToListAsync(ct);
        // The requested headcount only exists inside the rental's JSON form snapshot;
        // leave it unspecified rather than confusing it with the venue's capacity.
        return new(rows.Select(r => new ConfirmedRentalOptionDto(r.Id, r.DiaDiemId, r.TenDiaDiem,
            Stamp(r.ThoiGianBatDau), Stamp(r.ThoiGianKetThuc), null, r.TongTien, r.TienCoc)).ToArray());
    }

    public async Task<PageDto<SessionWorkDto>> Sessions(TaiKhoan actor, int id, string? cursor, CancellationToken ct)
    {
        await events.NeedRead(actor, id, ct);
        var after = After(cursor);
        var owner = await events.Owns(actor, id, ct);
        const int size = 30;
        var rows = await (from s in db.SuatDien.AsNoTracking() join v in db.DiaDiem on s.DiaDiemId equals v.Id
                          where s.SuKienId == id && s.Id > after orderby s.Id
                          select new { Session = s, v.TenDiaDiem }).Take(size + 1).ToListAsync(ct);
        var draftVersions = owner ? await db.PhienBanSuKien.AsNoTracking()
            .Where(v => v.SuKienId == id && v.TrangThaiDuyet == TrangThaiDuyetPhienBan.Nhap)
            .Select(v => v.Id).ToListAsync(ct) : [];
        return Page(rows.Select(s => SessionRow(s.Session, s.TenDiaDiem,
            VersionOf(s.Session) is { } version && draftVersions.Contains(version))).ToArray(), size, r => r.Id);
    }

    public async Task<SessionWorkDto> Session(TaiKhoan actor, int id, int sessionId, CancellationToken ct)
    {
        await events.NeedRead(actor, id, ct);
        var slot = await (from s in db.SuatDien.AsNoTracking() join v in db.DiaDiem on s.DiaDiemId equals v.Id
                          where s.Id == sessionId && s.SuKienId == id
                          select new { Session = s, v.TenDiaDiem }).SingleOrDefaultAsync(ct) ?? throw Missing();
        return SessionRow(slot.Session, slot.TenDiaDiem, await CanEdit(actor, slot.Session, ct));
    }

    private async Task<bool> CanEdit(TaiKhoan actor, SuatDien slot, CancellationToken ct) =>
        slot.TrangThai == TrangThaiSuatDien.Nhap && VersionOf(slot) is { } version &&
        await events.Owns(actor, slot.SuKienId, ct) && await db.PhienBanSuKien.AnyAsync(v =>
            v.Id == version && v.SuKienId == slot.SuKienId && v.TrangThaiDuyet == TrangThaiDuyetPhienBan.Nhap, ct);

    private static SessionWorkDto SessionRow(SuatDien slot, string venue, bool editable) =>
        new(slot.Id, NameOf(slot), venue, Stamp(slot.ThoiGianBatDau), Stamp(slot.ThoiGianKetThuc), slot.SucChua,
            slot.TrangThai.ToString(), editable && slot.TrangThai == TrangThaiSuatDien.Nhap ? ["updateDraft"] : [],
            [new("Mở bán (giờ Việt Nam)", InVietnam(slot.ThoiGianMoBan)), new("Đóng bán (giờ Việt Nam)", InVietnam(slot.ThoiGianDongBan))],
            new Dictionary<string, object?> { ["versionId"] = VersionOf(slot), ["venueId"] = slot.DiaDiemId,
                ["from"] = Stamp(slot.ThoiGianBatDau), ["to"] = Stamp(slot.ThoiGianKetThuc), ["rentalId"] = RentalOf(slot) });

    public async Task<int> ChangeSession(TaiKhoan actor, int id, int? sessionId, SessionChangeRequest request, CancellationToken ct)
    {
        await events.NeedOwner(actor, id, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        await events.Draft(id, request.VersionId, ct);
        var slot = sessionId.HasValue
            ? await db.SuatDien.SingleOrDefaultAsync(s => s.Id == sessionId.Value && s.SuKienId == id, ct) ?? throw Missing()
            : new SuatDien { SuKienId = id, TrangThai = TrangThaiSuatDien.Nhap };
        if (sessionId.HasValue && (slot.TrangThai != TrangThaiSuatDien.Nhap || VersionOf(slot) != request.VersionId))
            throw Conflict("Suất đã công khai hoặc không thuộc bản nháp này.");
        var location = await db.DiaDiem.AsNoTracking().SingleOrDefaultAsync(v => v.Id == request.VenueId &&
            v.TrangThai == TrangThaiDiaDiem.HoatDong, ct) ?? throw Invalid("Chọn địa điểm đang hoạt động.");
        if (request.ZoneId.HasValue && !await db.KhuVuc.AnyAsync(z => z.Id == request.ZoneId.Value && z.DiaDiemId == location.Id, ct))
            throw Invalid("Khu vực không thuộc địa điểm đã chọn.");
        var capacity = request.Capacity ?? (sessionId.HasValue ? slot.SucChua : 0);
        var start = request.From?.UtcDateTime ?? throw Invalid("Chọn giờ bắt đầu.");
        var end = request.To?.UtcDateTime ?? throw Invalid("Chọn giờ kết thúc.");
        var saleStart = request.SalesFrom?.UtcDateTime ?? (sessionId.HasValue ? slot.ThoiGianMoBan : DateTime.UtcNow);
        var saleEnd = request.SalesTo?.UtcDateTime ?? (sessionId.HasValue ? slot.ThoiGianDongBan : start);
        if (start <= DateTime.UtcNow || end <= start || saleStart >= saleEnd || saleEnd > start ||
            capacity < 1 || capacity > location.SucChua)
            throw Invalid("Kiểm tra ngày giờ, thời gian bán và sức chứa của suất.");
        var rentalId = request.RentalId == 0 ? null : request.RentalId ?? RentalOf(slot);
        await ValidateRental(db, actor.Id, rentalId, location.Id, start, end, ct);
        if (sessionId.HasValue && await db.LoaiVe.AnyAsync(t => t.SuatDienId == slot.Id, ct) &&
            (slot.DiaDiemId != location.Id || saleStart > slot.ThoiGianMoBan || saleEnd < slot.ThoiGianDongBan ||
             capacity < (await db.LoaiVe.Where(t => t.SuatDienId == slot.Id).SumAsync(t => (int?)t.SoLuong, ct) ?? 0)))
            throw Conflict("Suất đã khai loại vé. Kiểm tra lại địa điểm, thời gian bán hoặc giới hạn vé trước khi sửa.");
        var name = string.IsNullOrWhiteSpace(request.Name) && sessionId.HasValue ? NameOf(slot) : Required(request.Name, 180, "tên suất");
        slot.DiaDiemId = location.Id;
        slot.ThoiGianBatDau = start;
        slot.ThoiGianKetThuc = end;
        slot.ThoiGianMoBan = saleStart;
        slot.ThoiGianDongBan = saleEnd;
        slot.SucChua = capacity;
        slot.SoDoChot = JsonSerializer.Serialize(new { name, versionId = request.VersionId.ToString(CultureInfo.InvariantCulture),
            request.ZoneId, rentalId });
        if (!sessionId.HasValue) db.SuatDien.Add(slot);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return slot.Id;
    }

    public async Task<PageDto<TicketTypeWorkDto>> Types(TaiKhoan actor, int sessionId, string? cursor, CancellationToken ct)
    {
        var slot = await FindSlot(sessionId, ct);
        await events.NeedRead(actor, slot.SuKienId, ct);
        var after = After(cursor);
        var editable = await CanEdit(actor, slot, ct);
        const int size = 30;
        var rows = await (from t in db.LoaiVe.AsNoTracking() join z in db.KhuVuc on t.KhuVucId equals z.Id
                          where t.SuatDienId == sessionId && t.Id > after orderby t.Id
                          select new { Ticket = t, z.TenKhuVuc }).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(t => TypeRow(t.Ticket, t.TenKhuVuc, editable)).ToArray(), size, r => r.Id);
    }

    public async Task<TicketTypeWorkDto> Type(TaiKhoan actor, int sessionId, int id, CancellationToken ct)
    {
        var slot = await FindSlot(sessionId, ct);
        await events.NeedRead(actor, slot.SuKienId, ct);
        var row = await (from t in db.LoaiVe.AsNoTracking() join z in db.KhuVuc on t.KhuVucId equals z.Id
                         where t.SuatDienId == sessionId && t.Id == id
                         select new { Ticket = t, z.TenKhuVuc }).SingleOrDefaultAsync(ct) ?? throw Missing();
        return TypeRow(row.Ticket, row.TenKhuVuc, await CanEdit(actor, slot, ct));
    }

    private static TicketTypeWorkDto TypeRow(LoaiVe t, string zone, bool edit) =>
        new(t.Id, t.TenLoaiVe, zone, t.GiaVe, Stamp(t.BatDauBan), Stamp(t.KetThucBan), edit ? ["updateDraft"] : [],
            [new("Số vé", t.SoLuong.ToString()), new("Quyền lợi", t.QuyenLoi)]);

    private async Task<SuatDien> FindSlot(int id, CancellationToken ct) =>
        await db.SuatDien.AsNoTracking().SingleOrDefaultAsync(s => s.Id == id, ct) ?? throw Missing();

    public async Task<int> ChangeType(TaiKhoan actor, int sessionId, int? id, TicketTypeChangeRequest request, CancellationToken ct)
    {
        var source = await FindSlot(sessionId, ct);
        await events.NeedOwner(actor, source.SuKienId, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{source.SuKienId}", ct);
        var slot = await FindSlot(sessionId, ct);
        await events.Draft(slot.SuKienId, request.VersionId, ct);
        if (slot.TrangThai != TrangThaiSuatDien.Nhap || VersionOf(slot) != request.VersionId)
            throw Conflict("Chỉ sửa loại vé trong suất thuộc phiên bản nháp.");
        var ticket = id.HasValue ? await db.LoaiVe.SingleOrDefaultAsync(t => t.Id == id && t.SuatDienId == sessionId, ct) ?? throw Missing()
            : new LoaiVe { SuatDienId = sessionId, KhuVucId = request.ZoneId };
        var zone = await db.KhuVuc.AsNoTracking().SingleOrDefaultAsync(z => z.Id == ticket.KhuVucId && z.DiaDiemId == slot.DiaDiemId, ct)
            ?? throw Invalid("Khu vực không thuộc địa điểm của suất.");
        var plan = id.HasValue ? ticket.NhomVe : request.Type switch {
            "Standard" => NhomVe.TieuChuan, "VIP" => NhomVe.VIP, "Invitation" => NhomVe.VeMoi,
            _ => throw Invalid("Chọn loại vé.")
        };
        if (!id.HasValue && await db.LoaiVe.AnyAsync(t => t.SuatDienId == sessionId && t.KhuVucId == zone.Id &&
            t.NhomVe == plan, ct)) throw Conflict("Khu vực đã có hạng vé này.");
        var quantity = request.Quantity ?? ticket.SoLuong;
        var price = request.Price ?? throw Invalid("Nhập giá vé.");
        var start = request.SalesFrom?.UtcDateTime ?? (id.HasValue ? ticket.BatDauBan : slot.ThoiGianMoBan);
        var end = request.SalesTo?.UtcDateTime ?? (id.HasValue ? ticket.KetThucBan : slot.ThoiGianDongBan);
        var other = db.LoaiVe.Where(t => t.SuatDienId == sessionId && t.Id != id);
        if (quantity < 1 || price < 0 || decimal.Truncate(price) != price || (plan == NhomVe.VeMoi && price != 0) ||
            start >= end || start < slot.ThoiGianMoBan || end > slot.ThoiGianDongBan ||
            quantity + (await other.SumAsync(t => (int?)t.SoLuong, ct) ?? 0) > slot.SucChua ||
            quantity + (await other.Where(t => t.KhuVucId == zone.Id).SumAsync(t => (int?)t.SoLuong, ct) ?? 0) > zone.SucChua)
            throw Invalid("Kiểm tra giá, số vé, sức chứa và khoảng mở bán.");
        ticket.NhomVe = plan;
        ticket.TenLoaiVe = plan == NhomVe.TieuChuan ? "Standard" : plan == NhomVe.VIP ? "VIP" : "Vé mời";
        ticket.SoLuong = quantity;
        ticket.GiaVe = price;
        ticket.BatDauBan = start;
        ticket.KetThucBan = end;
        if (!id.HasValue) { ticket.QuyenLoi = Optional(request.Benefits, 1500, "quyền lợi"); db.LoaiVe.Add(ticket); }
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return ticket.Id;
    }

    private static string InVietnam(DateTime date) => date.AddHours(7).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
}
