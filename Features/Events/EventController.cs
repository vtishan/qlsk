using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Domain.Entities;

namespace QLSK.Features.Events;

public sealed record EventPublicationRequest(int VersionId, string? Reason);

[ApiController]
[Authorize]
[Route("api")]
public sealed class EventController(EventService events, ScheduleService schedule) : ControllerBase
{
    private TaiKhoan Actor => AccountAccess.Actor(HttpContext);

    [HttpPost("manage/events/{id:int}/publication")]
    public Task<EventWorkDto> Publish(int id, EventPublicationRequest request, CancellationToken ct) =>
        events.PublishInitial(Actor, id, request.VersionId, request.Reason, ct);

    [HttpPost("manage/events/{id:int}/publication/reopen")]
    public Task<EventWorkDto> Reopen(int id, EventPublicationRequest request, CancellationToken ct) =>
        events.ReopenInitial(Actor, id, request.VersionId, request.Reason, ct);

    [HttpGet("manage/events")]
    public Task<PageDto<EventWorkDto>> List(bool review, string? status, string? cursor, CancellationToken ct) =>
        events.List(Actor, review, status, cursor, ct);

    [HttpGet("manage/events/{id:int}")]
    public Task<EventWorkDto> Detail(int id, CancellationToken ct) => events.Detail(Actor, id, ct);

    [HttpPost("manage/events")]
    public async Task<IActionResult> Create(EventCreateRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await events.Create(Actor, request, ct)));

    [HttpPost("manage/events/{id:int}/suspensions")]
    public Task<EventWorkDto> Suspend(int id, DecisionRequest request, CancellationToken ct) =>
        events.Suspend(Actor, id, request, ct);

    [HttpPost("manage/events/{id:int}/completion")]
    public Task<EventWorkDto> Complete(int id, DecisionRequest request, CancellationToken ct) => events.Complete(Actor, id, request, ct);

    [HttpGet("manage/events/{id:int}/versions")]
    public Task<PageDto<VersionDto>> Versions(int id, string? status, string? cursor, CancellationToken ct) =>
        events.Versions(Actor, id, status, cursor, ct);

    [HttpGet("manage/events/{id:int}/versions/{versionId:int}")]
    public Task<VersionDto> Version(int id, int versionId, CancellationToken ct) =>
        events.Version(Actor, id, versionId, ct);

    [HttpPost("manage/events/{id:int}/versions")]
    public async Task<IActionResult> CreateVersion(int id, EventVersionRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await events.CreateVersion(Actor, id, request, ct)));

    [HttpPost("manage/events/{id:int}/versions/{versionId:int}/submissions")]
    public Task<VersionDto> Submit(int id, int versionId, DecisionRequest request, CancellationToken ct) =>
        events.Submit(Actor, id, versionId, request, ct);

    [HttpPost("manage/events/{id:int}/versions/{versionId:int}/decisions")]
    public Task<EventWorkDto> Decide(int id, int versionId, DecisionRequest request, CancellationToken ct) =>
        events.Decide(Actor, id, versionId, request, ct);

    [HttpGet("manage/events/{id:int}/sessions")]
    public Task<PageDto<SessionWorkDto>> Sessions(int id, string? cursor, CancellationToken ct) =>
        schedule.Sessions(Actor, id, cursor, ct);

    [HttpGet("manage/events/{id:int}/sessions/{sessionId:int}")]
    public Task<SessionWorkDto> Session(int id, int sessionId, CancellationToken ct) =>
        schedule.Session(Actor, id, sessionId, ct);

    [HttpPost("manage/events/{id:int}/sessions")]
    public async Task<IActionResult> CreateSession(int id, SessionChangeRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await schedule.ChangeSession(Actor, id, null, request, ct)));

    [HttpPut("manage/events/{id:int}/sessions/{sessionId:int}")]
    public async Task<SessionWorkDto> UpdateSession(int id, int sessionId, SessionChangeRequest request, CancellationToken ct)
    {
        await schedule.ChangeSession(Actor, id, sessionId, request, ct);
        return await schedule.Session(Actor, id, sessionId, ct);
    }

    [HttpGet("sessions/{sessionId:int}/ticket-types")]
    public Task<PageDto<TicketTypeWorkDto>> Types(int sessionId, string? cursor, CancellationToken ct) =>
        schedule.Types(Actor, sessionId, cursor, ct);

    [HttpGet("sessions/{sessionId:int}/ticket-types/{id:int}")]
    public Task<TicketTypeWorkDto> Type(int sessionId, int id, CancellationToken ct) =>
        schedule.Type(Actor, sessionId, id, ct);

    [HttpPost("sessions/{sessionId:int}/ticket-types")]
    public async Task<IActionResult> CreateType(int sessionId, TicketTypeChangeRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await schedule.ChangeType(Actor, sessionId, null, request, ct)));

    [HttpPut("sessions/{sessionId:int}/ticket-types/{id:int}")]
    public async Task<TicketTypeWorkDto> UpdateType(int sessionId, int id, TicketTypeChangeRequest request, CancellationToken ct)
    {
        await schedule.ChangeType(Actor, sessionId, id, request, ct);
        return await schedule.Type(Actor, sessionId, id, ct);
    }
}
