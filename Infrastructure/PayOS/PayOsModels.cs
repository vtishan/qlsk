using System.Text.Json;
using System.Text.Json.Serialization;

namespace QLSK.Infrastructure.PayOS;

public record PayOsLink(
    [property: JsonPropertyName("checkoutUrl")]
    string UrlThanhToan,

    [property: JsonPropertyName("paymentLinkId")]
    string? MaLienKetThanhToan
);

public class PayOsWebhook
{
    [JsonPropertyName("code")]
    public string MaKetQua { get; set; } = "";

    [JsonPropertyName("desc")]
    public string MoTaKetQua { get; set; } = "";

    [JsonPropertyName("success")]
    public bool ThanhCong { get; set; }

    [JsonPropertyName("data")]
    public JsonElement DuLieuWebhook { get; set; }

    [JsonPropertyName("signature")]
    public string ChuKyWebhook { get; set; } = "";
}

public record PayOsWebhookData(
    [property: JsonPropertyName("orderCode")]
    long MaDonVe,

    [property: JsonPropertyName("amount")]
    decimal SoTienGiaoDich,

    [property: JsonPropertyName("reference")]
    string MaThamChieu,

    [property: JsonPropertyName("paymentLinkId")]
    string? MaLienKetThanhToan,

    [property: JsonPropertyName("code")]
    string MaKetQua,

    [property: JsonPropertyName("desc")]
    string MoTaGiaoDich
);