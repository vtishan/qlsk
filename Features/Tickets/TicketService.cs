using Microsoft.EntityFrameworkCore;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Events;
using QRCoder;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Tickets;

public sealed class TicketService(AppDbContext db)
{
    internal static readonly string[] UserOperations = ["tickets.mine", "tickets.detail", "tickets.pdf"];
    internal IQueryable<Ve> Owned(int accountId) => db.Ve.AsNoTracking().Where(v =>
        db.VeMoi.Any(m => m.VeId == v.Id && m.TaiKhoanNhanId == accountId) ||
        db.ChiTietDonHang.Any(l => l.Id == v.ChiTietDonHangId && db.DonHang.Any(o => o.Id == l.DonHangId && o.TaiKhoanId == accountId)));

    private sealed class Row
    {
        public required Ve Ticket { get; init; }
        public required SuatDien Session { get; init; }
        public required string EventName { get; init; }
        public required string Venue { get; init; }
        public required string Type { get; init; }
        public required string Zone { get; init; }
        public bool AllowCancellation { get; init; }
        public string? Seat { get; init; }
    }

    private IQueryable<Row> WithDetails(int accountId) =>
        from ticket in Owned(accountId)
        join slot in db.SuatDien on ticket.SuatDienId equals slot.Id
        join evt in db.SuKien on slot.SuKienId equals evt.Id
        join venue in db.DiaDiem on slot.DiaDiemId equals venue.Id
        join type in db.LoaiVe on ticket.LoaiVeId equals type.Id
        join zone in db.KhuVuc on type.KhuVucId equals zone.Id
        select new Row { Ticket = ticket, Session = slot, EventName = evt.TenSuKien, Venue = venue.TenDiaDiem,
            Type = type.TenLoaiVe, Zone = zone.TenKhuVuc, AllowCancellation = evt.ChoPhepTuHuyVe,
            Seat = db.Ghe.Where(g => g.Id == ticket.GheId).Select(g => g.MaGhe).FirstOrDefault() };

    public async Task<PageDto<TicketDto>> Mine(TaiKhoan actor, string? cursor, CancellationToken ct)
    {
        var after = After(cursor);
        const int size = 40;
        var rows = await WithDetails(actor.Id).Where(r => r.Ticket.Id > after).OrderBy(r => r.Ticket.Id).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(row => Map(row, false)).ToArray(), size, r => r.Id);
    }

    public async Task<TicketDto> Detail(TaiKhoan actor, int id, CancellationToken ct)
    {
        var row = await WithDetails(actor.Id).SingleOrDefaultAsync(r => r.Ticket.Id == id, ct) ?? throw Missing();
        return Map(row, true);
    }

    public async Task<TicketFileDto> File(TaiKhoan actor, int id, bool pdf, CancellationToken ct)
    {
        var ticket = await Owned(actor.Id).SingleOrDefaultAsync(v => v.Id == id, ct) ?? throw Missing();
        if (ticket.TrangThai != TrangThaiVe.DaPhatHanh) throw Conflict("Vé không còn khả dụng để xuất trình.");
        using var data = QRCodeGenerator.GenerateQrCode(ticket.MaQR, QRCodeGenerator.ECCLevel.M);
        if (pdf)
        {
            using var renderer = new PdfByteQRCode(data);
            return new(renderer.GetGraphic(16), "application/pdf", $"qlsk-ve-{id}.pdf");
        }
        using var png = new PngByteQRCode(data);
        return new(png.GetGraphic(8), "image/png", null);
    }

    private static TicketDto Map(Row row, bool detail)
    {
        var phase = row.Ticket.TrangThai switch {
            TrangThaiVe.DaPhatHanh => ("ISSUED", "Đã phát hành"), TrangThaiVe.DaCheckIn => ("CHECKED_IN", "Đã check-in"),
            TrangThaiVe.DaThuHoi => ("REVOKED", "Đã thu hồi"), _ => ("CANCELLED", "Đã hủy")
        };
        var ready = detail && row.Ticket.TrangThai == TrangThaiVe.DaPhatHanh;
        var actions = new List<string>();
        if (ready) actions.Add("pdf");
        if (ready && row.AllowCancellation && row.Session.ThoiGianBatDau >= DateTime.UtcNow.AddHours(72)) actions.Add("refund");
        return new(row.Ticket.Id, row.EventName + " · " + row.Ticket.MaVe, row.EventName, ScheduleService.NameOf(row.Session),
            row.Venue, row.Type, row.Zone, row.Seat, phase.Item1, phase.Item2,
            ready ? $"/api/tickets/{row.Ticket.Id}/qr" : null, actions, detail ? row.Ticket.MaVe : null);
    }
}
