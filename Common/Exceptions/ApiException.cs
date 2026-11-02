namespace QLSK.Common.Exceptions;
public class ApiException(int maTrangThaiHttp, string maKetQua, string thongDiepLoi) : Exception(thongDiepLoi)
{
    [System.Text.Json.Serialization.JsonPropertyName("status")] public int MaTrangThaiHttp { get; } = maTrangThaiHttp;
    [System.Text.Json.Serialization.JsonPropertyName("code")] public string MaLoi { get; } = maKetQua;
}
