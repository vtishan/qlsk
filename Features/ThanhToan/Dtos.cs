using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.ThanhToan;
public class TaoThanhToanDto { [Range(1, int.MaxValue)] public int DonVeId { get; set; } }
public class HoanTienDto
{
    [Range(0.01, double.MaxValue)] public decimal SoTien { get; set; }
    [Required, MaxLength(120)] public string MaHoanTien { get; set; } = "";
}
