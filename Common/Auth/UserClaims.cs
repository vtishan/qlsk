using System.Security.Claims;
using QLSK.Common.Exceptions;

namespace QLSK.Common.Auth;
public static class UserClaims
{
    public static int LayId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : throw new ApiException(401, "AUTH_REQUIRED", "Chưa đăng nhập.");
    }
}
