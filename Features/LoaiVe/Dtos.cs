using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.LoaiVe;
public class LoaiVeDto
{
    [Required, MaxLength(120)] public string Ten { get; set; } = "";
    [Range(0, double.MaxValue)] public decimal Gia { get; set; }
    [Range(1, int.MaxValue)] public int SoLuong { get; set; }
    public DateTime MoBan { get; set; }
    public DateTime DongBan { get; set; }
}
