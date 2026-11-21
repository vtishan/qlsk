using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;

namespace QLSK.Features.Organizers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class OrganizerController(OrganizerService service) : ControllerBase
{
    [HttpGet("me/organizer-applications")]
    public Task<PageDto<ApplicationDto>> MyApplications([FromQuery] string? cursor, CancellationToken ct) =>
        service.List(AccountAccess.Actor(HttpContext), false, null, false, cursor, ct);

    [HttpPost("me/organizer-applications")]
    [RequestSizeLimit(24 * 1024 * 1024)]
    public async Task<IActionResult> Apply([FromForm] OrganizerSubmission request, CancellationToken ct)
    {
        var id = await service.Apply(AccountAccess.Actor(HttpContext), request, ct);
        return StatusCode(201, new CreatedDto(id));
    }

    [HttpGet("organizer-applications")]
    public Task<PageDto<ApplicationDto>> Queue([FromQuery] string? status, [FromQuery] bool overdue, [FromQuery] string? cursor, CancellationToken ct) =>
        service.List(AccountAccess.Actor(HttpContext), true, status, overdue, cursor, ct);

    [HttpGet("organizer-applications/{id:int}")]
    public Task<ApplicationDto> Detail(int id, CancellationToken ct) => service.Detail(AccountAccess.Actor(HttpContext), id, ct);

    [HttpGet("organizer-applications/{id:int}/documents/{documentId:int}")]
    public async Task<IActionResult> Document(int id, int documentId, CancellationToken ct)
    {
        var file = await service.Document(AccountAccess.Actor(HttpContext), id, documentId, ct);
        return File(file.Content, file.ContentType, file.Name);
    }

    [HttpPost("organizer-applications/{id:int}/decisions")]
    public Task<ApplicationDto> Decide(int id, DecisionRequest request, CancellationToken ct) =>
        service.Decide(AccountAccess.Actor(HttpContext), id, request, ct);
}
