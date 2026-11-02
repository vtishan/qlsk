namespace QLSK.Domain.Entities;

public class LoaiSuKien
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int LoaiSuKienId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("ten")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Ten")]
    public string TenLoaiSuKien { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("moTa")]
    [System.ComponentModel.DataAnnotations.Schema.Column("MoTa")]
    public string MoTaLoaiSuKien { get; set; } = "";

    public bool HoatDong { get; set; } = true;

    [System.Text.Json.Serialization.JsonPropertyName("suKiens")]
    public ICollection<SuKien> DanhSachSuKien { get; set; } = [];
}
