using System.Text.Json.Serialization;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.Common.Models;

// These snapshots deliberately have no EF navigation properties or account/organizer identifiers.
public sealed record PurchasedEventDto(int Id, int LoaiSuKienId, string Ten, string MoTa, string? HinhAnh,
    DateTime BatDau, DateTime KetThuc, int SoNguoi, TrangThaiSuKien TrangThai, PublicOrganizerDto? BanToChuc);
public sealed record PurchasedTicketTypeDto(int Id, int SuKienId, string Ten, decimal Gia, int SoLuong,
    int DaBan, int DangGiu, DateTime MoBan, DateTime DongBan, PurchasedEventDto? SuKien);
public sealed record OrderSummaryDto(int Id, int SuKienId, int SoLuong, decimal TongTien,
    TrangThaiDon TrangThai, DateTime NgayTao, DateTime? HetHan, PurchasedEventDto? SuKien);
public sealed record OrderItemDto(int Id, int DonVeId, int LoaiVeId, int SoLuong, decimal DonGia,
    decimal ThanhTien, PurchasedTicketTypeDto? LoaiVe);
public sealed record PurchasedPaymentDto(int Id, int DonVeId, decimal SoTien, decimal SoTienHoan,
    string? MaGiaoDich, string? MaLienKet, string? MaHoanTien, TrangThaiThanhToan TrangThai,
    DateTime NgayTao, DateTime? NgayThanhToan);
public sealed record PurchasedTicketDto(int Id, int DonVeId, int LoaiVeId, string MaVe,
    [property: JsonPropertyName("maQR")] string MaQR, TrangThaiVe TrangThai, DateTime NgayTao,
    DateTime? NgayCheckIn, OrderSummaryDto? DonVe, PurchasedTicketTypeDto? LoaiVe);
public sealed record PurchasedOrderDto(int Id, int SuKienId, int SoLuong, decimal TongTien,
    TrangThaiDon TrangThai, DateTime NgayTao, DateTime? HetHan, PurchasedEventDto? SuKien,
    IReadOnlyList<OrderItemDto> ChiTiets, IReadOnlyList<PurchasedPaymentDto> ThanhToans,
    IReadOnlyList<PurchasedTicketDto> Ves);

internal static class PurchaseResponse
{
    private static PurchasedEventDto? Event(SuKien? e) => e is null ? null : new(e.SuKienId,
        e.LoaiSuKienId, e.TenSuKien, e.MoTaSuKien, e.HinhAnh, e.ThoiGianBatDau, e.ThoiGianKetThuc,
        e.SoNguoi, e.TrangThai, e.BanToChuc is null ? null : new PublicOrganizerDto(e.BanToChuc.TenBanToChuc));

    private static PurchasedTicketTypeDto? Type(LoaiVe? t, SuKien? e = null) => t is null ? null : new(
        t.LoaiVeId, t.SuKienId, t.TenLoaiVe, t.GiaVe, t.SoLuong, t.SoLuongDaBan, t.SoLuongDangGiu,
        t.ThoiGianMoBan, t.ThoiGianDongBan, Event(t.SuKien ?? e));

    private static OrderSummaryDto? Summary(DonVe? o) => o is null ? null : new(o.DonVeId, o.SuKienId,
        o.SoLuong, o.TongTien, o.TrangThai, o.NgayTao, o.ThoiGianHetHan, Event(o.SuKien));

    internal static PurchasedTicketDto Ticket(Ve t) => new(t.VeId, t.DonVeId, t.LoaiVeId, t.MaVe,
        t.NoiDungMaQr, t.TrangThai, t.NgayTao, t.NgayCheckIn, Summary(t.DonVe), Type(t.LoaiVe));

    internal static PurchasedOrderDto Order(DonVe o) => new(o.DonVeId, o.SuKienId, o.SoLuong, o.TongTien,
        o.TrangThai, o.NgayTao, o.ThoiGianHetHan, Event(o.SuKien),
        o.ChiTietDonVe.Select(i => new OrderItemDto(i.ChiTietDonVeId, i.DonVeId, i.LoaiVeId, i.SoLuong,
            i.DonGia, i.ThanhTien, Type(i.LoaiVe, o.SuKien))).ToList(),
        o.DanhSachThanhToan.Select(p => new PurchasedPaymentDto(p.ThanhToanId, p.DonVeId, p.SoTien,
            p.SoTienHoan, p.MaGiaoDich, p.MaLienKet, p.MaHoanTien, p.TrangThai, p.NgayTao, p.NgayThanhToan)).ToList(),
        o.DanhSachVe.Select(t => new PurchasedTicketDto(t.VeId, t.DonVeId, t.LoaiVeId, t.MaVe,
            t.NoiDungMaQr, t.TrangThai, t.NgayTao, t.NgayCheckIn, Summary(o),
            Type(t.LoaiVe ?? o.ChiTietDonVe.FirstOrDefault(i => i.LoaiVeId == t.LoaiVeId)?.LoaiVe, o.SuKien))).ToList());
}
