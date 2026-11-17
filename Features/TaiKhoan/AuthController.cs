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
    public async Task<IActionResult> DangXuatTaiKhoan()
    {
        await dichVuTaiKhoan.DangXuatTaiKhoan(User.LayTaiKhoanId());
        return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã đăng xuất các phiên của tài khoản." }));
    }

    [HttpPut("change-password"), Authorize]
    public async Task<IActionResult> DoiMatKhauTaiKhoan(DoiMatKhauDto duLieuDoiMatKhau)
    {
        await dichVuTaiKhoan.DoiMatKhauTaiKhoan(User.LayTaiKhoanId(), duLieuDoiMatKhau);
        return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã đổi mật khẩu." }));
    }
}
