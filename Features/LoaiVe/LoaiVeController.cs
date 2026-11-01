using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.LoaiVe;
[ApiController, Authorize]
public class LoaiVeController(LoaiVeService service) : ControllerBase
{
    [HttpGet("api/events/{eventId:int}/ticket-types"), AllowAnonymous]
    public async Task<IActionResult> Get(int eventId) => Ok(ApiResponse.Ok(await service.TheoSuKien(
        eventId,
        User.Identity?.IsAuthenticated == true ? User.LayId() : null,
        User.IsInRole("QuanLySuKien") || User.IsInRole("QuanTriVien"))));
    [HttpPost("api/events/{eventId:int}/ticket-types"), Authorize(Roles = "BanToChuc,QuanTriVien")]
    public async Task<IActionResult> Tao(int eventId, LoaiVeDto dto) => Ok(ApiResponse.Ok(await service.Tao(User.LayId(), eventId, dto, User.IsInRole("QuanTriVien"))));
    [HttpPut("api/ticket-types/{id:int}"), Authorize(Roles = "BanToChuc,QuanTriVien")]
    public async Task<IActionResult> Sua(int id, LoaiVeDto dto) => Ok(ApiResponse.Ok(await service.Sua(User.LayId(), id, dto, User.IsInRole("QuanTriVien"))));
    [HttpPost("api/ticket-types/{id:int}/open"), Authorize(Roles = "BanToChuc")]
    public async Task<IActionResult> Mo(int id) { await service.MoBan(User.LayId(), id); return Ok(ApiResponse.Ok(new { message = "Đã mở bán." })); }
    [HttpPost("api/ticket-types/{id:int}/close"), Authorize(Roles = "BanToChuc")]
    public async Task<IActionResult> Dong(int id) { await service.DongBan(User.LayId(), id); return Ok(ApiResponse.Ok(new { message = "Đã đóng bán." })); }
}
