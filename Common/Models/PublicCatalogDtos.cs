namespace QLSK.Common.Models;

// Public responses contain only display/purchase fields, never EF entities or navigation properties.
public sealed record PublicOrganizerDto(string Ten);
public sealed record PublicEventTypeDto(int Id, string Ten, string MoTa);
public sealed record PublicEventVenueDto(int Id, string Ten, string DiaChi);
public sealed record PublicEventLocationDto(PublicEventVenueDto DiaDiem);
public sealed record PublicVenueDto(int Id, string Ten, string DiaChi, string? HinhAnh, int SucChua, decimal GiaThue);
public sealed record PublicTicketTypeDto(int Id, int SuKienId, string Ten, decimal Gia, int SoLuong,
    int DaBan, int DangGiu, DateTime MoBan, DateTime DongBan);
