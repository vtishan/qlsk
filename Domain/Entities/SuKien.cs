using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class SuKien
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int SuKienId { get; set; }

    public int BanToChucId { get; set; }

    public int LoaiSuKienId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("ten")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Ten")]
    public string TenSuKien { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("moTa")]
    [System.ComponentModel.DataAnnotations.Schema.Column("MoTa")]
    public string MoTaSuKien { get; set; } = "";

    public string? HinhAnh { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("batDau")]
    [System.ComponentModel.DataAnnotations.Schema.Column("BatDau")]
    public DateTime ThoiGianBatDau { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("ketThuc")]
    [System.ComponentModel.DataAnnotations.Schema.Column("KetThuc")]
    public DateTime ThoiGianKetThuc { get; set; }

    public int SoNguoi { get; set; }

    public TrangThaiSuKien TrangThai { get; set; } = TrangThaiSuKien.ChoDuyet;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonPropertyName("rowVersion")]
    [System.ComponentModel.DataAnnotations.Schema.Column("RowVersion")]
    public byte[] PhienBanDong { get; set; } = [];

    public BanToChuc BanToChuc { get; set; } = null!;

    public LoaiSuKien LoaiSuKien { get; set; } = null!;

    public DatDiaDiem? DatDiaDiem { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("loaiVes")]
    public ICollection<LoaiVe> DanhSachLoaiVe { get; set; } = [];

    [System.Text.Json.Serialization.JsonPropertyName("donVes")]
    public ICollection<DonVe> DanhSachDonVe { get; set; } = [];
}
