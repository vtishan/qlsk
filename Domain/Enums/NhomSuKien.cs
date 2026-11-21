using System.Text.Json.Serialization;

namespace QLSK.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiHienThi>))]
public enum TrangThaiHienThi
{
    Hien = 1,
    An
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiSuKien>))]
public enum TrangThaiSuKien
{
    Nhap = 1,
    ChoDuyet,
    DaDuyet,
    DangMoBan,
    Hoan,
    DinhChi,
    DaHuy,
    HoanTat,
    TuChoi
}

[JsonConverter(typeof(JsonStringEnumConverter<LoaiYeuCauSuKien>))]
public enum LoaiYeuCauSuKien
{
    TaoMoi = 1,
    CapNhat,
    Hoan,
    Huy
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiDuyetPhienBan>))]
public enum TrangThaiDuyetPhienBan
{
    Nhap = 1,
    ChoDuyet,
    DaDuyet,
    TuChoi
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiSuatDien>))]
public enum TrangThaiSuatDien
{
    Nhap = 1,
    DaXacNhan,
    DangMoBan,
    DongBan,
    Hoan,
    DaHuy,
    HoanTat
}
