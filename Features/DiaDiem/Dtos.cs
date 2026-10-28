using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.DiaDiem;
public class DiaDiemDto
{
    [Required, MaxLength(200)] public string Ten { get; set; } = "";
    [Required, MaxLength(300)] public string DiaChi { get; set; } = "";
    [Range(1, int.MaxValue)] public int SucChua { get; set; }
    [Range(0, double.MaxValue)] public decimal GiaThue { get; set; }
    public bool HoatDong { get; set; } = true;
}
public class DatDiaDiemDto
{
    [Range(1, int.MaxValue)] public int DiaDiemId { get; set; }
    [Range(1, int.MaxValue)] public int SuKienId { get; set; }
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }
    [Range(0, double.MaxValue)] public decimal TienCoc { get; set; }
}
