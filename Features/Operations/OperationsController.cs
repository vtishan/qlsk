using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Domain.Entities;

namespace QLSK.Features.Operations;

[ApiController]
[Authorize]
[Route("api")]
public sealed class OperationsController(OperationsService service) : ControllerBase
{
    private TaiKhoan Actor => AccountAccess.Actor(HttpContext);

    [HttpGet("manage/scopes")]
    public Task<ScopeDto> Scope(CancellationToken ct) => service.Scope(Actor, ct);

    [HttpGet("admin/dashboard")]
    public Task<DashboardDto> Dashboard(CancellationToken ct) => service.Dashboard(Actor, ct);

    [HttpGet("admin/accounts")]
    public Task<PageDto<AdminAccountDto>> Accounts(string? status, string? cursor, CancellationToken ct) =>
        service.Accounts(Actor, status, cursor, ct);

    [HttpGet("admin/accounts/{id:int}")]
    public Task<AdminAccountDto> Account(int id, CancellationToken ct) => service.Account(Actor, id, ct);

    [HttpPut("admin/accounts/{id:int}")]
    public Task<AdminAccountDto> ChangeAccount(int id, AccountAdminRequest request, CancellationToken ct) =>
        service.ChangeAccount(Actor, id, request, ct);

    [HttpGet("admin/settings")]
    public async Task<IActionResult> Settings(string? key, CancellationToken ct) => key == null
        ? Ok(await service.Settings(Actor, ct)) : Ok(await service.Setting(Actor, key, ct));

    [HttpPut("admin/settings")]
    public Task<SettingDto> Set(SettingRequest request, CancellationToken ct) => service.Set(Actor, request, ct);

    [HttpGet("admin/audit")]
    public Task<PageDto<AuditDto>> Audit(string? cursor, CancellationToken ct) => service.Audit(Actor, cursor, ct);

    [HttpGet("admin/audit/{id:int}")]
    public Task<AuditDto> AuditRecord(int id, CancellationToken ct) => service.AuditRecord(Actor, id, ct);

    [HttpGet("admin/monitoring")]
    public Task<PageDto<ServiceMonitorDto>> Monitoring(CancellationToken ct) => service.Monitoring(Actor, ct);
}
