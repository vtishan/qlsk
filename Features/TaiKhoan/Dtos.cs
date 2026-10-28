using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.TaiKhoan;
public class DangKyDto
{
    [Required, MaxLength(120)] public string HoTen { get; set; } = "";
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = "";
    [Required, MinLength(6), MaxLength(100)] public string MatKhau { get; set; } = "";
}
public class DangNhapDto
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string MatKhau { get; set; } = "";
}
public class DoiMatKhauDto
{
    [Required] public string MatKhauCu { get; set; } = "";
    [Required, MinLength(6), MaxLength(100)] public string MatKhauMoi { get; set; } = "";
}
public class SuaTaiKhoanDto
{
    [Required, MaxLength(120)] public string HoTen { get; set; } = "";
}
public class DoiVaiTroDto
{
    [Range(1, 4)] public int VaiTroId { get; set; }
}
public record TaiKhoanDto(int Id, string HoTen, string Email, string VaiTro, bool HoatDong, DateTime NgayTao);
public record DangNhapResult(string Token, TaiKhoanDto TaiKhoan);
