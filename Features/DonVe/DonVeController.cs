using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.DonVe;
[ApiController, Route("api/orders"), Authorize]
public class DonVeController(DonVeService service) : ControllerBase
{
    [HttpPost] public async Task<IActionResult> Tao(TaoDonDto dto) => Ok(ApiResponse.Ok(await service.Tao(User.LayId(), dto)));
    [HttpGet("me")] public async Task<IActionResult> Me() => Ok(ApiResponse.Ok(await service.CuaToi(User.LayId())));
    [HttpGet("{id:int}")] public async Task<IActionResult> Lay(int id) => Ok(ApiResponse.Ok(await service.Lay(User.LayId(), id, User.IsInRole("QuanLySuKien") || User.IsInRole("QuanTriVien"))));
    [HttpPost("{id:int}/cancel")] public async Task<IActionResult> Huy(int id) { await service.Huy(User.LayId(), id); return Ok(ApiResponse.Ok(new { message = "Đã hủy đơn." })); }
}
