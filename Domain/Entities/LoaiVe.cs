namespace QLSK.Domain.Entities;

public class LoaiVe
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int LoaiVeId { get; set; }

    public int SuKienId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("ten")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Ten")]
    public string TenLoaiVe { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("gia")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Gia")]
    public decimal GiaVe { get; set; }

    public int SoLuong { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("daBan")]
    [System.ComponentModel.DataAnnotations.Schema.Column("DaBan")]
    public int SoLuongDaBan { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("dangGiu")]
    [System.ComponentModel.DataAnnotations.Schema.Column("DangGiu")]
    public int SoLuongDangGiu { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("moBan")]
    [System.ComponentModel.DataAnnotations.Schema.Column("MoBan")]
    public DateTime ThoiGianMoBan { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("dongBan")]
    [System.ComponentModel.DataAnnotations.Schema.Column("DongBan")]
    public DateTime ThoiGianDongBan { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rowVersion")]
    [System.ComponentModel.DataAnnotations.Schema.Column("RowVersion")]
    public byte[] PhienBanDong { get; set; } = [];

    public SuKien SuKien { get; set; } = null!;

    [System.Text.Json.Serialization.JsonPropertyName("chiTiets")]
    public ICollection<ChiTietDonVe> ChiTietDonVe { get; set; } = [];

    [System.Text.Json.Serialization.JsonPropertyName("ves")]
    public ICollection<Ve> DanhSachVe { get; set; } = [];
}
