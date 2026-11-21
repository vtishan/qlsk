using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Domain.Entities;

namespace QLSK.Features.Requests;

[ApiController]
[Authorize]
[Route("api")]
public sealed class RequestController(RequestService service, RefundService refunds) : ControllerBase
{
    private TaiKhoan Actor => AccountAccess.Actor(HttpContext);

    [HttpGet("venue-rentals")]
    public Task<PageDto<RentalDto>> Rentals(string? scope, string? status, string? cursor, CancellationToken ct) =>
        service.Rentals(Actor, scope == "review", status, cursor, ct);

    [HttpGet("venue-rentals/{id:int}")]
    public Task<RentalDto> Rental(int id, CancellationToken ct) => service.Rental(Actor, id, ct);

    [HttpPost("venue-rentals")]
    public async Task<IActionResult> CreateRental(RentalCreateRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await service.CreateRental(Actor, request, ct)));

    [HttpPost("venue-rentals/{id:int}/decisions")]
    public Task<RentalDto> DecideRental(int id, DecisionRequest request, CancellationToken ct) =>
        service.DecideRental(Actor, id, request, ct);

    [HttpPost("venue-rentals/{id:int}/cancellations")]
    public Task<RentalDto> CancelRental(int id, CancelRentalRequest request, CancellationToken ct) =>
        service.CancelRental(Actor, id, request, ct);

    [HttpPost("venue-rentals/{id:int}/deposit")]
    public Task<RentalCheckoutDto> Deposit(int id, CancellationToken ct) => service.Deposit(Actor, id, ct);

    [HttpPost("venue-rentals/{id:int}/deposit/reconciliation")]
    public Task<RentalDto> ReconcileDeposit(int id, CancellationToken ct) => service.ReconcileDeposit(Actor, id, ct);

    [HttpGet("support-cases")]
    public Task<PageDto<SupportDto>> Cases(string? scope, string? status, string? cursor, CancellationToken ct) =>
        service.Cases(Actor, scope == "review", status, cursor, ct);

    [HttpGet("support-cases/{id:int}")]
    public Task<SupportDto> Case(int id, CancellationToken ct) => service.Case(Actor, id, ct);

    [HttpPost("support-cases")]
    public async Task<IActionResult> CreateCase(SupportCreateRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await service.CreateCase(Actor, request, ct)));

    [HttpPut("support-cases/{id:int}")]
    public Task<SupportDto> UpdateCase(int id, DecisionRequest request, CancellationToken ct) =>
        service.UpdateCase(Actor, id, request, ct);

    [HttpGet("refund-requests")]
    public Task<PageDto<RefundDto>> Refunds(string? scope, string? status, string? cursor, CancellationToken ct) =>
        refunds.List(Actor, scope == "review", status, cursor, ct);

    [HttpGet("me/refundable-tickets")]
    public Task<PageDto<RefundEligibleDto>> RefundEligible(CancellationToken ct) => refunds.Eligible(Actor, ct);

    [HttpPost("refund-requests")]
    public async Task<IActionResult> CreateRefund(RefundCreateRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await refunds.Create(Actor, request, ct)));

    [HttpGet("refund-requests/{id:int}")]
    public Task<RefundDto> Refund(int id, CancellationToken ct) => refunds.Detail(Actor, id, ct);

    [HttpPost("refund-requests/{id:int}/decisions")]
    public Task<RefundDto> DecideRefund(int id, DecisionRequest request, CancellationToken ct) =>
        refunds.Decide(Actor, id, request, ct);
}
