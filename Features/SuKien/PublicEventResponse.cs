using System.Linq.Expressions;
using System.Text.Json.Serialization;
using QLSK.Common.Models;
using QLSK.Domain.Enums;
using SuKienEntity = QLSK.Domain.Entities.SuKien;

namespace QLSK.Features.SuKien;

public sealed record PublicEventDto
{
    public int Id { get; init; }
    public int LoaiSuKienId { get; init; }
    public string Ten { get; init; } = "";
    public string MoTa { get; init; } = "";
    public string? HinhAnh { get; init; }
    public decimal? GiaThapNhat { get; init; }
    public bool MetadataComplete { get; init; } = true;
    public DateTime BatDau { get; init; }
    public DateTime KetThuc { get; init; }
    public int SoNguoi { get; init; }
    public TrangThaiSuKien TrangThai { get; init; }
    public PublicOrganizerDto BanToChuc { get; init; } = null!;
    public PublicEventTypeDto LoaiSuKien { get; init; } = null!;
    public PublicEventLocationDto? DatDiaDiem { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<PublicTicketTypeDto>? LoaiVes { get; init; }
}

internal static class PublicEventResponse
{
    // Project straight from SQL to DTOs: entity defaults and future entity fields cannot leak.
    internal static Expression<Func<SuKienEntity, PublicEventDto>> SummaryProjection => e => new PublicEventDto
    {
        Id = e.SuKienId, LoaiSuKienId = e.LoaiSuKienId, Ten = e.TenSuKien, MoTa = e.MoTaSuKien,
        HinhAnh = e.HinhAnh, GiaThapNhat = e.DanhSachLoaiVe.Min(v => (decimal?)v.GiaVe),
        BatDau = e.ThoiGianBatDau, KetThuc = e.ThoiGianKetThuc, SoNguoi = e.SoNguoi, TrangThai = e.TrangThai,
        BanToChuc = new PublicOrganizerDto(e.BanToChuc.TenBanToChuc),
        LoaiSuKien = new PublicEventTypeDto(e.LoaiSuKienId, e.LoaiSuKien.TenLoaiSuKien, e.LoaiSuKien.MoTaLoaiSuKien),
        DatDiaDiem = e.DatDiaDiem == null ? null : new PublicEventLocationDto(new PublicEventVenueDto(
            e.DatDiaDiem.DiaDiemId, e.DatDiaDiem.DiaDiem.TenDiaDiem, e.DatDiaDiem.DiaDiem.DiaChi))
    };

    internal static PublicEventDto From(SuKienEntity e) => new()
    {
        Id = e.SuKienId, LoaiSuKienId = e.LoaiSuKienId, Ten = e.TenSuKien, MoTa = e.MoTaSuKien,
        HinhAnh = e.HinhAnh, GiaThapNhat = e.DanhSachLoaiVe.Min(v => (decimal?)v.GiaVe),
        BatDau = e.ThoiGianBatDau, KetThuc = e.ThoiGianKetThuc, SoNguoi = e.SoNguoi, TrangThai = e.TrangThai,
        BanToChuc = new PublicOrganizerDto(e.BanToChuc.TenBanToChuc),
        LoaiSuKien = new PublicEventTypeDto(e.LoaiSuKienId, e.LoaiSuKien.TenLoaiSuKien, e.LoaiSuKien.MoTaLoaiSuKien),
        DatDiaDiem = e.DatDiaDiem is not { } booking ? null : new PublicEventLocationDto(
            new PublicEventVenueDto(booking.DiaDiemId, booking.DiaDiem.TenDiaDiem, booking.DiaDiem.DiaChi)),
        LoaiVes = e.DanhSachLoaiVe.OrderBy(v => v.GiaVe).ThenBy(v => v.LoaiVeId).Select(v => new PublicTicketTypeDto(
            v.LoaiVeId, v.SuKienId, v.TenLoaiVe, v.GiaVe, v.SoLuong, v.SoLuongDaBan, v.SoLuongDangGiu,
            v.ThoiGianMoBan, v.ThoiGianDongBan)).ToList()
    };
}
