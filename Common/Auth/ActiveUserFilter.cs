using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Models;
using QLSK.Data;

namespace QLSK.Common.Auth;
public class ActiveUserFilter(ApplicationDbContext db) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;
        var id = context.HttpContext.User.LayId();
        var account = await db.TaiKhoans.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.HoatDong, VaiTro = x.VaiTro.Ten, x.MatKhau })
            .SingleOrDefaultAsync();
        if (account is not { HoatDong: true })
        {
            context.Result = new ObjectResult(ApiResponse.Fail("ACCOUNT_LOCKED", "Tài khoản đã bị khóa.")) { StatusCode = 403 };
            return;
        }

        var tokenRole = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var tokenPassword = context.HttpContext.User.FindFirst(JwtService.PasswordFingerprintClaim)?.Value;
        if (!string.Equals(tokenRole, account.VaiTro, StringComparison.Ordinal) ||
            !string.Equals(tokenPassword, JwtService.PasswordFingerprint(account.MatKhau), StringComparison.Ordinal))
            context.Result = new ObjectResult(ApiResponse.Fail("TOKEN_STALE", "Thông tin đăng nhập đã thay đổi. Vui lòng đăng nhập lại.")) { StatusCode = 401 };
    }
}
