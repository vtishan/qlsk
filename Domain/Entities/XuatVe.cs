namespace QLSK.Domain.Entities;

public class XuatVe
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int XuatVeId { get; set; }

    public int VeId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("kieu")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Kieu")]
    public string HinhThucXuat { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("diaChi")]
    [System.ComponentModel.DataAnnotations.Schema.Column("DiaChi")]
    public string? EmailNhan { get; set; }

    public bool ThanhCong { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonPropertyName("ve")]
    public Ve VeDuocXuat { get; set; } = null!;
}
