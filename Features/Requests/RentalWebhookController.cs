using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Features.Tickets;

namespace QLSK.Features.Requests;

// Only this signed provider endpoint bypasses browser antiforgery validation.
[AttributeUsage(AttributeTargets.Method)]
public sealed class RentalPayOsWebhookAttribute : Attribute;

[ApiController]
public sealed class RentalWebhookController(RequestService requests, OrderService orders, RentalPayOs payos) : ControllerBase
{
    [AllowAnonymous]
    [RentalPayOsWebhook]
    [RequestSizeLimit(32768)]
    [HttpPost("api/venue-rentals/payos/webhook")]
    [HttpPost("api/payments/payos/webhook")]
    public async Task<IActionResult> Webhook(JsonElement payload, CancellationToken ct)
    {
        var verified = payos.VerifyWebhook(payload);
        // PayOS confirms a configured callback with its signed sample order (not a merchant payment).
        if (verified.Code == 123 && payload.GetProperty("data").TryGetProperty("amount", out var sampleAmount) &&
            sampleAmount.TryGetDecimal(out var sample) && sample == 3000)
            return Ok(new { code = "00", desc = "success" });
        if (verified.Code is > 2_000_000_000L and <= 4_147_483_647L) await orders.PaymentWebhook(payload, ct);
        else await requests.DepositWebhook(payload, ct);
        return Ok(new { code = "00", desc = "success" });
    }
}
