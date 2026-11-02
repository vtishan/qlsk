using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.LoaiVe;
public class LoaiVeDto
{
    [System.Text.Json.Serialization.JsonPropertyName("ten")] [Required, MaxLength(120)] public string TenLoaiVe { get; set; } = "";
    [System.Text.Json.Serialization.JsonPropertyName("gia")] [Range(0, double.MaxValue)] public decimal GiaVe { get; set; }
    [Range(1, int.MaxValue)] public int SoLuong { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("moBan")] public DateTime ThoiGianMoBan { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("dongBan")] public DateTime ThoiGianDongBan { get; set; }
}
