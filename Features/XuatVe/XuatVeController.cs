using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using QLSK.Common.Auth; using QLSK.Common.Models;
namespace QLSK.Features.XuatVe;
[ApiController,Route("api/tickets"),Authorize] public class XuatVeController(XuatVeService service):ControllerBase
{
 [HttpPost("{id:int}/export")] public async Task<IActionResult> Pdf(int id)=>File(await service.Pdf(User.LayId(),id),"application/pdf",$"ve-{id}.pdf");
 [HttpPost("{id:int}/email")] public async Task<IActionResult> Email(int id){await service.Gui(User.LayId(),id);return Ok(ApiResponse.Ok(new{message="Đã gửi vé."}));}
}
