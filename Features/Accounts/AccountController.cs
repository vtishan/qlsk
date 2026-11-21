using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Features.Catalog;

namespace QLSK.Features.Accounts;

[ApiController]
[Route("api")]
public sealed class AccountController(AccountService service) : ControllerBase
{
    [EnableRateLimiting("qlsk-identity")]
    [HttpPost("auth/register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var id = await service.Register(request, ct);
        return StatusCode(StatusCodes.Status201Created, new CreatedDto(id));
    }

    [EnableRateLimiting("qlsk-identity")]
    [HttpPost("auth/session")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var account = await service.Login(request, ct);
        await AccountAccess.SignIn(HttpContext, account);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("auth/session")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await service.RevokeSessions(AccountAccess.Actor(HttpContext).Id, ct);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [EnableRateLimiting("qlsk-identity")]
    [HttpPost("auth/password-resets")]
    public async Task<IActionResult> Reset(ResetRequest request, CancellationToken ct)
    {
        await service.Reset(request, ct);
        return Accepted();
    }

    [EnableRateLimiting("qlsk-identity")]
    [HttpPost("auth/password-resets/confirm")]
    public async Task<IActionResult> ConfirmReset(ConfirmResetRequest request, CancellationToken ct)
    {
        await service.ConfirmReset(request, ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ProfileDto Profile() => AccountService.Profile(AccountAccess.Actor(HttpContext));

    [Authorize]
    [HttpPut("me")]
    public Task<ProfileDto> Update(ProfileRequest request, CancellationToken ct) =>
        service.Update(AccountAccess.Actor(HttpContext).Id, request, ct);

    [Authorize]
    [EnableRateLimiting("qlsk-identity")]
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var account = await service.ChangePassword(AccountAccess.Actor(HttpContext).Id, request, ct);
        await AccountAccess.SignIn(HttpContext, account);
        return NoContent();
    }

    [Authorize]
    [EnableRateLimiting("qlsk-identity")]
    [HttpPost("me/contact-verifications")]
    public async Task<IActionResult> RequestVerification(VerificationRequest request, CancellationToken ct)
    {
        await service.RequestVerification(AccountAccess.Actor(HttpContext).Id, request, ct);
        return Accepted();
    }

    [Authorize]
    [EnableRateLimiting("qlsk-identity")]
    [HttpPost("me/contact-verifications/confirm")]
    public async Task<IActionResult> ConfirmVerification(ConfirmVerificationRequest request, CancellationToken ct)
    {
        await service.ConfirmVerification(AccountAccess.Actor(HttpContext).Id, request, ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me/notifications")]
    public Task<PageDto<NotificationDto>> Notifications([FromQuery] string? cursor, CancellationToken ct) =>
        service.Notifications(AccountAccess.Actor(HttpContext).Id, cursor, ct);

    [Authorize]
    [HttpPut("me/notifications/{id:int}/read")]
    public async Task<IActionResult> ReadNotification(int id, CancellationToken ct)
    {
        await service.ReadNotification(AccountAccess.Actor(HttpContext).Id, id, ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me/favorites")]
    public Task<PageDto<EventCardDto>> Favorites(string? cursor, CancellationToken ct) =>
        service.Favorites(AccountAccess.Actor(HttpContext).Id, cursor, ct);

    [Authorize]
    [HttpGet("me/view-history")]
    public Task<PageDto<EventCardDto>> ViewHistory(CancellationToken ct) =>
        service.ViewHistory(AccountAccess.Actor(HttpContext).Id, ct);

    [Authorize]
    [HttpPut("me/view-history/{eventId:int}")]
    public async Task<IActionResult> RecordView(int eventId, CancellationToken ct)
    {
        await service.RecordView(AccountAccess.Actor(HttpContext).Id, eventId, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPut("me/favorites/{eventId:int}")]
    [HttpDelete("me/favorites/{eventId:int}")]
    public async Task<IActionResult> SetFavorite(int eventId, CancellationToken ct)
    {
        await service.SetFavorite(AccountAccess.Actor(HttpContext).Id, eventId, HttpMethods.IsPut(Request.Method), ct);
        return NoContent();
    }
}
