using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Domain.Entities;

namespace QLSK.Features.Tickets;

[ApiController]
[Authorize]
[Route("api/manage/vouchers")]
public sealed class VoucherController(VoucherService service) : ControllerBase
{
    private TaiKhoan Actor => AccountAccess.Actor(HttpContext);
    [HttpGet]
    public Task<PageDto<VoucherWorkDto>> List(int? eventId, string? status, string? cursor, CancellationToken ct) =>
        service.List(Actor, eventId, status, cursor, ct);
    [HttpGet("{id:int}")]
    public Task<VoucherWorkDto> Detail(int id, CancellationToken ct) => service.Detail(Actor, id, ct);
}
