using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Domain.Entities;

namespace QLSK.Features.Finance;

[ApiController]
[Authorize]
[Route("api")]
public sealed class FinanceController(FinanceService service) : ControllerBase
{
    private TaiKhoan Actor => AccountAccess.Actor(HttpContext);

    [HttpGet("payments")]
    public Task<PageDto<PaymentDto>> Payments(string? status, string? cursor, CancellationToken ct) =>
        service.Payments(Actor, status, cursor, ct);

    [HttpGet("payments/{id:int}")]
    public Task<PaymentDto> Payment(int id, CancellationToken ct) => service.Payment(Actor, id, ct);

    [HttpGet("manage/contracts")]
    public Task<PageDto<ContractDto>> Contracts(string? cursor, CancellationToken ct) => service.Contracts(Actor, cursor, ct);

    [HttpGet("manage/contracts/{id:int}")]
    public Task<ContractDto> Contract(int id, CancellationToken ct) => service.Contract(Actor, id, ct);

    [HttpPost("manage/contracts")]
    public async Task<IActionResult> CreateContract(ContractCreateRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await service.CreateContract(Actor, request, ct)));

    [HttpGet("manage/settlements")]
    public Task<PageDto<SettlementDto>> Settlements(int? eventId, string? status, string? cursor, CancellationToken ct) =>
        service.Settlements(Actor, eventId, status, cursor, ct);

    [HttpPost("manage/settlements")]
    public async Task<IActionResult> CreateSettlement(SettlementCreateRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await service.CreateSettlement(Actor, request, ct)));

    [HttpGet("manage/settlements/eligible-events")]
    public Task<PageDto<DetailFieldDto>> EligibleSettlements(CancellationToken ct) => service.EligibleSettlements(Actor, ct);

    [HttpGet("manage/settlements/{id:int}")]
    public Task<SettlementDto> Settlement(int id, CancellationToken ct) => service.Settlement(Actor, id, ct);

    [HttpPost("manage/settlements/{id:int}/responses")]
    public Task<SettlementDto> Respond(int id, DecisionRequest request, CancellationToken ct) =>
        service.Respond(Actor, id, request, ct);

    [HttpPost("manage/settlements/{id:int}/approval")]
    public Task<SettlementDto> Approve(int id, DecisionRequest request, CancellationToken ct) =>
        service.Approve(Actor, id, request, ct);

    [HttpGet("manage/payouts")]
    public Task<PageDto<PayoutDto>> Payouts(string? status, string? cursor, CancellationToken ct) =>
        service.Payouts(Actor, status, cursor, ct);

    [HttpGet("manage/payouts/{id:int}")]
    public Task<PayoutDto> Payout(int id, CancellationToken ct) => service.Payout(Actor, id, ct);
}
