using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using QLSK.Common.Models;
namespace QLSK.Features.ThongKe; [ApiController,Route("api/admin/dashboard"),Authorize(Roles="QuanTriVien")] public class ThongKeController(ThongKeService dichVuThongKe):ControllerBase{[HttpGet]public async Task<IActionResult> LayThongKeTongQuan()=>Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuThongKe.LayThongKeTongQuan()));
[HttpGet("events/{id:int}/check-in")] public async Task<IActionResult> ThongKeTrangThaiVeTheoSuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int suKienId)=>Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuThongKe.ThongKeTrangThaiVeTheoSuKien(suKienId)));
}
