using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using QLSK.Common.Models;
namespace QLSK.Features.ThongKe; [ApiController,Route("api/admin/dashboard"),Authorize(Roles="QuanTriVien")] public class ThongKeController(ThongKeService service):ControllerBase{[HttpGet]public async Task<IActionResult> Get()=>Ok(ApiResponse.Ok(await service.Dashboard()));
[HttpGet("events/{id:int}/check-in")] public async Task<IActionResult> CheckIn(int id)=>Ok(ApiResponse.Ok(await service.CheckIn(id)));
}
