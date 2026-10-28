using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.XacThucToChuc;
[ApiController, Route("api/organizer-verifications"), Authorize]
public class XacThucController(XacThucService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Gui(GuiYeuCauDto dto) => Ok(ApiResponse.Ok(await service.Gui(User.LayId(), dto)));
    [HttpGet("me")]
    public async Task<IActionResult> Me() => Ok(ApiResponse.Ok(await service.CuaToi(User.LayId())));
    [HttpGet, Authorize(Roles = "QuanLySuKien,QuanTriVien")]
    public async Task<IActionResult> DanhSach() => Ok(ApiResponse.Ok(await service.DanhSach()));
    [HttpPost("{id:int}/approve"), Authorize(Roles = "QuanLySuKien,QuanTriVien")]
    public async Task<IActionResult> Duyet(int id) { await service.Duyet(User.LayId(), id); return Ok(ApiResponse.Ok(new { message = "Đã duyệt yêu cầu." })); }
    [HttpPost("{id:int}/reject"), Authorize(Roles = "QuanLySuKien,QuanTriVien")]
    public async Task<IActionResult> TuChoi(int id, TuChoiYeuCauDto dto) { await service.TuChoi(User.LayId(), id, dto.LyDo); return Ok(ApiResponse.Ok(new { message = "Đã từ chối yêu cầu." })); }
}
