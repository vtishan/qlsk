using System.Text.Json.Serialization;

namespace QLSK.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiHoSoToChuc>))]
public enum TrangThaiHoSoToChuc
{
    Nhap = 1,
    ChoDuyet,
    CanBoSung,
    TuChoi,
    DaDuyet
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiPhanCongSuKien>))]
public enum TrangThaiPhanCongSuKien
{
    ChoNhan = 1,
    DangHieuLuc,
    DaThuHoi,
    HetHan,
    TuChoi
}

public enum VaiTroTaiKhoan
{
    NguoiDung = 1,
    BanToChuc = 2,
    QuanLySuKien = 3,
    QuanLyHeThong = 4,
    QuanTriVien = 5
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiTaiKhoan>))]
public enum TrangThaiTaiKhoan
{
    HoatDong = 1,
    DaKhoa,
    ChoXacMinh
}
