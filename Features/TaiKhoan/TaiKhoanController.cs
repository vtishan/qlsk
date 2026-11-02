using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Auth;
using QLSK.Common.Models;

namespace QLSK.Features.TaiKhoan;
[ApiController, Route("api/accounts"), Authorize]
public class TaiKhoanController(TaiKhoanService dichVuTaiKhoan) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> LayTaiKhoanCuaToi() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuTaiKhoan.LayTaiKhoanTheoId(User.LayTaiKhoanId())));

    [HttpPut("me")]
    public async Task<IActionResult> CapNhatHoTenTaiKhoan(SuaTaiKhoanDto duLieuSuaTaiKhoan) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuTaiKhoan.CapNhatHoTenTaiKhoan(User.LayTaiKhoanId(), duLieuSuaTaiKhoan)));

    [HttpGet, Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> LayDanhSachTaiKhoan([FromQuery(Name = "q")] string? tuKhoa, [FromQuery] int? vaiTroId, [FromQuery] bool? hoatDong, [FromQuery(Name = "page")] int soTrang = 1, [FromQuery(Name = "size")] int kichThuocTrang = 20)
        => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuTaiKhoan.LayDanhSachTaiKhoan(tuKhoa, vaiTroId, hoatDong, soTrang, kichThuocTrang)));

    [HttpPost("{id:int}/lock"), Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> KhoaTaiKhoan([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int taiKhoanId) { await dichVuTaiKhoan.KhoaTaiKhoan(taiKhoanId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã khóa tài khoản." })); }

    [HttpPost("{id:int}/unlock"), Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> MoKhoaTaiKhoan([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int taiKhoanId) { await dichVuTaiKhoan.MoKhoaTaiKhoan(taiKhoanId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã mở khóa tài khoản." })); }

    [HttpPut("{id:int}/role"), Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> DoiVaiTroTaiKhoan([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int taiKhoanId, DoiVaiTroDto duLieuDoiVaiTro) { await dichVuTaiKhoan.DoiVaiTroTaiKhoan(taiKhoanId, duLieuDoiVaiTro.VaiTroId); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã đổi vai trò." })); }
}
