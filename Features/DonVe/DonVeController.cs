using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.DonVe;
[ApiController, Route("api/orders"), Authorize]
public class DonVeController(DonVeService dichVuDonVe) : ControllerBase
{
    [HttpPost] public async Task<IActionResult> TaoDonVaGiuVe(TaoDonDto duLieuTaoDon) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDonVe.TaoDonVaGiuVe(User.LayTaiKhoanId(), duLieuTaoDon)));
    [HttpGet("me")] public async Task<IActionResult> LayDonVeCuaToi() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDonVe.LayDonVeCuaTaiKhoan(User.LayTaiKhoanId())));
    [HttpGet("{id:int}")] public async Task<IActionResult> LayChiTietDonVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int donVeId) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDonVe.LayChiTietDonVe(User.LayTaiKhoanId(), donVeId, User.IsInRole("QuanLySuKien") || User.IsInRole("QuanTriVien"))));
    [HttpPost("{id:int}/cancel")] public async Task<IActionResult> HuyDonVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int donVeId) { await dichVuDonVe.HuyDonVe(User.LayTaiKhoanId(), donVeId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã hủy đơn." })); }
}
