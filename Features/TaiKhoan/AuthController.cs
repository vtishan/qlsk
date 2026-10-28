using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;

namespace QLSK.Features.TaiKhoan;
[ApiController, Route("api/auth")]
public class AuthController(TaiKhoanService service) : ControllerBase
{
    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> DangKy(DangKyDto dto) => Ok(ApiResponse.Ok(await service.DangKy(dto)));

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> DangNhap(DangNhapDto dto) => Ok(ApiResponse.Ok(await service.DangNhap(dto)));

    [HttpPost("logout"), Authorize]
    public IActionResult DangXuat() => Ok(ApiResponse.Ok(new { message = "Đăng xuất thành công. Hãy xóa JWT ở client." }));

    [HttpPut("change-password"), Authorize]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauDto dto)
    {
        await service.DoiMatKhau(User.LayId(), dto);
        return Ok(ApiResponse.Ok(new { message = "Đã đổi mật khẩu." }));
    }
}
