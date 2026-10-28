using System.Text.Json;
using Microsoft.Extensions.Options;
using QLSK.Common.Exceptions;
using QLSK.Infrastructure.PayOS;
using Xunit;
namespace QLSK.Tests;

public class RuleTests
{
    [Fact]
    public void Payos_official_webhook_sample_signature_is_valid()
    {
        const string key = "1a54716c8f0efb2744fb28b6e38b25da7f67a925d98bc1c18bd8faaecadd7675";
        using var doc = JsonDocument.Parse("""{"orderCode":123,"amount":3000,"description":"VQRIO123","accountNumber":"12345678","reference":"TF230204212323","transactionDateTime":"2023-02-04 18:25:00","currency":"VND","paymentLinkId":"124c33293c43417ab7879e14c8d9eb18","code":"00","desc":"Thành công","counterAccountBankId":"","counterAccountBankName":"","counterAccountName":"","counterAccountNumber":"","virtualAccountName":"","virtualAccountNumber":""}""");
        var svc = new PayOsService(new HttpClient(), Options.Create(new PayOsOptions { ChecksumKey = key }));
        var x = svc.Verify(new PayOsWebhook { Code = "00", Desc = "success", Success = true, Data = doc.RootElement.Clone(), Signature = "412e915d2871504ed31be63c8f62a149a4410d34c4c42affc9006ef9917eaa03" });
        Assert.Equal(123, x.OrderCode); Assert.Equal(3000, x.Amount);
    }

    [Fact]
    public async Task Payos_rejects_fractional_vnd_amount()
    {
        var svc = new PayOsService(new HttpClient(), Options.Create(new PayOsOptions()));

        var error = await Assert.ThrowsAsync<ApiException>(() => svc.Tao(1, 10.5m, "DON 1"));

        Assert.Equal("PAYMENT_INVALID", error.Code);
    }
}
