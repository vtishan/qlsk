using System.ComponentModel.DataAnnotations;
using QLSK.Features.SuKien;

namespace QLSK.Features.QuanTri;

public class QuanTriSuKienDto : TaoSuKienDto
{
    [Range(1, int.MaxValue)] public int BanToChucId { get; set; }
}

public class QuanTriToChucDto
{
    [Range(1, int.MaxValue)] public int TaiKhoanId { get; set; }
    [Required, MaxLength(200)] public string Ten { get; set; } = "";
    [MaxLength(1000)] public string MoTa { get; set; } = "";
}

public class QuanTriTaiKhoanDto
{
    [Required, MaxLength(120)] public string HoTen { get; set; } = "";
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = "";
    [MinLength(8), MaxLength(100)] public string? MatKhauMoi { get; set; }
}
