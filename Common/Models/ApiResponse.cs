namespace QLSK.Common.Models;
public record ApiResponse<TDuLieu>([property: System.Text.Json.Serialization.JsonPropertyName("success")] bool ThanhCong, [property: System.Text.Json.Serialization.JsonPropertyName("data")] TDuLieu? DuLieuPhanHoi, [property: System.Text.Json.Serialization.JsonPropertyName("code")] string? MaKetQua, [property: System.Text.Json.Serialization.JsonPropertyName("message")] string? ThongDiep)
{
    public static ApiResponse<TDuLieu> TaoPhanHoiThanhCong(TDuLieu duLieuPhanHoi) => new(true, duLieuPhanHoi, null, null);
    public static ApiResponse<TDuLieu> TaoPhanHoiThatBai(string maKetQua, string thongDiepLoi) => new(false, default, maKetQua, thongDiepLoi);
}
public static class ApiResponse
{
    public static ApiResponse<TDuLieu> TaoPhanHoiThanhCong<TDuLieu>(TDuLieu duLieuPhanHoi) => ApiResponse<TDuLieu>.TaoPhanHoiThanhCong(duLieuPhanHoi);
    public static ApiResponse<object> TaoPhanHoiThatBai(string maKetQua, string thongDiepLoi) => ApiResponse<object>.TaoPhanHoiThatBai(maKetQua, thongDiepLoi);
}
