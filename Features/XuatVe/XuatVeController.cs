using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using QLSK.Common.Auth; using QLSK.Common.Models;
namespace QLSK.Features.XuatVe;
[ApiController,Route("api/tickets"),Authorize] public class XuatVeController(XuatVeService dichVuXuatVe):ControllerBase
{
 [HttpGet("{id:int}/exports")] public async Task<IActionResult> LayLichSuXuatVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int veDaPhatHanhId)=>Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuXuatVe.LayLichSuXuatVe(User.LayTaiKhoanId(),veDaPhatHanhId,User.IsInRole("QuanTriVien"))));
 [HttpPost("{id:int}/export")] public async Task<IActionResult> XuatVePdf([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int veDaPhatHanhId)=>File(await dichVuXuatVe.XuatVePdf(User.LayTaiKhoanId(),veDaPhatHanhId),"application/pdf",$"ve-{veDaPhatHanhId}.pdf");
 [HttpPost("{id:int}/email")] public async Task<IActionResult> GuiVeQuaEmail([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int veDaPhatHanhId){await dichVuXuatVe.GuiVeQuaEmail(User.LayTaiKhoanId(),veDaPhatHanhId);return Ok(ApiResponse.TaoPhanHoiThanhCong(new{message="Đã gửi vé."}));}
}
