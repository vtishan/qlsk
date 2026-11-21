using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QLSK.Common.Http;

namespace QLSK.Features.Requests;

// Provider credentials and return URLs are deployment settings; callers cannot override them.
public sealed class RentalPayOs(HttpClient client, IConfiguration config)
{
    private string? Setting(string key) => config["QLSK:PayOS:" + key];
    public bool Configured => new[] { "ClientId", "ApiKey", "ChecksumKey" }.All(k => !string.IsNullOrWhiteSpace(Setting(k)))
        && SafeReturn(Setting("ReturnUrl")) && SafeReturn(Setting("CancelUrl"));
    private static bool SafeReturn(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == "https";

    public async Task<string> Checkout(long code, decimal amount, DateTime deadline, CancellationToken ct, string descriptionPrefix = "COC")
    {
        NeedConfigured();
        if (descriptionPrefix is not ("COC" or "VE")) throw ApiRequest.Invalid("Loại thanh toán không hợp lệ.");
        var description = descriptionPrefix + " " + code.ToString(CultureInfo.InvariantCulture);
        var cancel = Setting("CancelUrl")!;
        var returns = Setting("ReturnUrl")!;
        var data = new SortedDictionary<string, string>(StringComparer.Ordinal) {
            ["amount"] = Amount(amount), ["cancelUrl"] = cancel, ["description"] = description,
            ["orderCode"] = code.ToString(CultureInfo.InvariantCulture), ["returnUrl"] = returns
        };
        using var request = Request(HttpMethod.Post, "https://api-merchant.payos.vn/v2/payment-requests");
        request.Content = JsonContent.Create(new { orderCode = code, amount = long.Parse(Amount(amount), CultureInfo.InvariantCulture),
            description, cancelUrl = cancel, returnUrl = returns, expiredAt = new DateTimeOffset(DateTime.SpecifyKind(deadline, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            signature = Sign(string.Join('&', data.Select(x => x.Key + "=" + x.Value))) });
        var response = await Send(request, ct);
        Verify(response);
        var body = response.GetProperty("data");
        if (body.GetProperty("orderCode").GetInt64() != code || body.GetProperty("amount").GetDecimal() != amount)
            throw new ApiException(503, "Thông tin thanh toán từ PayOS không khớp.");
        var url = body.GetProperty("checkoutUrl").GetString();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https" ||
            !(uri.Host == "pay.payos.vn" || uri.Host.EndsWith(".payos.vn", StringComparison.OrdinalIgnoreCase)))
            throw new ApiException(503, "Liên kết PayOS không hợp lệ.");
        return uri.AbsoluteUri;
    }

    public async Task<PayOsStatus> Status(long code, CancellationToken ct)
    {
        NeedConfigured();
        using var request = Request(HttpMethod.Get, "https://api-merchant.payos.vn/v2/payment-requests/" + code.ToString(CultureInfo.InvariantCulture));
        var response = await Send(request, ct);
        // This authenticated provider API is queried server-to-server over TLS. Creation is checksum signed.
        if (response.TryGetProperty("signature", out _)) Verify(response);
        var data = response.GetProperty("data");
        if (data.GetProperty("orderCode").GetInt64() != code) throw new ApiException(503, "Mã đối soát PayOS không khớp.");
        var transactions = data.GetProperty("transactions").EnumerateArray().ToArray();
        var reference = transactions.Length == 1 ? transactions[0].GetProperty("reference").GetString() : null;
        var paidAt = transactions.Length == 1 ? PaymentTime(transactions[0].GetProperty("transactionDateTime").GetString()) : null;
        return new(data.GetProperty("status").GetString() ?? "", data.GetProperty("amount").GetDecimal(),
            data.GetProperty("amountPaid").GetDecimal(), reference, paidAt);
    }

    private static DateTime? PaymentTime(string? value)
    {
        // Bank statements may use a Vietnam-local timestamp without an offset.
        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var time)) return null;
        return time.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(time, TimeSpan.FromHours(7)).UtcDateTime : time.ToUniversalTime();
    }

    public async Task<string?> ExistingCheckout(long code, decimal amount, CancellationToken ct)
    {
        NeedConfigured();
        using var request = Request(HttpMethod.Get, "https://api-merchant.payos.vn/v2/payment-requests/" + code.ToString(CultureInfo.InvariantCulture));
        using var response = await client.SendAsync(request, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode) throw new ApiException(503, "Chưa kiểm tra được mã thanh toán cọc. Vui lòng thử lại.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = json.RootElement;
        // PayOS uses 231 for a payment link that has not been created yet.
        if (root.GetProperty("code").GetString() == "231") return null;
        if (root.GetProperty("code").GetString() != "00") throw new ApiException(503, "Chưa kiểm tra được thanh toán cọc PayOS.");
        if (root.TryGetProperty("signature", out _)) Verify(root);
        var data = root.GetProperty("data");
        if (data.GetProperty("orderCode").GetInt64() != code || data.GetProperty("amount").GetDecimal() != amount ||
            !Guid.TryParseExact(data.GetProperty("id").GetString(), "N", out var link))
            throw new ApiException(503, "Thông tin liên kết thanh toán PayOS không khớp.");
        return "https://pay.payos.vn/web/" + link.ToString("N");
    }

    private void NeedConfigured()
    {
        if (!Configured) throw new ApiException(503, "Chưa cấu hình thanh toán PayOS cho tiền cọc.");
    }
    public (long Code, string EventId) VerifyWebhook(JsonElement payload)
    {
        NeedConfigured();
        try
        {
            Verify(payload);
            var data = payload.GetProperty("data");
            if (data.GetProperty("code").GetString() != "00" || data.GetProperty("currency").GetString() != "VND")
                throw ApiRequest.Invalid("Webhook không phải khoản thu VND thành công.");
            var code = data.GetProperty("orderCode").GetInt64();
            var reference = data.GetProperty("reference").GetString();
            if (code <= 0 || string.IsNullOrWhiteSpace(reference) || reference.Length > 100)
                throw ApiRequest.Invalid("Webhook thiếu mã giao dịch.");
            return (code, "payos:" + reference);
        }
        catch (ApiException) { throw; }
        catch (Exception error) when (error is KeyNotFoundException or InvalidOperationException or FormatException)
        { throw ApiRequest.Invalid("Webhook PayOS không đúng định dạng."); }
    }
    private HttpRequestMessage Request(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("x-client-id", Setting("ClientId"));
        request.Headers.Add("x-api-key", Setting("ApiKey"));
        return request;
    }
    private async Task<JsonElement> Send(HttpRequestMessage request, CancellationToken ct)
    {
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) throw new ApiException(503, "PayOS chưa phản hồi thành công. Vui lòng thử đối soát lại.");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        if (document.RootElement.GetProperty("code").GetString() != "00")
            throw new ApiException(503, "PayOS chưa hoàn tất yêu cầu thanh toán. Vui lòng thử lại.");
        return document.RootElement.Clone();
    }
    private void Verify(JsonElement response)
    {
        var signature = response.GetProperty("signature").GetString() ?? "";
        var canonical = string.Join('&', response.GetProperty("data").EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => p.Name + "=" + (p.Value.ValueKind == JsonValueKind.Null ? "" : p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() : p.Value.GetRawText())));
        var expected = Sign(canonical);
        if (signature.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(signature.ToLowerInvariant()), Encoding.ASCII.GetBytes(expected)))
            throw new ApiException(503, "Chữ ký phản hồi PayOS không hợp lệ.");
    }
    private string Sign(string data) => Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(Setting("ChecksumKey")!), Encoding.UTF8.GetBytes(data)));
    private static string Amount(decimal amount) => amount > 0 && decimal.Truncate(amount) == amount && amount <= long.MaxValue
        ? amount.ToString("0", CultureInfo.InvariantCulture) : throw new ApiException(422, "Tiền cọc phải là số nguyên VND dương.");
}

public sealed record PayOsStatus(string Status, decimal Amount, decimal Paid, string? Reference, DateTime? PaidAt);
public sealed record RentalCheckoutDto(int PaymentId, string CheckoutUrl);
