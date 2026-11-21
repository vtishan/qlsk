using System.Text.Json.Serialization;

namespace QLSK.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<NhomVe>))]
public enum NhomVe
{
    TieuChuan = 1,
    VIP,
    VeMoi
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiDonHang>))]
public enum TrangThaiDonHang
{
    ChoThanhToan = 1,
    DaThanhToan,
    HetHan,
    DaHuy,
    DaThuTienHetCho,
    ChoHoanTien,
    HoanMotPhan,
    DaHoanTien
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiGiuGhe>))]
public enum TrangThaiGiuGhe
{
    DangGiu = 1,
    DaChot,
    DaNha,
    HetHan
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiVe>))]
public enum TrangThaiVe
{
    DaPhatHanh = 1,
    DaCheckIn,
    DaHuy,
    DaThuHoi
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiVeMoi>))]
public enum TrangThaiVeMoi
{
    ChoGui = 1,
    DaGui,
    DaNhan,
    DaThuHoi,
    HetHan
}

[JsonConverter(typeof(JsonStringEnumConverter<HinhThucCheckIn>))]
public enum HinhThucCheckIn
{
    QuetQR = 1,
    ThuCong
}

[JsonConverter(typeof(JsonStringEnumConverter<LoaiGiamGia>))]
public enum LoaiGiamGia
{
    PhanTram = 1,
    SoTien
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiVoucher>))]
public enum TrangThaiVoucher
{
    Nhap = 1,
    ChoDuyet,
    HoatDong,
    TamDung,
    TuChoi,
    HetHan
}
