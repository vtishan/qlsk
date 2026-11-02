using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.ThanhToan;
public class TaoThanhToanDto { [Range(1, int.MaxValue)] public int DonVeId { get; set; } }
public class HoanTienDto
{
    [System.Text.Json.Serialization.JsonPropertyName("soTien")] [Range(0.01, double.MaxValue)] public decimal SoTienHoan { get; set; }
    [Required, MaxLength(120)] public string MaHoanTien { get; set; } = "";
}
