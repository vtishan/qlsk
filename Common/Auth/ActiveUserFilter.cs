using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Models;
using QLSK.Data;

namespace QLSK.Common.Auth;
public class ActiveUserFilter(ApplicationDbContext nguCanhDuLieu) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext nguCanhPhanQuyen)
    {
        if (nguCanhPhanQuyen.HttpContext.User.Identity?.IsAuthenticated != true) return;
        var taiKhoanId = nguCanhPhanQuyen.HttpContext.User.LayTaiKhoanId();
        var thongTinXacThucTaiKhoan = await nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking()
            .Where(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId)
            .Select(taiKhoanDb => new { taiKhoanDb.HoatDong, VaiTro = taiKhoanDb.VaiTro.TenVaiTro, MatKhau = taiKhoanDb.MatKhauBam })
            .SingleOrDefaultAsync();
        if (thongTinXacThucTaiKhoan is not { HoatDong: true })
        {
            nguCanhPhanQuyen.Result = new ObjectResult(ApiResponse.TaoPhanHoiThatBai("ACCOUNT_LOCKED", "Tài khoản đã bị khóa.")) { StatusCode = 403 };
            return;
        }

        var vaiTroTrongToken = nguCanhPhanQuyen.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var dauVanTayMatKhauTrongToken = nguCanhPhanQuyen.HttpContext.User.FindFirst(JwtService.PasswordFingerprintClaim)?.Value;
        if (!string.Equals(vaiTroTrongToken, thongTinXacThucTaiKhoan.VaiTro, StringComparison.Ordinal) ||
            !string.Equals(dauVanTayMatKhauTrongToken, JwtService.TaoDauVanTayMatKhau(thongTinXacThucTaiKhoan.MatKhau), StringComparison.Ordinal))
            nguCanhPhanQuyen.Result = new ObjectResult(ApiResponse.TaoPhanHoiThatBai("TOKEN_STALE", "Thông tin đăng nhập đã thay đổi. Vui lòng đăng nhập lại.")) { StatusCode = 401 };
    }
}
