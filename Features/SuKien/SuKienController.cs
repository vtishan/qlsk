using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.SuKien;
[ApiController, Route("api/event-types")]
public class LoaiSuKienController(SuKienService service) : ControllerBase
{
    [HttpGet, AllowAnonymous] public async Task<IActionResult> Get() => Ok(ApiResponse.Ok(await service.LayLoai()));
    [HttpGet("all"), Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> All() => Ok(ApiResponse.Ok(await service.LayLoai(true)));
    [HttpPost, Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> Tao(LoaiSuKienDto dto) => Ok(ApiResponse.Ok(await service.TaoLoai(dto)));
    [HttpPut("{id:int}"), Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> Sua(int id, LoaiSuKienDto dto) => Ok(ApiResponse.Ok(await service.SuaLoai(id, dto)));
}
[ApiController, Route("api/events")]
public class SuKienController(SuKienService service) : ControllerBase
{
    [HttpGet, AllowAnonymous] public async Task<IActionResult> Get() => Ok(ApiResponse.Ok(await service.DanhSach()));
    [HttpGet("manage"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> Manage() => Ok(ApiResponse.Ok(await service.DanhSach(true)));
    [HttpGet("mine"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> Mine() => Ok(ApiResponse.Ok(await service.CuaToi(User.LayId())));
    [HttpGet("{id:int}"), AllowAnonymous] public async Task<IActionResult> Lay(int id) => Ok(ApiResponse.Ok(await service.Lay(id, User.Identity?.IsAuthenticated == true ? User.LayId() : null, User.IsInRole("QuanLySuKien") || User.IsInRole("QuanTriVien"))));
    [HttpPost, Authorize(Roles = "BanToChuc")] public async Task<IActionResult> Tao(TaoSuKienDto dto) => Ok(ApiResponse.Ok(await service.Tao(User.LayId(), dto)));
    [HttpPut("{id:int}"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> Sua(int id, TaoSuKienDto dto) => Ok(ApiResponse.Ok(await service.Sua(User.LayId(), id, dto)));
    [HttpPost("{id:int}/submit"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> Gui(int id) => Ok(ApiResponse.Ok(await service.GuiDuyet(User.LayId(), id)));
    [HttpPost("{id:int}/approve"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> Duyet(int id) { await service.Duyet(User.LayId(), id); return Ok(ApiResponse.Ok(new { message = "Đã duyệt sự kiện." })); }
    [HttpPost("{id:int}/reject"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> TuChoi(int id, TuChoiSuKienDto dto) { await service.TuChoi(User.LayId(), id, dto.LyDo); return Ok(ApiResponse.Ok(new { message = "Đã từ chối sự kiện." })); }
    [HttpPost("{id:int}/cancel"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> Huy(int id) { await service.Huy(User.LayId(), id); return Ok(ApiResponse.Ok(new { message = "Đã hủy sự kiện." })); }
}
