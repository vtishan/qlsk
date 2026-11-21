using System.Text.Json.Serialization;

namespace QLSK.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<KenhGuiThongBao>))]
public enum KenhGuiThongBao
{
    TrongUngDung = 1,
    Email,
    SMS
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiGuiThongBao>))]
public enum TrangThaiGuiThongBao
{
    ChoGui = 1,
    DangGui,
    DaGui,
    LoiGui
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiKhieuNai>))]
public enum TrangThaiKhieuNai
{
    ChoXuLy = 1,
    DangXuLy,
    DaGiaiQuyet,
    TuChoi
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiDanhGia>))]
public enum TrangThaiDanhGia
{
    ChoDuyet = 1,
    CongKhai,
    DaAn
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiChinhSach>))]
public enum TrangThaiChinhSach
{
    Nhap = 1,
    CoHieuLuc,
    HetHan,
    DaHuy
}
