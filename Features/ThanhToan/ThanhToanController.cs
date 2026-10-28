using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
using QLSK.Infrastructure.PayOS;
namespace QLSK.Features.ThanhToan;
[ApiController, Route("api/payments")]
public class ThanhToanController(ThanhToanService service) : ControllerBase
{
    [HttpPost, Authorize] public async Task<IActionResult> Tao(TaoThanhToanDto dto) => Ok(ApiResponse.Ok(await service.Tao(User.LayId(), dto.DonVeId)));
    [HttpPost("webhook"), AllowAnonymous] public async Task<IActionResult> Webhook(PayOsWebhook webhook) { await service.XuLyWebhook(webhook); return Ok(new { success = true }); }
    [HttpPost("{id:int}/refund"), Authorize(Roles = "QuanTriVien,QuanLySuKien")] public async Task<IActionResult> Hoan(int id, HoanTienDto dto) { await service.HoanTien(id, dto.SoTien, dto.MaHoanTien); return Ok(ApiResponse.Ok(new { message = "Đã ghi nhận hoàn tiền." })); }
}
