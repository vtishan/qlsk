using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.DonVe;
[ApiController, Route("api/orders"), Authorize]
public class DonVeController(DonVeService dichVuDonVe) : ControllerBase
{
    [HttpPost] public async Task<IActionResult> TaoDonVaGiuVe(TaoDonDto duLieuTaoDon) => Ok(ApiResponse.TaoPhanHoiThanhCong(PurchaseResponse.Order(await dichVuDonVe.TaoDonVaGiuVe(User.LayTaiKhoanId(), duLieuTaoDon, Request.Headers["Idempotency-Key"].FirstOrDefault()))));
    [HttpGet("me")] public async Task<IActionResult> LayDonVeCuaToi() => Ok(ApiResponse.TaoPhanHoiThanhCong((await dichVuDonVe.LayDonVeCuaTaiKhoan(User.LayTaiKhoanId())).Select(PurchaseResponse.Order).ToList()));
    [HttpGet("{id:int}")] public async Task<IActionResult> LayChiTietDonVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int donVeId) => Ok(ApiResponse.TaoPhanHoiThanhCong(PurchaseResponse.Order(await dichVuDonVe.LayChiTietDonVe(User.LayTaiKhoanId(), donVeId, User.IsInRole("QuanLySuKien") || User.IsInRole("QuanTriVien")))));
    [HttpPost("{id:int}/cancel")] public async Task<IActionResult> HuyDonVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int donVeId) { await dichVuDonVe.HuyDonVe(User.LayTaiKhoanId(), donVeId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã hủy đơn." })); }
}
