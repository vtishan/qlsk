using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.Common.Security;

internal static class AccountAccess
{
    internal const string ActorKey = "QLSK.Actor";
    internal const string VersionClaim = "qlsk-session-version";

    public static TaiKhoan? Current(HttpContext context) => context.Items[ActorKey] as TaiKhoan;
    public static TaiKhoan Actor(HttpContext context) => Current(context) ?? throw ApiRequest.SignIn();
    public static bool Admin(TaiKhoan? actor) => actor?.VaiTro == VaiTroTaiKhoan.QuanTriVien;

    public static bool Platform(TaiKhoan? actor, string permission)
    {
        if (Admin(actor)) return true;
        if (actor?.VaiTro != VaiTroTaiKhoan.QuanLyHeThong || string.IsNullOrEmpty(actor.QuyenQuanLy)) return false;
        try
        {
            using var doc = JsonDocument.Parse(actor.QuyenQuanLy);
            return doc.RootElement.TryGetProperty(permission, out var result) && result.ValueKind == JsonValueKind.True;
        }
        catch (JsonException) { return false; }
    }

    public static void NeedPlatform(TaiKhoan? actor, string permission)
    {
        if (!Platform(actor, permission)) throw ApiRequest.Denied();
    }

    public static string RoleLabel(VaiTroTaiKhoan role) => role switch
    {
        VaiTroTaiKhoan.BanToChuc => "Ban tổ chức",
        VaiTroTaiKhoan.QuanLySuKien => "Nhân sự Quản lý sự kiện",
        VaiTroTaiKhoan.QuanLyHeThong => "Quản lý hệ thống",
        VaiTroTaiKhoan.QuanTriVien => "Admin",
        _ => "Người dùng"
    };

    public static Task SignIn(HttpContext context, TaiKhoan actor)
    {
        const string scheme = CookieAuthenticationDefaults.AuthenticationScheme;
        var identity = new ClaimsIdentity([
            new(ClaimTypes.NameIdentifier, actor.Id.ToString()),
            new(VersionClaim, actor.PhienBanDangNhap.ToString())
        ], scheme);
        return context.SignInAsync(scheme, new ClaimsPrincipal(identity));
    }
}

internal sealed class SessionCookie(AppDbContext db) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var identity = context.Principal;
        if (!int.TryParse(identity?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ||
            !int.TryParse(identity.FindFirstValue(AccountAccess.VersionClaim), out var version))
        {
            context.RejectPrincipal();
            return;
        }
        var actor = await db.TaiKhoan.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, context.HttpContext.RequestAborted);
        if (actor is null || actor.TrangThai == TrangThaiTaiKhoan.DaKhoa || actor.PhienBanDangNhap != version)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }
        context.HttpContext.Items[AccountAccess.ActorKey] = actor;
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    }
}
