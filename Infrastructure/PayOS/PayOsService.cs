using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QLSK.Common.Exceptions;

namespace QLSK.Infrastructure.PayOS;

public class PayOsService(
    HttpClient http,
    IOptions<PayOsOptions> options)
{
    private readonly PayOsOptions cfg = options.Value;

    public async Task<PayOsLink> Tao(
        long orderCode,
        decimal amount,
        string moTa,
        DateTime? expiredAt = null)
    {
        KiemTraSoTien(amount);

        if (ChuaCauHinh())
        {
            throw new ApiException(503, "PAYMENT_NOT_CONFIGURED", "Thanh toán trực tuyến hiện chưa khả dụng. Vui lòng thử lại sau.");
        }

        var money = decimal.ToInt64(amount);
        var desc = RutGonMoTa(moTa);

        var raw = TaoChuoiKy(
            orderCode,
            money,
            desc
        );

        var body = new
        {
            orderCode,
            amount = money,
            description = desc,
            returnUrl = cfg.ReturnUrl,
            cancelUrl = cfg.CancelUrl,
            expiredAt = ChuyenHanThanhToan(expiredAt),
            signature = Hmac(raw)
        };

        using var req = new HttpRequestMessage(
            HttpMethod.Post,
            "v2/payment-requests"
        )
        {
            Content = JsonContent.Create(body)
        };

        req.Headers.Add("x-client-id", cfg.ClientId);
        req.Headers.Add("x-api-key", cfg.ApiKey);

        using var res = await http.SendAsync(req);

        var text = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
        {
            throw new ApiException(
                502,
                "PAYMENT_GATEWAY_ERROR",
                "Không thể tạo link thanh toán payOS."
            );
        }

        using var doc = JsonDocument.Parse(text);

        var root = doc.RootElement;
        var code = root.GetProperty("code").GetString();

        if (code != "00")
        {
            throw new ApiException(
                502,
                "PAYMENT_GATEWAY_ERROR",
                "payOS từ chối tạo link thanh toán."
            );
        }

        var data = root.GetProperty("data");

        var checkoutUrl =
            data.GetProperty("checkoutUrl").GetString() ?? "";

        var paymentLinkId =
            data.TryGetProperty("paymentLinkId", out var link)
                ? link.GetString()
                : null;

        return new PayOsLink(
            checkoutUrl,
            paymentLinkId
        );
    }

    public PayOsWebhookData Verify(PayOsWebhook webhook)
    {
        KiemTraChecksumKey();

        var raw = TaoChuoiWebhook(webhook.Data);
        var expected = Hmac(raw);

        var hopLe = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected.ToLowerInvariant()),
            Encoding.UTF8.GetBytes(
                webhook.Signature.ToLowerInvariant()
            )
        );

        if (!hopLe)
        {
            throw new ApiException(
                400,
                "PAYMENT_VERIFICATION_FAILED",
                "Webhook payOS không hợp lệ."
            );
        }

        var data = webhook.Data;

        var orderCode =
            data.GetProperty("orderCode").GetInt64();

        var amount =
            data.GetProperty("amount").GetDecimal();

        var reference =
            data.TryGetProperty("reference", out var r)
                ? r.GetString() ?? ""
                : "";

        var paymentLinkId =
            data.TryGetProperty("paymentLinkId", out var link)
                ? link.GetString()
                : null;

        var code =
            data.TryGetProperty("code", out var c)
                ? c.GetString() ?? ""
                : "";

        var desc =
            data.TryGetProperty("desc", out var d)
                ? d.GetString() ?? ""
                : "";

        return new PayOsWebhookData(
            orderCode,
            amount,
            reference,
            paymentLinkId,
            code,
            desc
        );
    }

    private void KiemTraSoTien(decimal amount)
    {
        if (amount <= 0 || amount != decimal.Truncate(amount))
        {
            throw new ApiException(
                400,
                "PAYMENT_INVALID",
                "Số tiền thanh toán phải là số VND nguyên dương."
            );
        }
    }

    private bool ChuaCauHinh()
    {
        return
            string.IsNullOrWhiteSpace(cfg.ClientId) ||
            string.IsNullOrWhiteSpace(cfg.ApiKey) ||
            string.IsNullOrWhiteSpace(cfg.ChecksumKey);
    }

    private static string RutGonMoTa(string moTa)
    {
        return moTa.Length > 25
            ? moTa[..25]
            : moTa;
    }

    private string TaoChuoiKy(
        long orderCode,
        long amount,
        string description)
    {
        return
            $"amount={amount}" +
            $"&cancelUrl={cfg.CancelUrl}" +
            $"&description={description}" +
            $"&orderCode={orderCode}" +
            $"&returnUrl={cfg.ReturnUrl}";
    }

    private static long? ChuyenHanThanhToan(
        DateTime? expiredAt)
    {
        if (!expiredAt.HasValue)
        {
            return null;
        }

        return new DateTimeOffset(
            expiredAt.Value
        ).ToUnixTimeSeconds();
    }

    private void KiemTraChecksumKey()
    {
        if (string.IsNullOrWhiteSpace(cfg.ChecksumKey))
        {
            throw new ApiException(
                503,
                "PAYMENT_NOT_CONFIGURED",
                "Chưa cấu hình checksum key payOS."
            );
        }
    }

    private string TaoChuoiWebhook(JsonElement data)
    {
        var pairs = new List<string>();

        foreach (var p in data
            .EnumerateObject()
            .OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            pairs.Add(
                $"{p.Name}={ToValue(p.Value)}"
            );
        }

        return string.Join("&", pairs);
    }

    private string Hmac(string raw)
    {
        var key = Encoding.UTF8.GetBytes(
            cfg.ChecksumKey
        );

        var data = Encoding.UTF8.GetBytes(raw);

        using var hmac = new HMACSHA256(key);

        var hash = hmac.ComputeHash(data);

        return Convert
            .ToHexString(hash)
            .ToLowerInvariant();
    }

    private static string ToValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null
                or JsonValueKind.Undefined
                => "",

            JsonValueKind.True
                => "true",

            JsonValueKind.False
                => "false",

            JsonValueKind.Number
                => value.GetRawText(),

            JsonValueKind.String
                => value.GetString() ?? "",

            JsonValueKind.Array
                => JsonSerializer.Serialize(
                    value
                        .EnumerateArray()
                        .Select(x =>
                            x.ValueKind == JsonValueKind.Object
                                ? (object)SortObject(x)
                                : x
                        )
                ),

            JsonValueKind.Object
                => JsonSerializer.Serialize(
                    SortObject(value)
                ),

            _ => value.ToString()
        };
    }

    private static SortedDictionary<string, object?>
        SortObject(JsonElement value)
    {
        var result =
            new SortedDictionary<string, object?>(
                StringComparer.Ordinal
            );

        foreach (var p in value
            .EnumerateObject()
            .OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            result[p.Name] =
                p.Value.ValueKind == JsonValueKind.String
                    ? p.Value.GetString()
                    : p.Value.GetRawText();
        }

        return result;
    }
}
