using System.Security.Claims;
using QLSK.Common.Exceptions;

namespace QLSK.Common.Auth;
public static class UserClaims
{
    public static int LayTaiKhoanId(this ClaimsPrincipal nguoiDungHienTai)
    {
        var chuoiTaiKhoanId = nguoiDungHienTai.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(chuoiTaiKhoanId, out var taiKhoanId) ? taiKhoanId : throw new ApiException(401, "AUTH_REQUIRED", "Chưa đăng nhập.");
    }
}
