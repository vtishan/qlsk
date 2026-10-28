using System.Text.Json;
namespace QLSK.Infrastructure.PayOS;
public record PayOsLink(string CheckoutUrl, string? PaymentLinkId);
public class PayOsWebhook
{
    public string Code { get; set; } = "";
    public string Desc { get; set; } = "";
    public bool Success { get; set; }
    public JsonElement Data { get; set; }
    public string Signature { get; set; } = "";
}
public record PayOsWebhookData(long OrderCode, decimal Amount, string Reference, string? PaymentLinkId, string Code, string Desc);
