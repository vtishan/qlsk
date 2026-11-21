using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Accounts;
using QLSK.Features.Events;
using QLSK.Features.Operations;
using QLSK.Features.Tickets;
using QRCoder;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Admissions;

public sealed record InvitationRequest(string? Name, string? Contact, int SessionId, int TicketTypeId, int SeatId, string? Note);
public sealed record InvitationDto(int Id, string Name, string SessionName, string? Seat, DateTimeOffset? CreatedAt,
    string Status, string StatusLabel, IReadOnlyList<string> AllowedActions, IReadOnlyList<DetailFieldDto>? Fields = null);
public sealed record InvitationOptionsDto(IReadOnlyList<ScopeOptionDto> InvitationType, IReadOnlyList<InvitationSeatDto> InvitationSeat);
public sealed record InvitationSeatDto(int Id, string Name, int TicketTypeId);
public sealed record InvitationTokenRequest(string? Token);
public sealed record InvitationLinkDto(string Token, DateTimeOffset? ExpiresAt, string Route);
public sealed record InvitationResolveDto(string EventName, string SessionName, string? Seat, string StatusLabel,
    IReadOnlyList<string> AllowedActions, string? TicketPath = null, string? QrImageUrl = null);

public sealed partial class AdmissionService
{
    internal static readonly string[] InvitationOperations = ["work.guests.list", "work.guests.detail", "work.guests.create",
        "work.guests.revoke", "work.guests.options", "work.guests.pdf", "work.guests.link"];

    public async Task<InvitationOptionsDto> InvitationOptions(TaiKhoan actor, int eventId, int sessionId, CancellationToken ct)
    {
        await events.NeedOwner(actor, eventId, ct);
        var slot = await Slot(sessionId, ct);
        if (slot.SuKienId != eventId) throw Missing();
        var types = await db.LoaiVe.AsNoTracking().Where(t => t.SuatDienId == sessionId && t.NhomVe == NhomVe.VeMoi)
            .OrderBy(t => t.Id).ToListAsync(ct);
        var zones = types.Select(t => t.KhuVucId).ToArray();
        var now = DateTime.UtcNow;
        var seats = await db.Ghe.AsNoTracking().Where(g => zones.Contains(g.KhuVucId) && g.TrangThai == TrangThaiGhe.SuDung && !g.CumGheId.HasValue &&
            !db.Ve.Any(v => v.SuatDienId == sessionId && v.GheId == g.Id &&
                (v.TrangThai == TrangThaiVe.DaPhatHanh || v.TrangThai == TrangThaiVe.DaCheckIn)) &&
            !db.GiuGhe.Any(h => h.SuatDienId == sessionId && h.GheId == g.Id && h.TrangThai == TrangThaiGiuGhe.DangGiu && h.HetHan > now))
            .OrderBy(g => g.MaGhe).Take(3000).ToListAsync(ct);
        return new(types.Select(t => new ScopeOptionDto(t.Id, t.TenLoaiVe)).ToArray(),
            types.SelectMany(t => seats.Where(s => s.KhuVucId == t.KhuVucId)
                .Select(s => new InvitationSeatDto(s.Id, s.MaGhe, t.Id))).ToArray());
    }

    private sealed class InvitationWorkRow
    {
        public required VeMoi Invitation { get; init; }
        public required Ve Ticket { get; init; }
        public required SuatDien Session { get; init; }
        public string? Seat { get; init; }
    }

    private IQueryable<InvitationWorkRow> InvitationRows(int eventId) =>
        from m in db.VeMoi.AsNoTracking() join v in db.Ve on m.VeId equals v.Id
        join s in db.SuatDien on v.SuatDienId equals s.Id where s.SuKienId == eventId
        select new InvitationWorkRow { Invitation = m, Ticket = v, Session = s,
            Seat = db.Ghe.Where(g => g.Id == v.GheId).Select(g => g.MaGhe).FirstOrDefault() };

    private static InvitationDto InvitationRow(InvitationWorkRow row)
    {
        var state = row.Ticket.TrangThai == TrangThaiVe.DaPhatHanh ? "ISSUED" :
            row.Ticket.TrangThai == TrangThaiVe.DaCheckIn ? "CHECKED_IN" : "REVOKED";
        return new(row.Invitation.Id, row.Invitation.HoTenKhachMoi, ScheduleService.NameOf(row.Session), row.Seat,
            Stamp(row.Ticket.NgayPhatHanh), state, state == "ISSUED" ? "Đã phát hành" : state == "CHECKED_IN" ? "Đã check-in" : "Đã thu hồi",
            state == "ISSUED" ? ["revoke", "pdf", "link"] : [],
            [new("Liên hệ", row.Invitation.Email ?? row.Invitation.SoDienThoai), new("Mã vé", row.Ticket.MaVe),
                new("Nhận vé", row.Invitation.TaiKhoanNhanId.HasValue ? "Đã liên kết tài khoản" : "Chưa liên kết tài khoản"),
                new("Gửi lời mời", row.Invitation.DaGuiLuc.HasValue ? "Đã ghi nhận gửi" : "Chưa có xác nhận gửi email; BTC tự gửi liên kết hoặc PDF")]);
    }

    public async Task<PageDto<InvitationDto>> Invitations(TaiKhoan actor, int eventId, string? status, string? cursor, CancellationToken ct)
    {
        await events.NeedOwner(actor, eventId, ct);
        var after = After(cursor);
        var query = InvitationRows(eventId).Where(m => m.Invitation.Id > after);
        if (status != null)
        {
            if (status is not ("ISSUED" or "CHECKED_IN" or "REVOKED")) throw Invalid("Trạng thái vé mời không hợp lệ.");
            query = status == "ISSUED" ? query.Where(m => m.Ticket.TrangThai == TrangThaiVe.DaPhatHanh) :
                status == "CHECKED_IN" ? query.Where(m => m.Ticket.TrangThai == TrangThaiVe.DaCheckIn) :
                query.Where(m => m.Ticket.TrangThai == TrangThaiVe.DaThuHoi || m.Ticket.TrangThai == TrangThaiVe.DaHuy);
        }
        var rows = await query.OrderBy(m => m.Invitation.Id).Take(31).ToListAsync(ct);
        return Page(rows.Select(InvitationRow).ToArray(), 30, m => m.Id);
    }

    public async Task<InvitationDto> Invitation(TaiKhoan actor, int eventId, int id, CancellationToken ct)
    {
        await events.NeedOwner(actor, eventId, ct);
        return InvitationRow(await InvitationRows(eventId).SingleOrDefaultAsync(m => m.Invitation.Id == id, ct) ?? throw Missing());
    }

    public async Task<int> IssueInvitation(TaiKhoan actor, int eventId, InvitationRequest request, CancellationToken ct)
    {
        await events.NeedOwner(actor, eventId, ct);
        var name = Required(request.Name, 120, "tên khách mời");
        var email = AccountService.Email(request.Contact, false);
        var phone = email == null ? AccountService.Phone(request.Contact, false) : null;
        if (email == null && phone == null) throw Invalid("Nhập email hoặc số điện thoại hợp lệ của khách mời.");
        var note = Optional(request.Note, 1000, "ghi chú");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{eventId}", ct);
        await SqlOperations.Lock(db, $"qlsk:session:{request.SessionId}", ct);
        var slot = await Slot(request.SessionId, ct);
        if (slot.SuKienId != eventId) throw Missing();
        var evt = await db.SuKien.AsNoTracking().SingleAsync(e => e.Id == eventId, ct);
        var now = DateTime.UtcNow;
        if (evt.TrangThai is not (TrangThaiSuKien.DaDuyet or TrangThaiSuKien.DangMoBan) ||
            slot.TrangThai is not (TrangThaiSuatDien.DaXacNhan or TrangThaiSuatDien.DangMoBan or TrangThaiSuatDien.DongBan) ||
            slot.ThoiGianKetThuc <= now) throw Conflict("Chỉ phát hành vé mời cho suất đã công bố và chưa kết thúc.");
        var type = await db.LoaiVe.AsNoTracking().SingleOrDefaultAsync(t => t.Id == request.TicketTypeId &&
            t.SuatDienId == slot.Id && t.NhomVe == NhomVe.VeMoi && t.GiaVe == 0, ct) ?? throw Invalid("Chọn loại vé mời của suất.");
        var seat = await db.Ghe.AsNoTracking().SingleOrDefaultAsync(g => g.Id == request.SeatId && g.KhuVucId == type.KhuVucId &&
            g.TrangThai == TrangThaiGhe.SuDung && !g.CumGheId.HasValue, ct) ?? throw Invalid("Chọn ghế lẻ khả dụng của loại vé mời.");
        var zone = await db.KhuVuc.AsNoTracking().SingleOrDefaultAsync(z => z.Id == seat.KhuVucId && z.DiaDiemId == slot.DiaDiemId, ct)
            ?? throw Invalid("Khu vực không thuộc địa điểm của suất.");
        var sold = db.Ve.Where(v => v.SuatDienId == slot.Id && (v.TrangThai == TrangThaiVe.DaPhatHanh || v.TrangThai == TrangThaiVe.DaCheckIn));
        var held = db.GiuGhe.Where(h => h.SuatDienId == slot.Id && h.TrangThai == TrangThaiGiuGhe.DangGiu && h.HetHan > now);
        if (await sold.AnyAsync(v => v.GheId == seat.Id, ct) || await held.AnyAsync(h => h.GheId == seat.Id, ct))
            throw Conflict("Ghế đã được giữ hoặc phát hành. Chọn lại ghế.");
        var heldTypes = from h in held join l in db.ChiTietDonHang on h.ChiTietDonHangId equals l.Id select l.LoaiVeId;
        if (await sold.CountAsync(ct) + await held.CountAsync(ct) >= slot.SucChua ||
            await sold.CountAsync(v => v.LoaiVeId == type.Id, ct) + await heldTypes.CountAsync(t => t == type.Id, ct) >= type.SoLuong ||
            await sold.CountAsync(v => db.LoaiVe.Any(t => t.Id == v.LoaiVeId && t.KhuVucId == zone.Id), ct) +
            await heldTypes.CountAsync(t => db.LoaiVe.Any(v => v.Id == t && v.KhuVucId == zone.Id), ct) >= zone.SucChua)
            throw Conflict("Suất, khu vực hoặc loại vé đã hết hạn mức.");
        var receiver = await db.TaiKhoan.AsNoTracking().Where(a => a.TrangThai != TrangThaiTaiKhoan.DaKhoa &&
            ((email != null && a.Email == email && a.EmailXacMinhLuc != null) ||
             (phone != null && a.SoDienThoai == phone && a.SoDienThoaiXacMinhLuc != null))).Select(a => (int?)a.Id).FirstOrDefaultAsync(ct);
        var ticket = new Ve { LoaiVeId = type.Id, SuatDienId = slot.Id, GheId = seat.Id, NgayPhatHanh = now,
            TrangThai = TrangThaiVe.DaPhatHanh, MaVe = "QLSK-" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant(),
            MaQR = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)) };
        db.Ve.Add(ticket);
        await db.SaveChangesAsync(ct);
        var invitation = new VeMoi { VeId = ticket.Id, HoTenKhachMoi = name, Email = email, SoDienThoai = phone,
            TaiKhoanNhanId = receiver, TrangThai = receiver.HasValue ? TrangThaiVeMoi.DaNhan : TrangThaiVeMoi.ChoGui,
            DaNhanLuc = receiver.HasValue ? now : null };
        db.VeMoi.Add(invitation);
        await db.SaveChangesAsync(ct);
        SqlOperations.Audit(db, actor, "Phát hành vé mời", nameof(VeMoi), invitation.Id, note);
        if (receiver.HasValue) SqlOperations.Notify(db, receiver.Value, "Bạn có vé mời", "Mở Vé của tôi để xem vé " + ticket.MaVe + ".", "ticket");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return invitation.Id;
    }

    public async Task RevokeInvitation(TaiKhoan actor, int id, string? reason, CancellationToken ct)
    {
        var context = await (from m in db.VeMoi.AsNoTracking() join v in db.Ve on m.VeId equals v.Id
            join s in db.SuatDien on v.SuatDienId equals s.Id where m.Id == id select new { s.SuKienId, s.Id }).SingleOrDefaultAsync(ct) ?? throw Missing();
        await events.NeedOwner(actor, context.SuKienId, ct);
        var note = Required(reason, 1000, "lý do thu hồi");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{context.SuKienId}", ct);
        await SqlOperations.Lock(db, $"qlsk:session:{context.Id}", ct);
        var invitation = await db.VeMoi.SingleAsync(m => m.Id == id, ct);
        var ticket = await db.Ve.SingleAsync(v => v.Id == invitation.VeId, ct);
        if (ticket.TrangThai == TrangThaiVe.DaCheckIn) throw Conflict("Vé đã check-in, không thể thu hồi.");
        if (ticket.TrangThai == TrangThaiVe.DaThuHoi) return;
        ticket.TrangThai = TrangThaiVe.DaThuHoi;
        invitation.TrangThai = TrangThaiVeMoi.DaThuHoi;
        invitation.MaBamNhanVe = null;
        invitation.HanNhanVe = null;
        SqlOperations.Audit(db, actor, "Thu hồi vé mời", nameof(VeMoi), id, note);
        if (invitation.TaiKhoanNhanId.HasValue) SqlOperations.Notify(db, invitation.TaiKhoanNhanId.Value, "Vé mời đã thu hồi", note, "ticket");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<TicketFileDto> InvitationFile(TaiKhoan actor, int eventId, int id, CancellationToken ct)
    {
        await events.NeedOwner(actor, eventId, ct);
        var ticket = await (from m in db.VeMoi join v in db.Ve on m.VeId equals v.Id join s in db.SuatDien on v.SuatDienId equals s.Id
            where m.Id == id && s.SuKienId == eventId select v).SingleOrDefaultAsync(ct) ?? throw Missing();
        if (ticket.TrangThai != TrangThaiVe.DaPhatHanh) throw Conflict("Vé mời không còn khả dụng để gửi.");
        using var data = QRCodeGenerator.GenerateQrCode(ticket.MaQR, QRCodeGenerator.ECCLevel.M);
        using var renderer = new PdfByteQRCode(data);
        return new(renderer.GetGraphic(16), "application/pdf", $"qlsk-ve-moi-{id}.pdf");
    }

    private static string TokenHash(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

    private static bool ContactMatches(TaiKhoan actor, VeMoi invitation) =>
        (actor.EmailXacMinhLuc.HasValue && invitation.Email != null && actor.Email == invitation.Email) ||
        (actor.SoDienThoaiXacMinhLuc.HasValue && invitation.SoDienThoai != null && actor.SoDienThoai == invitation.SoDienThoai);

    private async Task<InvitationWorkRow> TokenInvitation(string? raw, CancellationToken ct)
    {
        var token = Required(raw, 128, "mã nhận vé mời");
        if (token.Length != 64 || !token.All(Uri.IsHexDigit)) throw Missing();
        var hash = TokenHash(token);
        var eventId = await (from m in db.VeMoi.AsNoTracking() join v in db.Ve on m.VeId equals v.Id
            join s in db.SuatDien on v.SuatDienId equals s.Id where m.MaBamNhanVe == hash select (int?)s.SuKienId)
            .SingleOrDefaultAsync(ct) ?? throw Missing();
        return await InvitationRows(eventId).SingleOrDefaultAsync(r => r.Invitation.MaBamNhanVe == hash, ct) ?? throw Missing();
    }

    private async Task NeedLiveInvitation(InvitationWorkRow row, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        if (row.Ticket.TrangThai != TrangThaiVe.DaPhatHanh || row.Invitation.TrangThai is TrangThaiVeMoi.DaThuHoi or TrangThaiVeMoi.HetHan ||
            !row.Invitation.HanNhanVe.HasValue || row.Invitation.HanNhanVe <= now || row.Session.ThoiGianKetThuc <= now)
            throw Conflict("Lời mời đã hết hạn, được sử dụng tại cổng hoặc đã thu hồi. Liên hệ Ban tổ chức.");
        if (!await db.SuKien.AnyAsync(e => e.Id == row.Session.SuKienId &&
            (e.TrangThai == TrangThaiSuKien.DaDuyet || e.TrangThai == TrangThaiSuKien.DangMoBan), ct) ||
            row.Session.TrangThai is not (TrangThaiSuatDien.DaXacNhan or TrangThaiSuatDien.DangMoBan or TrangThaiSuatDien.DongBan))
            throw Conflict("Sự kiện hoặc suất diễn hiện không cho phép nhận vé.");
    }

    public async Task<InvitationLinkDto> InvitationLink(TaiKhoan actor, int eventId, int id, CancellationToken ct)
    {
        await events.NeedOwner(actor, eventId, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{eventId}", ct);
        var context = await InvitationRows(eventId).SingleOrDefaultAsync(r => r.Invitation.Id == id, ct) ?? throw Missing();
        await SqlOperations.Lock(db, $"qlsk:session:{context.Session.Id}", ct);
        if (context.Ticket.TrangThai != TrangThaiVe.DaPhatHanh || context.Session.ThoiGianKetThuc <= DateTime.UtcNow)
            throw Conflict("Vé mời không còn khả dụng để tạo liên kết.");
        context.Invitation.HanNhanVe = context.Session.ThoiGianKetThuc;
        if (context.Invitation.TrangThai == TrangThaiVeMoi.HetHan) context.Invitation.TrangThai = TrangThaiVeMoi.ChoGui;
        await NeedLiveInvitation(context, ct);
        var invitation = await db.VeMoi.SingleAsync(m => m.Id == id, ct);
        if (invitation.TrangThai == TrangThaiVeMoi.HetHan)
            invitation.TrangThai = invitation.TaiKhoanNhanId.HasValue ? TrangThaiVeMoi.DaNhan : TrangThaiVeMoi.ChoGui;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        invitation.MaBamNhanVe = TokenHash(token);
        invitation.HanNhanVe = new[] { DateTime.UtcNow.AddDays(7), context.Session.ThoiGianKetThuc }.Min();
        // Creating a link does not prove an email has been sent.
        SqlOperations.Audit(db, actor, "Tạo lại liên kết nhận vé mời", nameof(VeMoi), id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new(token, Stamp(invitation.HanNhanVe), "tickets/invitations?token=" + token);
    }

    public async Task<InvitationResolveDto> ResolveInvitation(TaiKhoan actor, InvitationTokenRequest request, CancellationToken ct)
    {
        var row = await TokenInvitation(request.Token, ct);
        await NeedLiveInvitation(row, ct);
        var owns = row.Invitation.TaiKhoanNhanId == actor.Id;
        if (row.Invitation.TaiKhoanNhanId.HasValue && !owns) throw Denied();
        var name = await db.SuKien.Where(e => e.Id == row.Session.SuKienId).Select(e => e.TenSuKien).SingleAsync(ct);
        return new(name, ScheduleService.NameOf(row.Session), row.Seat, owns ? "Đã liên kết tài khoản" : "Vé mời còn hiệu lực",
            !row.Invitation.TaiKhoanNhanId.HasValue && ContactMatches(actor, row.Invitation) ? ["claim"] : [],
            owns ? "tickets/mine/" + row.Ticket.Id : null, owns ? $"/api/tickets/{row.Ticket.Id}/qr" : null);
    }

    public async Task<CreatedDto> ClaimInvitation(TaiKhoan actor, InvitationTokenRequest request, CancellationToken ct)
    {
        var first = await TokenInvitation(request.Token, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{first.Session.SuKienId}", ct);
        await SqlOperations.Lock(db, $"qlsk:session:{first.Session.Id}", ct);
        await SqlOperations.Lock(db, $"qlsk:account:{actor.Id}", ct);
        // Regeneration, revocation and check-in share these locks. Re-read the token under lock.
        var row = await TokenInvitation(request.Token, ct);
        await NeedLiveInvitation(row, ct);
        if (row.Invitation.TaiKhoanNhanId.HasValue && row.Invitation.TaiKhoanNhanId != actor.Id) throw Denied();
        var freshActor = await db.TaiKhoan.AsNoTracking().SingleAsync(a => a.Id == actor.Id, ct);
        if (freshActor.TrangThai == TrangThaiTaiKhoan.DaKhoa || !ContactMatches(freshActor, row.Invitation)) throw Denied();
        var invitation = await db.VeMoi.SingleAsync(m => m.Id == row.Invitation.Id, ct);
        invitation.TaiKhoanNhanId = actor.Id;
        invitation.DaNhanLuc = DateTime.UtcNow;
        invitation.TrangThai = TrangThaiVeMoi.DaNhan;
        invitation.MaBamNhanVe = null;
        invitation.HanNhanVe = null;
        SqlOperations.Audit(db, actor, "Nhận vé mời", nameof(VeMoi), invitation.Id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new(row.Ticket.Id);
    }
}
