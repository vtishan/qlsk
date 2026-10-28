using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;
namespace QLSK.Features.DiaDiem;
[ApiController, Route("api/venues")]
public class DiaDiemController(DiaDiemService service) : ControllerBase
{
    [HttpGet, AllowAnonymous] public async Task<IActionResult> Get() => Ok(ApiResponse.Ok(await service.DanhSach()));
    [HttpGet("all"), Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> All() => Ok(ApiResponse.Ok(await service.DanhSach(true)));
    [HttpPost, Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> Tao(DiaDiemDto dto) => Ok(ApiResponse.Ok(await service.Tao(dto)));
    [HttpPut("{id:int}"), Authorize(Roles = "QuanTriVien")] public async Task<IActionResult> Sua(int id, DiaDiemDto dto) => Ok(ApiResponse.Ok(await service.Sua(id, dto)));
}
[ApiController, Route("api/venue-bookings"), Authorize]
public class DatDiaDiemController(DiaDiemService service) : ControllerBase
{
    [HttpPost, Authorize(Roles = "BanToChuc")] public async Task<IActionResult> Dat(DatDiaDiemDto dto) => Ok(ApiResponse.Ok(await service.Dat(User.LayId(), dto)));
    [HttpGet, Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> Get() => Ok(ApiResponse.Ok(await service.DanhSachDat()));
    [HttpPost("{id:int}/approve"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> Duyet(int id) { await service.XacNhan(id, true); return Ok(ApiResponse.Ok(new { message = "Đã xác nhận." })); }
    [HttpPost("{id:int}/reject"), Authorize(Roles = "QuanLySuKien,QuanTriVien")] public async Task<IActionResult> TuChoi(int id) { await service.XacNhan(id, false); return Ok(ApiResponse.Ok(new { message = "Đã từ chối." })); }
    [HttpPost("{id:int}/cancel"), Authorize(Roles = "BanToChuc")] public async Task<IActionResult> Huy(int id) { await service.Huy(User.LayId(), id); return Ok(ApiResponse.Ok(new { message = "Đã hủy." })); }
}
