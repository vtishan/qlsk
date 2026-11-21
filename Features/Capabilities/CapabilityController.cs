using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Security;

namespace QLSK.Features.Capabilities;

[ApiController]
[Route("api/capabilities")]
public sealed class CapabilityController(CapabilityService service, IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public Task<CapabilityDto> Get(CancellationToken ct)
    {
        var token = antiforgery.GetAndStoreTokens(HttpContext);
        return service.Get(AccountAccess.Current(HttpContext), token.RequestToken, ct);
    }
}
