using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using QLSK.Common.Auth; using QLSK.Common.Models;
namespace QLSK.Features.Ve;
[ApiController,Route("api/tickets"),Authorize] public class VeController(VeService dichVuVe):ControllerBase
{
 [HttpGet("me")] public async Task<IActionResult> LayVeCuaToi()=>Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuVe.LayVeCuaTaiKhoan(User.LayTaiKhoanId())));
 [HttpGet("{id:int}")] public async Task<IActionResult> LayChiTietVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int veDaPhatHanhId)=>Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuVe.LayChiTietVe(User.LayTaiKhoanId(),veDaPhatHanhId,User.IsInRole("QuanLySuKien")||User.IsInRole("QuanTriVien"))));
 [HttpGet("by-code/{code}")] public async Task<IActionResult> LayVeTheoMaVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "code")] string maVeTraCuu)=>Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuVe.LayVeTheoMaVe(User.LayTaiKhoanId(),maVeTraCuu,User.IsInRole("QuanLySuKien")||User.IsInRole("QuanTriVien"))));
}
[ApiController,Route("api/check-in"),Authorize(Roles="QuanLySuKien,QuanTriVien")] public class CheckInController(VeService dichVuVe):ControllerBase
{
 public class CheckInDto{[System.Text.Json.Serialization.JsonPropertyName("ma")] [Required] public string MaVeHoacQr{get;set;}="";}
 [HttpPost] public async Task<IActionResult> CheckInVe(CheckInDto duLieuCheckIn)=>Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuVe.CheckInVe(duLieuCheckIn.MaVeHoacQr.Trim())));
}
