using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Domain.Entities;

namespace QLSK.Features.Tickets;

[ApiController]
[Authorize]
[Route("api")]
public sealed class TicketController(TicketService tickets, OrderService orders) : ControllerBase
{
    private TaiKhoan Actor => AccountAccess.Actor(HttpContext);

    [HttpGet("me/tickets")]
    public Task<PageDto<TicketDto>> Mine([FromQuery] string? cursor, CancellationToken ct) => tickets.Mine(Actor, cursor, ct);

    [HttpGet("tickets/{id:int}")]
    public Task<TicketDto> Detail(int id, CancellationToken ct) => tickets.Detail(Actor, id, ct);

    [HttpGet("tickets/{id:int}/qr")]
    public async Task<IActionResult> Qr(int id, CancellationToken ct)
    {
        var file = await tickets.File(Actor, id, false, ct);
        return File(file.Content, file.ContentType);
    }

    [HttpGet("tickets/{id:int}/pdf")]
    public async Task<IActionResult> Pdf(int id, CancellationToken ct)
    {
        var file = await tickets.File(Actor, id, true, ct);
        return File(file.Content, file.ContentType, file.Filename);
    }

    [HttpGet("sessions/{id:int}/inventory")]
    public Task<InventoryDto> Inventory(int id, int? zoneId, CancellationToken ct) => orders.Inventory(Actor, id, zoneId, ct);

    [HttpGet("orders")]
    public Task<PageDto<OrderDto>> Orders(int? eventId, string? status, string? cursor, CancellationToken ct) =>
        orders.List(Actor, eventId, status, cursor, ct);

    [HttpGet("orders/{id:int}")]
    public Task<OrderDto> Order(int id, CancellationToken ct) => orders.Detail(Actor, id, ct);

    [HttpPost("orders")]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await orders.Place(Actor, request, ct)));

    [HttpPost("orders/{id:int}/cancel")]
    public Task<OrderDto> Cancel(int id, CancellationToken ct) => orders.Cancel(Actor, id, ct);

    [HttpPost("orders/{id:int}/checkout")]
    public Task<QLSK.Features.Requests.RentalCheckoutDto> Checkout(int id, CancellationToken ct) => orders.Checkout(Actor, id, ct);

    [HttpPost("orders/{id:int}/reconcile")]
    public Task<OrderDto> Reconcile(int id, CancellationToken ct) => orders.Reconcile(Actor, id, ct);
}
