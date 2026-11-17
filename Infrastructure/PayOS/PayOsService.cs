using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QLSK.Common.Exceptions;

namespace QLSK.Infrastructure.PayOS;

public class PayOsService(
    HttpClient clientHttp,
    IOptions<PayOsOptions> tuyChonPayOs)
{
    private readonly PayOsOptions cauHinh = tuyChonPayOs.Value;

    public async Task<PayOsLink> TaoLienKetThanhToan(
        long maDonVe, decimal soTienThanhToan, string moTaThanhToan,
        DateTime? thoiDiemHetHanLienKet = null, CancellationToken cancellationToken = default, bool recoverExisting = false)
    {
        try
        {
            return await TaoLienKetThanhToanCore(maDonVe, soTienThanhToan, moTaThanhToan,
                thoiDiemHetHanLienKet, cancellationToken, recoverExisting);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException(503, "PAYMENT_GATEWAY_TIMEOUT", "Cổng thanh toán phản hồi chậm. Vui lòng kiểm tra đơn rồi thử lại.");
        }
        catch (ApiException error) when (error.MaLoi == "PAYMENT_VERIFICATION_FAILED")
        {
            throw new ApiException(502, "PAYMENT_GATEWAY_ERROR", "Không thể xác thực thông tin từ cổng thanh toán.");
        }
        catch (Exception error) when (error is JsonException or KeyNotFoundException or InvalidOperationException or FormatException or OverflowException)
        {
            throw new ApiException(502, "PAYMENT_GATEWAY_ERROR", "Phản hồi từ cổng thanh toán không hợp lệ.");
        }
    }

    private async Task<PayOsLink> TaoLienKetThanhToanCore(
        long maDonVe,
        decimal soTienThanhToan,
        string moTaThanhToan,
        DateTime? thoiDiemHetHanLienKet = null,
        CancellationToken cancellationToken = default,
        bool recoverExisting = false)
    {
        KiemTraSoTienThanhToan(soTienThanhToan);

        if (ThieuCauHinhThanhToan())
        {
            throw new ApiException(503, "PAYMENT_NOT_CONFIGURED", "Thanh toán trực tuyến hiện chưa khả dụng. Vui lòng thử lại sau.");
        }

        if (recoverExisting)
        {
            var existing = await LayLienKetDaTao(maDonVe, soTienThanhToan, cancellationToken);
            if (existing is not null) return existing;
        }

        var soTienNguyen = decimal.ToInt64(soTienThanhToan);
        var moTaRutGon = RutGonMoTaThanhToan(moTaThanhToan);

        var chuoiCanKy = TaoChuoiKyLienKetThanhToan(
            maDonVe,
            soTienNguyen,
            moTaRutGon
        );

        var noiDungYeuCau = new
        {
            orderCode = maDonVe,
            amount = soTienNguyen,
            description = moTaRutGon,
            returnUrl = cauHinh.ReturnUrl,
            cancelUrl = cauHinh.CancelUrl,
            expiredAt = ChuyenHanThanhToanSangUnix(thoiDiemHetHanLienKet),
            signature = TaoChuKyHmac(chuoiCanKy)
        };

        using var yeuCauHttp = new HttpRequestMessage(
            HttpMethod.Post,
            "v2/payment-requests"
        )
        {
            Content = JsonContent.Create(noiDungYeuCau)
        };

        yeuCauHttp.Headers.Add("x-client-id", cauHinh.ClientId);
        yeuCauHttp.Headers.Add("x-api-key", cauHinh.ApiKey);

        using var phanHoiHttp = await clientHttp.SendAsync(yeuCauHttp, cancellationToken);

        var noiDungPhanHoi = await phanHoiHttp.Content.ReadAsStringAsync(cancellationToken);

        if (!phanHoiHttp.IsSuccessStatusCode)
        {
            if (phanHoiHttp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new ApiException(503, "PAYMENT_GATEWAY_BUSY", "Cổng thanh toán đang bận. Vui lòng thử lại sau.");
            throw new ApiException(
                502,
                "PAYMENT_GATEWAY_ERROR",
                "Không thể tạo link thanh toán payOS."
            );
        }

        using var taiLieuJson = JsonDocument.Parse(noiDungPhanHoi);

        var gocJson = taiLieuJson.RootElement;
        var maKetQua = gocJson.GetProperty("code").GetString();

        if (maKetQua != "00")
        {
            // payOS orderCode is stable across retries. Never create a second payable order after a timeout.
            var existing = await LayLienKetDaTao(maDonVe, soTienThanhToan, cancellationToken);
            if (existing is not null) return existing;
            throw new ApiException(
                502,
                "PAYMENT_GATEWAY_ERROR",
                "payOS từ chối tạo link thanh toán."
            );
        }

        var duLieuJson = gocJson.GetProperty("data");
        KiemTraChuKy(duLieuJson, gocJson.TryGetProperty("signature", out var signature) ? signature.GetString() : null);
        if (!duLieuJson.TryGetProperty("orderCode", out var orderCode) || !orderCode.TryGetInt64(out var order) || order != maDonVe ||
            !duLieuJson.TryGetProperty("amount", out var amount) || !amount.TryGetDecimal(out var paidAmount) || paidAmount != soTienThanhToan ||
            !duLieuJson.TryGetProperty("currency", out var currency) || currency.GetString() != "VND")
            throw new ApiException(502, "PAYMENT_GATEWAY_ERROR", "Thông tin liên kết thanh toán không khớp đơn.");

        var checkoutUrl =
            duLieuJson.GetProperty("checkoutUrl").GetString() ?? "";

        var maLienKetThanhToan =
            duLieuJson.TryGetProperty("paymentLinkId", out var lienKetJson)
                ? lienKetJson.GetString()
                : null;

        if (string.IsNullOrWhiteSpace(maLienKetThanhToan) || maLienKetThanhToan.Length > 120 ||
            !Uri.TryCreate(checkoutUrl, UriKind.Absolute, out var checkout) || checkout.Scheme != "https" ||
            !checkout.IsDefaultPort || checkout.UserInfo.Length != 0 ||
            (checkout.Host != "pay.payos.vn" && checkout.Host != "checkout.payos.vn"))
            throw new ApiException(502, "PAYMENT_GATEWAY_ERROR", "Liên kết thanh toán không hợp lệ.");

        return new PayOsLink(
            checkoutUrl,
            maLienKetThanhToan
        );
    }

    public PayOsWebhookData XacThucVaDocWebhook(PayOsWebhook webhook)
    {
        KiemTraKhoaXacThucWebhook();

        KiemTraChuKy(webhook.DuLieuWebhook, webhook.ChuKyWebhook);

        var duLieuJson = webhook.DuLieuWebhook;

        if (!duLieuJson.TryGetProperty("orderCode", out var orderElement) || orderElement.ValueKind != JsonValueKind.Number || !orderElement.TryGetInt64(out var parsedOrder) || parsedOrder is < 1 or > int.MaxValue ||
            !duLieuJson.TryGetProperty("amount", out var amountElement) || amountElement.ValueKind != JsonValueKind.Number || !amountElement.TryGetDecimal(out var parsedAmount) ||
            parsedAmount <= 0 || parsedAmount != decimal.Truncate(parsedAmount) ||
            (duLieuJson.TryGetProperty("currency", out var currencyElement) && (currencyElement.ValueKind != JsonValueKind.String || currencyElement.GetString() != "VND")))
            throw new ApiException(400, "PAYMENT_INVALID", "Dữ liệu webhook không hợp lệ.");
        foreach (var name in new[] { "reference", "paymentLinkId", "code", "desc" })
            if (duLieuJson.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.String)
                throw new ApiException(400, "PAYMENT_INVALID", "Dữ liệu webhook không hợp lệ.");

        var maDonVe =
            duLieuJson.GetProperty("orderCode").GetInt64();

        var soTienThanhToan =
            duLieuJson.GetProperty("amount").GetDecimal();

        var maThamChieuThanhToan =
            duLieuJson.TryGetProperty("reference", out var maThamChieuJson)
                ? maThamChieuJson.GetString() ?? ""
                : "";

        var maLienKetThanhToan =
            duLieuJson.TryGetProperty("paymentLinkId", out var lienKetJson)
                ? lienKetJson.GetString()
                : null;

        var maKetQua =
            duLieuJson.TryGetProperty("code", out var maKetQuaJson)
                ? maKetQuaJson.GetString() ?? ""
                : "";

        var moTaGiaoDich =
            duLieuJson.TryGetProperty("desc", out var moTaJson)
                ? moTaJson.GetString() ?? ""
                : "";

        if (string.IsNullOrWhiteSpace(maThamChieuThanhToan) || maThamChieuThanhToan.Length > 120 ||
            string.IsNullOrWhiteSpace(maLienKetThanhToan) || maLienKetThanhToan.Length > 120)
            throw new ApiException(400, "PAYMENT_INVALID", "Mã giao dịch hoặc liên kết thanh toán không hợp lệ.");

        return new PayOsWebhookData(
            maDonVe,
            soTienThanhToan,
            maThamChieuThanhToan,
            maLienKetThanhToan,
            maKetQua,
            moTaGiaoDich
        );
    }

    private void KiemTraChuKy(JsonElement data, string? signature)
    {
        if (data.ValueKind != JsonValueKind.Object || signature?.Length != 64 ||
            signature.Any(c => !Uri.IsHexDigit(c)) ||
            data.EnumerateObject().Select(p => p.Name).Distinct(StringComparer.Ordinal).Count() != data.EnumerateObject().Count())
            throw new ApiException(400, "PAYMENT_VERIFICATION_FAILED", "Chữ ký thanh toán không hợp lệ.");
        var expected = Convert.FromHexString(TaoChuKyHmac(TaoChuoiKyDuLieuWebhook(data)));
        if (!CryptographicOperations.FixedTimeEquals(expected, Convert.FromHexString(signature)))
            throw new ApiException(400, "PAYMENT_VERIFICATION_FAILED", "Chữ ký thanh toán không hợp lệ.");
    }

    private async Task<PayOsLink?> LayLienKetDaTao(long orderCode, decimal amount, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"v2/payment-requests/{orderCode.ToString(CultureInfo.InvariantCulture)}");
        request.Headers.Add("x-client-id", cauHinh.ClientId);
        request.Headers.Add("x-api-key", cauHinh.ApiKey);
        using var response = await clientHttp.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode) throw new ApiException(503, "PAYMENT_GATEWAY_BUSY", "Không thể kiểm tra liên kết thanh toán. Vui lòng thử lại sau.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var root = json.RootElement;
        // 201 is payOS's 'payment link not found' code; other errors must not trigger a new creation.
        var code = root.GetProperty("code").GetString();
        if (code == "201") return null;
        if (code != "00") throw new ApiException(502, "PAYMENT_GATEWAY_ERROR", "Không thể kiểm tra liên kết thanh toán.");
        var data = root.GetProperty("data");
        KiemTraChuKy(data, root.TryGetProperty("signature", out var signature) ? signature.GetString() : null);
        if (data.GetProperty("orderCode").GetInt64() != orderCode || data.GetProperty("amount").GetDecimal() != amount ||
            data.GetProperty("status").GetString() is not ("PENDING" or "PROCESSING"))
            throw new ApiException(409, "PAYMENT_IN_PROGRESS", "Giao dịch đã thay đổi. Vui lòng kiểm tra trạng thái đơn.");
        var id = data.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(id) || id.Length > 120 || id.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-'))
            throw new ApiException(502, "PAYMENT_GATEWAY_ERROR", "Mã liên kết thanh toán không hợp lệ.");
        return new PayOsLink($"https://pay.payos.vn/web/{id}", id);
    }

    private void KiemTraSoTienThanhToan(decimal soTienThanhToan)
    {
        if (soTienThanhToan <= 0 || soTienThanhToan != decimal.Truncate(soTienThanhToan))
        {
            throw new ApiException(
                400,
                "PAYMENT_INVALID",
                "Số tiền thanh toán phải là số VND nguyên dương."
            );
        }
    }

    private bool ThieuCauHinhThanhToan()
    {
        return
            string.IsNullOrWhiteSpace(cauHinh.ClientId) ||
            string.IsNullOrWhiteSpace(cauHinh.ApiKey) ||
            string.IsNullOrWhiteSpace(cauHinh.ChecksumKey);
    }

    private static string RutGonMoTaThanhToan(string moTaThanhToan)
    {
        return moTaThanhToan.Length > 25
            ? moTaThanhToan[..25]
            : moTaThanhToan;
    }

    private string TaoChuoiKyLienKetThanhToan(
        long maDonVe,
        long soTienThanhToan,
        string moTaSuKien)
    {
        return
            $"amount={soTienThanhToan}" +
            $"&cancelUrl={cauHinh.CancelUrl}" +
            $"&description={moTaSuKien}" +
            $"&orderCode={maDonVe}" +
            $"&returnUrl={cauHinh.ReturnUrl}";
    }

    private static long? ChuyenHanThanhToanSangUnix(
        DateTime? thoiDiemHetHanLienKet)
    {
        if (!thoiDiemHetHanLienKet.HasValue)
        {
            return null;
        }

        return new DateTimeOffset(
            thoiDiemHetHanLienKet.Value
        ).ToUnixTimeSeconds();
    }

    private void KiemTraKhoaXacThucWebhook()
    {
        if (string.IsNullOrWhiteSpace(cauHinh.ChecksumKey))
        {
            throw new ApiException(
                503,
                "PAYMENT_NOT_CONFIGURED",
                "Chưa cấu hình checksum key payOS."
            );
        }
    }

    private string TaoChuoiKyDuLieuWebhook(JsonElement duLieuJson)
    {
        var capThuocTinh = new List<string>();

        foreach (var thuocTinhJson in duLieuJson
            .EnumerateObject()
            .OrderBy(thuocTinhJson => thuocTinhJson.Name, StringComparer.Ordinal))
        {
            capThuocTinh.Add(
                $"{thuocTinhJson.Name}={ChuyenGiaTriJsonDeKy(thuocTinhJson.Value)}"
            );
        }

        return string.Join("&", capThuocTinh);
    }

    private string TaoChuKyHmac(string chuoiCanKy)
    {
        var khoaKyHmac = Encoding.UTF8.GetBytes(
            cauHinh.ChecksumKey
        );

        var duLieuCanKy = Encoding.UTF8.GetBytes(chuoiCanKy);

        using var boBamHmac = new HMACSHA256(khoaKyHmac);

        var chuKyHmac = boBamHmac.ComputeHash(duLieuCanKy);

        return Convert
            .ToHexString(chuKyHmac)
            .ToLowerInvariant();
    }

    private static string ChuyenGiaTriJsonDeKy(JsonElement giaTriJsonCanXuLy)
    {
        return giaTriJsonCanXuLy.ValueKind switch
        {
            JsonValueKind.Null
                or JsonValueKind.Undefined
                => "",

            JsonValueKind.True
                => "true",

            JsonValueKind.False
                => "false",

            JsonValueKind.Number
                => giaTriJsonCanXuLy.GetRawText(),

            JsonValueKind.String
                => giaTriJsonCanXuLy.GetString() ?? "",

            JsonValueKind.Array or JsonValueKind.Object
                => JsonSerializer.Serialize(
                    SapXepGiaTriJson(giaTriJsonCanXuLy),
                    new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
                ),

            _ => giaTriJsonCanXuLy.ToString()
        };
    }

    private static object? SapXepGiaTriJson(JsonElement giaTriJsonCanXuLy)
    {
        if (giaTriJsonCanXuLy.ValueKind == JsonValueKind.Array)
            return giaTriJsonCanXuLy.EnumerateArray().Select(SapXepGiaTriJson).ToArray();
        if (giaTriJsonCanXuLy.ValueKind != JsonValueKind.Object)
            return giaTriJsonCanXuLy.ValueKind switch
            {
                JsonValueKind.String => giaTriJsonCanXuLy.GetString(),
                JsonValueKind.Null => null,
                _ => giaTriJsonCanXuLy.Clone()
            };
        var thuocTinhDaSapXep =
            new SortedDictionary<string, object?>(
                StringComparer.Ordinal
            );

        foreach (var thuocTinhJson in giaTriJsonCanXuLy
            .EnumerateObject()
            .OrderBy(thuocTinhJson => thuocTinhJson.Name, StringComparer.Ordinal))
        {
            thuocTinhDaSapXep[thuocTinhJson.Name] = SapXepGiaTriJson(thuocTinhJson.Value);
        }

        return thuocTinhDaSapXep;
    }
}
