using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
using QLSK.Infrastructure.PayOS;
namespace QLSK.Features.ThanhToan;
[ApiController, Route("api/payments")]
public class ThanhToanController(ThanhToanService dichVuThanhToan) : ControllerBase
{
    [HttpPost, Authorize] public async Task<IActionResult> BatDauThanhToanDonVe(TaoThanhToanDto duLieuTaoThanhToan) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuThanhToan.BatDauThanhToanDonVe(User.LayTaiKhoanId(), duLieuTaoThanhToan.DonVeId)));
    [HttpPost("webhook"), AllowAnonymous] public async Task<IActionResult> XuLyWebhookThanhToan(PayOsWebhook webhook) { await dichVuThanhToan.XuLyWebhookThanhToan(webhook); return Ok(new { success = true }); }
    [HttpPost("{id:int}/refund"), Authorize(Roles = "QuanTriVien,QuanLySuKien")] public async Task<IActionResult> GhiNhanHoanTien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int thanhToanId, HoanTienDto duLieuHoanTien) { await dichVuThanhToan.GhiNhanHoanTien(thanhToanId, duLieuHoanTien.SoTienHoan, duLieuHoanTien.MaHoanTien); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã ghi nhận hoàn tiền." })); }
}
