using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;

namespace QLSK.Features.TaiKhoan;
[ApiController, Route("api/auth")]
public class AuthController(TaiKhoanService dichVuTaiKhoan) : ControllerBase
{
    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> DangKyTaiKhoan(DangKyDto duLieuDangKy) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuTaiKhoan.DangKyTaiKhoan(duLieuDangKy)));

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> DangNhapTaiKhoan(DangNhapDto duLieuDangNhap) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuTaiKhoan.DangNhapTaiKhoan(duLieuDangNhap)));

    [HttpPost("logout"), Authorize]
    public IActionResult DangXuatTaiKhoan() => Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đăng xuất thành công. Hãy xóa JWT ở client." }));

    [HttpPut("change-password"), Authorize]
    public async Task<IActionResult> DoiMatKhauTaiKhoan(DoiMatKhauDto duLieuDoiMatKhau)
    {
        await dichVuTaiKhoan.DoiMatKhauTaiKhoan(User.LayTaiKhoanId(), duLieuDoiMatKhau);
        return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã đổi mật khẩu." }));
    }
}
