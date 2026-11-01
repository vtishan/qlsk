using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.SuKien;
public class LoaiSuKienDto
{
    [Required, MaxLength(120)] public string Ten { get; set; } = "";
    [MaxLength(500)] public string MoTa { get; set; } = "";
    public bool HoatDong { get; set; } = true;
}
public class TaoSuKienDto
{
    [Required, MaxLength(200)] public string Ten { get; set; } = "";
    [MaxLength(4000)] public string MoTa { get; set; } = "";
    [MaxLength(500), RegularExpression(@"^(?:/(?!/)[^\s\\]*|https?://[^\s]+)$", ErrorMessage = "Đường dẫn ảnh không hợp lệ.")]
    public string? HinhAnh { get; set; }
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }
    [Range(1, int.MaxValue)] public int SoNguoi { get; set; }
    [Range(1, int.MaxValue)] public int LoaiSuKienId { get; set; }
}
public class TuChoiSuKienDto { [Required, MaxLength(500)] public string LyDo { get; set; } = ""; }
