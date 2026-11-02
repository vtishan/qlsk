using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.SuKien;
[ApiController, Route("api/event-types")]
public class LoaiSuKienController(SuKienService dichVuSuKien) : ControllerBase
{
    [HttpGet, AllowAnonymous] public async Task<IActionResult> LayLoaiSuKienHoatDong() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.LayDanhSachLoaiSuKien()));
    [HttpGet("all"), Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> LayTatCaLoaiSuKien() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.LayDanhSachLoaiSuKien(true)));
    [HttpPost, Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> TaoLoaiSuKien(LoaiSuKienDto duLieuLoaiSuKien) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.TaoLoaiSuKien(duLieuLoaiSuKien)));
    [HttpPut("{id:int}"), Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> CapNhatLoaiSuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int loaiSuKienId, LoaiSuKienDto duLieuLoaiSuKien) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.CapNhatLoaiSuKien(loaiSuKienId, duLieuLoaiSuKien)));
}
[ApiController, Route("api/events")]
public class SuKienController(SuKienService dichVuSuKien) : ControllerBase
{
    [HttpGet, AllowAnonymous] public async Task<IActionResult> LaySuKienCongKhai() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.LayDanhSachSuKien()));
    [HttpGet("manage"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> LaySuKienQuanLy() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.LayDanhSachSuKien(true)));
    [HttpGet("mine"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> LaySuKienCuaToi() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.LaySuKienCuaTaiKhoan(User.LayTaiKhoanId())));
    [HttpGet("{id:int}"), AllowAnonymous] public async Task<IActionResult> LayChiTietSuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int suKienId) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.LayChiTietSuKien(suKienId, User.Identity?.IsAuthenticated == true ? User.LayTaiKhoanId() : null, User.IsInRole("QuanLySuKien") || User.IsInRole("QuanTriVien"))));
    [HttpPost, Authorize(Roles = "BanToChuc")] public async Task<IActionResult> TaoSuKien(TaoSuKienDto duLieuTaoSuKien) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.TaoSuKien(User.LayTaiKhoanId(), duLieuTaoSuKien)));
    [HttpPut("{id:int}"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> CapNhatVaGuiLaiSuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int suKienId, TaoSuKienDto duLieuTaoSuKien) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.CapNhatVaGuiLaiSuKien(User.LayTaiKhoanId(), suKienId, duLieuTaoSuKien)));
    [HttpPost("{id:int}/submit"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> GuiDuyetSuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int suKienId) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuSuKien.GuiDuyetSuKien(User.LayTaiKhoanId(), suKienId)));
    [HttpPost("{id:int}/approve"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> DuyetSuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int suKienId) { await dichVuSuKien.DuyetSuKien(User.LayTaiKhoanId(), suKienId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã duyệt sự kiện." })); }
    [HttpPost("{id:int}/reject"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> TuChoiSuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int suKienId, TuChoiSuKienDto duLieuTuChoiSuKien) { await dichVuSuKien.TuChoiSuKien(User.LayTaiKhoanId(), suKienId, duLieuTuChoiSuKien.LyDo); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã từ chối sự kiện." })); }
    [HttpPost("{id:int}/cancel"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> HuySuKien([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int suKienId) { await dichVuSuKien.HuySuKien(User.LayTaiKhoanId(), suKienId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã hủy sự kiện." })); }
}
