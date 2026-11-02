using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Models;
using QLSK.Common.Exceptions;

namespace QLSK.Features.QuanTri;

[ApiController, Route("api/admin/catalog"), Authorize(Roles = "QuanTriVien")]
public class QuanTriController(QuanTriService dichVuQuanTri) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> LayDanhMucQuanTri() => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuQuanTri.LayDanhMucQuanTri()));
    [HttpPost("events")] public async Task<IActionResult> TaoSuKien(QuanTriSuKienDto duLieuQuanTriSuKien) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuQuanTri.TaoSuKienQuanTri(duLieuQuanTriSuKien)));
    [HttpPut("events/{id:int}")] public async Task<IActionResult> CapNhatSuKienQuanTri([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int suKienId, QuanTriSuKienDto duLieuQuanTriSuKien) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuQuanTri.CapNhatSuKienQuanTri(suKienId, duLieuQuanTriSuKien)));
    [HttpPost("organizers")] public async Task<IActionResult> TaoBanToChucQuanTri(QuanTriToChucDto duLieuQuanTriToChuc) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuQuanTri.TaoBanToChucQuanTri(duLieuQuanTriToChuc)));
    [HttpPut("organizers/{id:int}")] public async Task<IActionResult> CapNhatBanToChucQuanTri([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int banToChucId, QuanTriToChucDto duLieuQuanTriToChuc) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuQuanTri.CapNhatBanToChucQuanTri(banToChucId, duLieuQuanTriToChuc)));
    [HttpPut("accounts/{id:int}")] public async Task<IActionResult> CapNhatTaiKhoanQuanTri([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int taiKhoanId, QuanTriTaiKhoanDto duLieuQuanTriTaiKhoan)
    { await dichVuQuanTri.CapNhatTaiKhoanQuanTri(taiKhoanId, duLieuQuanTriTaiKhoan); return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã cập nhật tài khoản." })); }
    [HttpGet("accounts/{id:int}/activity")] public async Task<IActionResult> LayHoatDongTaiKhoan([Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int taiKhoanId) => Ok(ApiResponse.TaoPhanHoiThanhCong(await dichVuQuanTri.LaySuKienVaDonVeTheoTaiKhoan(taiKhoanId)));
    [HttpDelete("{kind}/{id:int}")] public async Task<IActionResult> XoaBanGhiChuaSuDung([Microsoft.AspNetCore.Mvc.FromRoute(Name = "kind")] string loaiDanhMuc, [Microsoft.AspNetCore.Mvc.FromRoute(Name = "id")] int banGhiId)
    {
        await (loaiDanhMuc switch
        {
            "events" => dichVuQuanTri.XoaSuKienChuaSuDung(banGhiId),
            "organizers" => dichVuQuanTri.XoaBanToChucChuaSuDung(banGhiId),
            "ticket-types" => dichVuQuanTri.XoaLoaiVeChuaSuDung(banGhiId),
            "venues" => dichVuQuanTri.XoaDiaDiemChuaSuDung(banGhiId),
            "event-types" => dichVuQuanTri.XoaLoaiSuKienChuaSuDung(banGhiId),
            _ => throw new ApiException(404, "NOT_FOUND", "Không tìm thấy bản ghi.")
        });
        return Ok(ApiResponse.TaoPhanHoiThanhCong(new { message = "Đã xóa bản ghi chưa sử dụng." }));
    }
}
