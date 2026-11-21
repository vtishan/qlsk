using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Features.Requests;

namespace QLSK.Features.Tickets;

[ApiController]
public sealed class OrderWebhookController(OrderService orders) : ControllerBase
{
    [AllowAnonymous]
    [RentalPayOsWebhook]
    [RequestSizeLimit(32768)]
    [HttpPost("api/orders/payos/webhook")]
    public async Task<IActionResult> Webhook(JsonElement payload, CancellationToken ct)
    {
        await orders.PaymentWebhook(payload, ct);
        return Ok(new { code = "00", desc = "success" });
    }
}
