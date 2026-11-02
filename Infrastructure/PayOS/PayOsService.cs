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
        long maDonVe,
        decimal soTienThanhToan,
        string moTaThanhToan,
        DateTime? thoiDiemHetHanLienKet = null)
    {
        KiemTraSoTienThanhToan(soTienThanhToan);

        if (ThieuCauHinhThanhToan())
        {
            throw new ApiException(503, "PAYMENT_NOT_CONFIGURED", "Thanh toán trực tuyến hiện chưa khả dụng. Vui lòng thử lại sau.");
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

        using var phanHoiHttp = await clientHttp.SendAsync(yeuCauHttp);

        var noiDungPhanHoi = await phanHoiHttp.Content.ReadAsStringAsync();

        if (!phanHoiHttp.IsSuccessStatusCode)
        {
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
            throw new ApiException(
                502,
                "PAYMENT_GATEWAY_ERROR",
                "payOS từ chối tạo link thanh toán."
            );
        }

        var duLieuJson = gocJson.GetProperty("data");

        var checkoutUrl =
            duLieuJson.GetProperty("checkoutUrl").GetString() ?? "";

        var maLienKetThanhToan =
            duLieuJson.TryGetProperty("paymentLinkId", out var lienKetJson)
                ? lienKetJson.GetString()
                : null;

        return new PayOsLink(
            checkoutUrl,
            maLienKetThanhToan
        );
    }

    public PayOsWebhookData XacThucVaDocWebhook(PayOsWebhook webhook)
    {
        KiemTraKhoaXacThucWebhook();

        var chuoiCanKy = TaoChuoiKyDuLieuWebhook(webhook.DuLieuWebhook);
        var chuKyMongDoi = TaoChuKyHmac(chuoiCanKy);

        var chuKyHopLe = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(chuKyMongDoi.ToLowerInvariant()),
            Encoding.UTF8.GetBytes(
                webhook.ChuKyWebhook.ToLowerInvariant()
            )
        );

        if (!chuKyHopLe)
        {
            throw new ApiException(
                400,
                "PAYMENT_VERIFICATION_FAILED",
                "Webhook payOS không hợp lệ."
            );
        }

        var duLieuJson = webhook.DuLieuWebhook;

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

        return new PayOsWebhookData(
            maDonVe,
            soTienThanhToan,
            maThamChieuThanhToan,
            maLienKetThanhToan,
            maKetQua,
            moTaGiaoDich
        );
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

            JsonValueKind.Array
                => JsonSerializer.Serialize(
                    giaTriJsonCanXuLy
                        .EnumerateArray()
                        .Select(phanTuJson =>
                            phanTuJson.ValueKind == JsonValueKind.Object
                                ? (object)SapXepThuocTinhJson(phanTuJson)
                                : phanTuJson
                        )
                ),

            JsonValueKind.Object
                => JsonSerializer.Serialize(
                    SapXepThuocTinhJson(giaTriJsonCanXuLy)
                ),

            _ => giaTriJsonCanXuLy.ToString()
        };
    }

    private static SortedDictionary<string, object?>
        SapXepThuocTinhJson(JsonElement giaTriJsonCanXuLy)
    {
        var thuocTinhDaSapXep =
            new SortedDictionary<string, object?>(
                StringComparer.Ordinal
            );

        foreach (var thuocTinhJson in giaTriJsonCanXuLy
            .EnumerateObject()
            .OrderBy(thuocTinhJson => thuocTinhJson.Name, StringComparer.Ordinal))
        {
            thuocTinhDaSapXep[thuocTinhJson.Name] =
                thuocTinhJson.Value.ValueKind == JsonValueKind.String
                    ? thuocTinhJson.Value.GetString()
                    : thuocTinhJson.Value.GetRawText();
        }

        return thuocTinhDaSapXep;
    }
}
