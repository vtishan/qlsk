using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QLSK.Common.Exceptions;
using QLSK.Infrastructure.PayOS;
using Xunit;

namespace QLSK.Tests;

public sealed class PayOsHardeningTests
{
    private static PayOsService Service(HttpMessageHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("https://api-merchant.payos.vn/") },
        Options.Create(new PayOsOptions { ClientId = "test", ApiKey = "test", ChecksumKey = "test-checksum-key" }));

    [Theory]
    [InlineData(false, "https://pay.payos.vn/web/link123", 100000)]
    [InlineData(true, "https://attacker.example/pay", 100000)]
    [InlineData(true, "https://pay.payos.vn/web/link123", 1)]
    public async Task Gateway_responses_require_signature_trusted_host_and_expected_amount(bool signed, string checkout, int amount)
    {
        var data = JsonSerializer.SerializeToElement(new { amount, checkoutUrl = checkout, currency = "VND", orderCode = 7, paymentLinkId = "link123" });
        using var handler = new Handler(_ => Response(data, signed));
        var error = await Assert.ThrowsAsync<ApiException>(() => Service(handler).TaoLienKetThanhToan(7, 100000, "DON 7"));
        Assert.Equal(502, error.MaTrangThaiHttp);
    }

    [Fact]
    public async Task Gateway_timeout_is_temporary_unavailability()
    {
        using var handler = new Handler(_ => throw new TaskCanceledException("test timeout"));
        var error = await Assert.ThrowsAsync<ApiException>(() => Service(handler).TaoLienKetThanhToan(7, 100000, "DON 7"));
        Assert.Equal(503, error.MaTrangThaiHttp);
        Assert.DoesNotContain("test timeout", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Recovery_verifies_the_existing_link_and_preserves_numeric_transaction_fields(bool withTransaction)
    {
        var transactions = withTransaction
            ? JsonSerializer.SerializeToElement(new[] { new { amount = 100, reference = "receipt", transactionDateTime = "2026-01-01" } })
            : JsonSerializer.SerializeToElement(Array.Empty<object>());
        var data = JsonSerializer.SerializeToElement(new { amount = 100000, id = "link123", orderCode = 7, status = "PENDING", transactions });
        var calls = new List<HttpMethod>();
        using var handler = new Handler(request => { calls.Add(request.Method); Assert.Equal("/v2/payment-requests/7", request.RequestUri!.AbsolutePath); return Response(data); });
        var link = await Service(handler).TaoLienKetThanhToan(7, 100000, "DON 7", recoverExisting: true);
        Assert.Equal("https://pay.payos.vn/web/link123", link.UrlThanhToan);
        Assert.Equal(new[] { HttpMethod.Get }, calls);
    }

    [Theory]
    [InlineData("{\"amount\":100000,\"code\":\"00\",\"currency\":\"USD\",\"orderCode\":7,\"paymentLinkId\":\"link\",\"reference\":\"ref\"}")]
    [InlineData("{\"amount\":100000,\"code\":\"00\",\"orderCode\":\"7\",\"paymentLinkId\":\"link\",\"reference\":\"ref\"}")]
    [InlineData("{\"amount\":100000.1,\"code\":\"00\",\"orderCode\":7,\"paymentLinkId\":\"link\",\"reference\":\"ref\"}")]
    public void Signed_but_invalid_webhook_fields_are_rejected(string json)
    {
        using var handler = new Handler(_ => throw new InvalidOperationException("A webhook must not call the gateway."));
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        var error = Assert.Throws<ApiException>(() => Service(handler).XacThucVaDocWebhook(new PayOsWebhook
        {
            MaKetQua = "00", ThanhCong = true, DuLieuWebhook = data, ChuKyWebhook = Sign(data)
        }));
        Assert.Equal(400, error.MaTrangThaiHttp);
    }

    private static HttpResponseMessage Response(JsonElement data, bool signed = true) => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new { code = "00", data, signature = signed ? Sign(data) : null })
    };

    private static string Sign(JsonElement data)
    {
        var text = string.Join("&", data.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => $"{p.Name}={(p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() : p.Value.GetRawText())}"));
        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("test-checksum-key"), Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(respond(request));
    }
}
