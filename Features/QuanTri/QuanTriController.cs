using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Models;

namespace QLSK.Features.QuanTri;

[ApiController, Route("api/admin/catalog"), Authorize(Roles = "QuanTriVien")]
public class QuanTriController(QuanTriService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Get() => Ok(ApiResponse.Ok(await service.DanhMuc()));
    [HttpPost("events")] public async Task<IActionResult> TaoSuKien(QuanTriSuKienDto dto) => Ok(ApiResponse.Ok(await service.LuuSuKien(null, dto)));
    [HttpPut("events/{id:int}")] public async Task<IActionResult> SuaSuKien(int id, QuanTriSuKienDto dto) => Ok(ApiResponse.Ok(await service.LuuSuKien(id, dto)));
    [HttpPost("organizers")] public async Task<IActionResult> TaoToChuc(QuanTriToChucDto dto) => Ok(ApiResponse.Ok(await service.LuuToChuc(null, dto)));
    [HttpPut("organizers/{id:int}")] public async Task<IActionResult> SuaToChuc(int id, QuanTriToChucDto dto) => Ok(ApiResponse.Ok(await service.LuuToChuc(id, dto)));
    [HttpPut("accounts/{id:int}")] public async Task<IActionResult> SuaTaiKhoan(int id, QuanTriTaiKhoanDto dto)
    { await service.SuaTaiKhoan(id, dto); return Ok(ApiResponse.Ok(new { message = "Đã cập nhật tài khoản." })); }
    [HttpGet("accounts/{id:int}/activity")] public async Task<IActionResult> HoatDong(int id) => Ok(ApiResponse.Ok(await service.TheoTaiKhoan(id)));
    [HttpDelete("{kind}/{id:int}")] public async Task<IActionResult> Xoa(string kind, int id)
    { await service.Xoa(kind, id); return Ok(ApiResponse.Ok(new { message = "Đã xóa bản ghi chưa sử dụng." })); }
}
