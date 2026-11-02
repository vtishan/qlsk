using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class DonVe
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int DonVeId { get; set; }

    public int TaiKhoanId { get; set; }

    public int SuKienId { get; set; }

    public int SoLuong { get; set; }

    public decimal TongTien { get; set; }

    public TrangThaiDon TrangThai { get; set; } = TrangThaiDon.ChoThanhToan;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonPropertyName("hetHan")]
    [System.ComponentModel.DataAnnotations.Schema.Column("HetHan")]
    public DateTime? ThoiGianHetHan { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rowVersion")]
    [System.ComponentModel.DataAnnotations.Schema.Column("RowVersion")]
    public byte[] PhienBanDong { get; set; } = [];

    public TaiKhoan TaiKhoan { get; set; } = null!;

    public SuKien SuKien { get; set; } = null!;

    [System.Text.Json.Serialization.JsonPropertyName("chiTiets")]
    public ICollection<ChiTietDonVe> ChiTietDonVe { get; set; } = [];

    [System.Text.Json.Serialization.JsonPropertyName("thanhToans")]
    public ICollection<ThanhToan> DanhSachThanhToan { get; set; } = [];

    [System.Text.Json.Serialization.JsonPropertyName("ves")]
    public ICollection<Ve> DanhSachVe { get; set; } = [];
}
