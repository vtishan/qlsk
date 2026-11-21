using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Catalog;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Events;

[ApiController]
[Authorize]
[Route("api/manage/events/{id:int}")]
public sealed class EventPublicationController(AppDbContext db, EventService events, ScheduleService schedule) : ControllerBase
{
    private TaiKhoan Actor => AccountAccess.Actor(HttpContext);

    [HttpGet("confirmed-rentals")]
    public Task<PageDto<ConfirmedRentalOptionDto>> ConfirmedRentals(int id, CancellationToken ct) =>
        schedule.ConfirmedRentals(Actor, id, ct);

    [HttpGet("publication")]
    public async Task<EventPublicationPreviewDto> Preview(int id, CancellationToken ct)
    {
        var actor = Actor;
        if (actor.VaiTro != VaiTroTaiKhoan.BanToChuc) throw Denied();
        await events.NeedOwner(actor, id, ct);
        var version = await events.InitialPublicationVersion(id, ct) ??
            throw ApiRequest.Conflict("Chỉ xem trước khi sự kiện đã duyệt và chưa công khai.");
        var content = EventService.Content(version);
        var category = await db.DanhMucSuKien.Where(c => c.Id == content.CategoryId)
            .Select(c => c.TenDanhMuc).SingleOrDefaultAsync(ct);
        var allSlots = await (from slot in db.SuatDien.AsNoTracking()
                              join venue in db.DiaDiem on slot.DiaDiemId equals venue.Id
                              where slot.SuKienId == id && slot.TrangThai == TrangThaiSuatDien.Nhap
                              orderby slot.ThoiGianBatDau, slot.Id
                              select new { Slot = slot, Venue = venue.TenDiaDiem }).ToListAsync(ct);
        var slots = allSlots.Where(s => ScheduleService.VersionOf(s.Slot) == version.Id).ToArray();
        var sessionIds = slots.Select(s => s.Slot.Id).ToArray();
        var prices = (await (from ticket in db.LoaiVe.AsNoTracking()
                             join zone in db.KhuVuc on ticket.KhuVucId equals zone.Id
                             where sessionIds.Contains(ticket.SuatDienId)
                             orderby ticket.GiaVe, ticket.Id
                             select new { Ticket = ticket, Zone = zone.TenKhuVuc }).ToListAsync(ct))
            .ToLookup(r => r.Ticket.SuatDienId);
        var rentalIds = slots.Select(s => ScheduleService.RentalOf(s.Slot)).OfType<int>().Distinct().ToArray();
        var rentals = await db.YeuCauThue.AsNoTracking().Where(r => rentalIds.Contains(r.Id) && r.TaiKhoanId == actor.Id)
            .ToDictionaryAsync(r => r.Id, ct);
        return new(version.Id, content.Name, category, content.Description, CatalogService.SafeImage(content.Image),
            content.Program, content.Participants, content.Rules, content.Policy, content.AllowRefund, slots.Select(row =>
            {
                var slot = row.Slot;
                var rentalId = ScheduleService.RentalOf(slot);
                var rental = rentalId.HasValue ? rentals.GetValueOrDefault(rentalId.Value) : null;
                return new PublicationSessionDto(ScheduleService.NameOf(slot), row.Venue, Stamp(slot.ThoiGianBatDau),
                    Stamp(slot.ThoiGianKetThuc), slot.SucChua, rentalId, rental?.TongTien, rental?.TienCoc,
                    prices[slot.Id].Select(r => new PublicationPriceDto(r.Ticket.TenLoaiVe, r.Zone, r.Ticket.GiaVe,
                        r.Ticket.SoLuong, Stamp(r.Ticket.BatDauBan), Stamp(r.Ticket.KetThucBan), r.Ticket.QuyenLoi,
                        r.Ticket.NhomVe == NhomVe.VeMoi)).ToArray());
            }).ToArray());
    }
}

public sealed record EventPublicationPreviewDto(int VersionId, string Name, string? Category, string? Description,
    string? Image, string? Program, string? Participants, string? Rules, string? Policy, bool AllowRefund,
    IReadOnlyList<PublicationSessionDto> Sessions);
public sealed record PublicationSessionDto(string Name, string Venue, DateTimeOffset? From, DateTimeOffset? To,
    int Capacity, int? RentalId, decimal? RentalTotal, decimal? Deposit, IReadOnlyList<PublicationPriceDto> Prices);
public sealed record PublicationPriceDto(string Name, string Zone, decimal Price, int Quantity, DateTimeOffset? From,
    DateTimeOffset? To, string? Benefits, bool Invitation);
