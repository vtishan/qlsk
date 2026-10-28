using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;

namespace QLSK.Features.TaiKhoan;
[ApiController, Route("api/accounts"), Authorize]
public class TaiKhoanController(TaiKhoanService service) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me() => Ok(ApiResponse.Ok(await service.Lay(User.LayId())));

    [HttpPut("me")]
    public async Task<IActionResult> Sua(SuaTaiKhoanDto dto) => Ok(ApiResponse.Ok(await service.Sua(User.LayId(), dto)));

    [HttpGet, Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> DanhSach([FromQuery] string? q, [FromQuery] int? vaiTroId, [FromQuery] bool? hoatDong, [FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(ApiResponse.Ok(await service.DanhSach(q, vaiTroId, hoatDong, page, size)));

    [HttpPost("{id:int}/lock"), Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> Khoa(int id) { await service.Khoa(id, true); return Ok(ApiResponse.Ok(new { message = "Đã khóa tài khoản." })); }

    [HttpPost("{id:int}/unlock"), Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> MoKhoa(int id) { await service.Khoa(id, false); return Ok(ApiResponse.Ok(new { message = "Đã mở khóa tài khoản." })); }

    [HttpPut("{id:int}/role"), Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> DoiVaiTro(int id, DoiVaiTroDto dto) { await service.DoiVaiTro(id, dto.VaiTroId); return Ok(ApiResponse.Ok(new { message = "Đã đổi vai trò." })); }
}
