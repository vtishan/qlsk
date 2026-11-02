using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.LoaiVe;
[ApiController, Authorize]
public class LoaiVeController(LoaiVeService dichVuLoaiVe) : ControllerBase
{
    [HttpGet("api/events/{eventId:int}/ticket-types"), AllowAnonymous]
    public async Task<IActionResult> LayLoaiVeTheoSuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "eventId")] int suKienId) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuLoaiVe.LayLoaiVeTheoSuKien(
        suKienId,
        User.Identity?.IsAuthenticated == true ? User.LayTaiKhoanId() : null,
        User.IsInRole("QuanLySuKien") || User.IsInRole("QuanTriVien"))));
    [HttpPost("api/events/{eventId:int}/ticket-types"), Authorize(Roles = "BanToChuc,QuanTriVien")]
    public async Task<IActionResult> TaoLoaiVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "eventId")] int suKienId, LoaiVeDto duLieuLoaiVe) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuLoaiVe.TaoLoaiVe(User.LayTaiKhoanId(), suKienId, duLieuLoaiVe, User.IsInRole("QuanTriVien"))));
    [HttpPut("api/ticket-types/{id:int}"), Authorize(Roles = "BanToChuc,QuanTriVien")]
    public async Task<IActionResult> CapNhatLoaiVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int loaiVeId, LoaiVeDto duLieuLoaiVe) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuLoaiVe.CapNhatLoaiVe(User.LayTaiKhoanId(), loaiVeId, duLieuLoaiVe, User.IsInRole("QuanTriVien"))));
    [HttpPost("api/ticket-types/{id:int}/open"), Authorize(Roles = "BanToChuc")]
    public async Task<IActionResult> MoBanLoaiVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int loaiVeId) { await dichVuLoaiVe.MoBanLoaiVe(User.LayTaiKhoanId(), loaiVeId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã mở bán." })); }
    [HttpPost("api/ticket-types/{id:int}/close"), Authorize(Roles = "BanToChuc")]
    public async Task<IActionResult> DongBanLoaiVe([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int loaiVeId) { await dichVuLoaiVe.DongBanLoaiVe(User.LayTaiKhoanId(), loaiVeId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã đóng bán." })); }
}
