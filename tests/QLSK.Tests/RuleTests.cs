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
        const string khoaKyHmac = "1a54716c8f0efb2744fb28b6e38b25da7f67a925d98bc1c18bd8faaecadd7675";
        using var taiLieuJson = JsonDocument.Parse("""{"orderCode":123,"amount":3000,"description":"VQRIO123","accountNumber":"12345678","reference":"TF230204212323","transactionDateTime":"2023-02-04 18:25:00","currency":"VND","paymentLinkId":"124c33293c43417ab7879e14c8d9eb18","code":"00","desc":"Thành công","counterAccountBankId":"","counterAccountBankName":"","counterAccountName":"","counterAccountNumber":"","virtualAccountName":"","virtualAccountNumber":""}""");
        var dichVuPayOs = new PayOsService(new HttpClient(), Options.Create(new PayOsOptions { ChecksumKey = khoaKyHmac }));
        var duLieuWebhook = dichVuPayOs.XacThucVaDocWebhook(new PayOsWebhook { MaKetQua = "00", MoTaKetQua = "success", ThanhCong = true, DuLieuWebhook = taiLieuJson.RootElement.Clone(), ChuKyWebhook = "412e915d2871504ed31be63c8f62a149a4410d34c4c42affc9006ef9917eaa03" });
        Assert.Equal(123, duLieuWebhook.MaDonVe); Assert.Equal(3000, duLieuWebhook.SoTienGiaoDich);
    }

    [Fact]
    public async Task Payos_rejects_fractional_vnd_amount()
    {
        var dichVuPayOs = new PayOsService(new HttpClient(), Options.Create(new PayOsOptions()));

        var loiXuLy = await Assert.ThrowsAsync<ApiException>(() => dichVuPayOs.TaoLienKetThanhToan(1, 10.5m, "DON 1"));

        Assert.Equal("PAYMENT_INVALID", loiXuLy.MaLoi);
    }

    [Theory]
    [InlineData("", "test-api-key", "test-checksum-key")]
    [InlineData("test-client-id", "", "test-checksum-key")]
    [InlineData("test-client-id", "test-api-key", "")]
    public async Task Payos_missing_configuration_returns_unavailable_without_gateway_calls(string clientId, string apiKey, string checksumKey)
    {
        using var boXuLyYeuCauHttp = new RejectingPaymentHandler();
        using var clientHttp = new HttpClient(boXuLyYeuCauHttp);
        var dichVuPayOs = new PayOsService(clientHttp, Options.Create(new PayOsOptions
        {
            ClientId = clientId, ApiKey = apiKey, ChecksumKey = checksumKey
        }));

        var loiXuLy = await Assert.ThrowsAsync<ApiException>(() => dichVuPayOs.TaoLienKetThanhToan(1, 100_000, "DON 1"));

        Assert.Equal(503, loiXuLy.MaTrangThaiHttp);
        Assert.Equal("PAYMENT_NOT_CONFIGURED", loiXuLy.MaLoi);
        Assert.Equal(0, boXuLyYeuCauHttp.Calls);
    }

    private sealed class RejectingPaymentHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage yeuCauHttp, CancellationToken tokenHuyThaoTac)
        {
            Calls++;
            throw new InvalidOperationException("A missing payment configuration must not contact any gateway.");
        }
    }
}
