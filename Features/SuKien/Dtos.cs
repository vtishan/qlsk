using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.SuKien;
public class LoaiSuKienDto
{
    [System.Text.Json.Serialization.JsonPropertyName("ten")] [Required, MaxLength(120)] public string TenLoaiSuKien { get; set; } = "";
    [System.Text.Json.Serialization.JsonPropertyName("moTa")] [MaxLength(500)] public string MoTaLoaiSuKien { get; set; } = "";
    public bool HoatDong { get; set; } = true;
}
public class TaoSuKienDto
{
    [System.Text.Json.Serialization.JsonPropertyName("ten")] [Required, MaxLength(200)] public string TenSuKien { get; set; } = "";
    [System.Text.Json.Serialization.JsonPropertyName("moTa")] [MaxLength(4000)] public string MoTaSuKien { get; set; } = "";
    [MaxLength(500), RegularExpression(@"^(?:/(?!/)[^\s\\]*|https?://[^\s]+)$", ErrorMessage = "Đường dẫn ảnh không hợp lệ.")]
    public string? HinhAnh { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("batDau")] public DateTime ThoiGianBatDau { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("ketThuc")] public DateTime ThoiGianKetThuc { get; set; }
    [Range(1, int.MaxValue)] public int SoNguoi { get; set; }
    [Range(1, int.MaxValue)] public int LoaiSuKienId { get; set; }
}
public class TuChoiSuKienDto { [Required, MaxLength(500)] public string LyDo { get; set; } = ""; }
