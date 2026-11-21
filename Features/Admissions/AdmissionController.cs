using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Domain.Entities;

namespace QLSK.Features.Admissions;

[ApiController]
[Authorize]
[Route("api")]
public sealed class AdmissionController(AdmissionService service, StaffGrantService grants) : ControllerBase
{
    private TaiKhoan Actor => AccountAccess.Actor(HttpContext);

    [HttpPost("events/{id:int}/invitations/{invitationId:int}/links")]
    public Task<InvitationLinkDto> InvitationLink(int id, int invitationId, CancellationToken ct) => service.InvitationLink(Actor, id, invitationId, ct);

    [HttpPost("invitations/resolve")]
    public Task<InvitationResolveDto> ResolveInvitation(InvitationTokenRequest request, CancellationToken ct) => service.ResolveInvitation(Actor, request, ct);

    [HttpPost("invitations/claim")]
    public Task<CreatedDto> ClaimInvitation(InvitationTokenRequest request, CancellationToken ct) => service.ClaimInvitation(Actor, request, ct);

    [HttpGet("events/{id:int}/invitations/options")]
    public Task<InvitationOptionsDto> InvitationOptions(int id, int sessionId, CancellationToken ct) => service.InvitationOptions(Actor, id, sessionId, ct);

    [HttpGet("events/{id:int}/invitations")]
    public Task<PageDto<InvitationDto>> Invitations(int id, string? status, string? cursor, CancellationToken ct) => service.Invitations(Actor, id, status, cursor, ct);

    [HttpGet("events/{id:int}/invitations/{invitationId:int}")]
    public Task<InvitationDto> Invitation(int id, int invitationId, CancellationToken ct) => service.Invitation(Actor, id, invitationId, ct);

    [HttpPost("events/{id:int}/invitations")]
    public async Task<IActionResult> IssueInvitation(int id, InvitationRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await service.IssueInvitation(Actor, id, request, ct)));

    [HttpPost("invitations/{id:int}/revoke")]
    public async Task<IActionResult> RevokeInvitation(int id, DecisionRequest request, CancellationToken ct)
    {
        await service.RevokeInvitation(Actor, id, request.Reason, ct);
        return NoContent();
    }

    [HttpGet("events/{id:int}/invitations/{invitationId:int}/pdf")]
    public async Task<IActionResult> InvitationFile(int id, int invitationId, CancellationToken ct)
    {
        var file = await service.InvitationFile(Actor, id, invitationId, ct);
        return File(file.Content, file.ContentType, file.Filename);
    }

    [HttpPost("sessions/{id:int}/check-ins")]
    public Task<CheckInDto> CheckIn(int id, CheckInRequest request, CancellationToken ct) => service.CheckIn(Actor, id, request, ct);

    [HttpPost("sessions/{id:int}/attendees/lookup")]
    public Task<PageDto<AttendeeDto>> Lookup(int id, AttendeeLookupRequest request, CancellationToken ct) =>
        service.Lookup(Actor, id, request, ct);

    [HttpGet("sessions/{id:int}/attendees")]
    public Task<PageDto<AttendeeDto>> Attendees(int id, string? status, string? cursor, CancellationToken ct) =>
        service.Attendees(Actor, id, status, cursor, ct);

    [HttpGet("sessions/{id:int}/attendees/{ticketId:int}")]
    public Task<AttendeeDto> Attendee(int id, int ticketId, CancellationToken ct) => service.Attendee(Actor, id, ticketId, ct);

    [HttpGet("sessions/{id:int}/check-ins")]
    public Task<PageDto<CheckInLogDto>> Log(int id, string? cursor, CancellationToken ct) => service.Log(Actor, id, cursor, ct);

    [HttpGet("sessions/{id:int}/check-ins/{entryId:int}")]
    public Task<CheckInLogDto> LogEntry(int id, int entryId, CancellationToken ct) => service.LogEntry(Actor, id, entryId, ct);

    [HttpGet("manage/events/{id:int}/staff-grants")]
    public Task<PageDto<StaffGrantDto>> Staff(int id, string? status, string? cursor, CancellationToken ct) =>
        grants.List(Actor, id, status, cursor, ct);

    [HttpGet("manage/events/{id:int}/staff-grants/{grantId:int}")]
    public Task<StaffGrantDto> StaffMember(int id, int grantId, CancellationToken ct) => grants.Detail(Actor, id, grantId, ct);

    [HttpPost("manage/events/{id:int}/staff-grants")]
    public async Task<IActionResult> GiveStaffGrant(int id, StaffGrantRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await grants.Give(Actor, id, request, ct)));

    [HttpDelete("manage/events/{id:int}/staff-grants/{grantId:int}")]
    public async Task<IActionResult> RevokeStaffGrant(int id, int grantId, CancellationToken ct)
    {
        await grants.Revoke(Actor, id, grantId, ct);
        return NoContent();
    }
}
