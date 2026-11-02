using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.DiaDiem;

[ApiController, Route("api/venues")]
public class DiaDiemController(DiaDiemService dichVuDiaDiem) : ControllerBase
{
    [HttpGet, AllowAnonymous] public async Task<IActionResult> LayDiaDiemHoatDong() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDiaDiem.LayDanhSachDiaDiem()));
    [HttpGet("all"), Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> LayTatCaDiaDiem() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDiaDiem.LayDanhSachDiaDiem(true)));
    [HttpPost, Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> TaoDiaDiem(DiaDiemDto duLieuDiaDiem) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDiaDiem.TaoDiaDiem(duLieuDiaDiem)));
    [HttpPut("{id:int}"), Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> CapNhatDiaDiem([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int diaDiemId, DiaDiemDto duLieuDiaDiem) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDiaDiem.CapNhatDiaDiem(diaDiemId, duLieuDiaDiem)));
}
[ApiController, Route("api/venue-bookings"), Authorize]
public class DatDiaDiemController(DiaDiemService dichVuDiaDiem) : ControllerBase
{
    [HttpPost, Authorize(Roles = "BanToChuc")] public async Task<IActionResult> DatDiaDiemChoSuKien(DatDiaDiemDto duLieuDatDiaDiem) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDiaDiem.DatDiaDiemChoSuKien(User.LayTaiKhoanId(), duLieuDatDiaDiem)));
    [HttpGet, Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> LayDanhSachDatDiaDiem() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuDiaDiem.LayDanhSachDatDiaDiem()));
    [HttpPost("{id:int}/approve"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> XacNhanDatDiaDiem([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int datDiaDiemId) { await dichVuDiaDiem.XacNhanDatDiaDiem(datDiaDiemId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã xác nhận." })); }
    [HttpPost("{id:int}/reject"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> TuChoiDatDiaDiem([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int datDiaDiemId) { await dichVuDiaDiem.TuChoiDatDiaDiem(datDiaDiemId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã từ chối." })); }
    [HttpPost("{id:int}/cancel"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> HuyDatDiaDiem([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int datDiaDiemId) { await dichVuDiaDiem.HuyDatDiaDiem(User.LayTaiKhoanId(), datDiaDiemId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã hủy." })); }
}
