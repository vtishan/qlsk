using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.XacThucToChuc;
[ApiController, Route("api/organizer-verifications"), Authorize]
public class XacThucController(XacThucService dichVuXacThuc) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GuiYeuCauXacThucToChuc(GuiYeuCauDto duLieuGuiYeuCau) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuXacThuc.GuiYeuCauXacThucToChuc(User.LayTaiKhoanId(), duLieuGuiYeuCau)));
    [HttpGet("me")]
    public async Task<IActionResult> LayYeuCauDangXuLyCuaToi() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuXacThuc.LayYeuCauDangXuLyCuaTaiKhoan(User.LayTaiKhoanId())));
    [HttpGet, Authorize(Roles = "QuanLySuKien,QuanTriVien")]
    public async Task<IActionResult> LayDanhSachYeuCauDangXuLy() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuXacThuc.LayDanhSachYeuCauDangXuLy()));
    [HttpPost("{id:int}/approve"), Authorize(Roles = "QuanLySuKien,QuanTriVien")]
    public async Task<IActionResult> DuyetYeuCauXacThuc([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int yeuCauXacThucId) { await dichVuXacThuc.DuyetYeuCauXacThuc(User.LayTaiKhoanId(), yeuCauXacThucId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã duyệt yêu cầu." })); }
    [HttpPost("{id:int}/reject"), Authorize(Roles = "QuanLySuKien,QuanTriVien")]
    public async Task<IActionResult> TuChoiYeuCauXacThuc([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int yeuCauXacThucId, TuChoiYeuCauDto duLieuTuChoiYeuCau) { await dichVuXacThuc.TuChoiYeuCauXacThuc(User.LayTaiKhoanId(), yeuCauXacThucId, duLieuTuChoiYeuCau.LyDo); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã từ chối yêu cầu." })); }
}
