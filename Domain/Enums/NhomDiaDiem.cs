using System.Text.Json.Serialization;

namespace QLSK.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiDiaDiem>))]
public enum TrangThaiDiaDiem
{
    HoatDong = 1,
    TamDung,
    DaAn
}

[JsonConverter(typeof(JsonStringEnumConverter<LoaiGheNgoi>))]
public enum LoaiGheNgoi
{
    GheDon = 1,
    GheDoi,
    GheCum
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiGhe>))]
public enum TrangThaiGhe
{
    SuDung = 1,
    BaoTri,
    NgungSuDung
}

[JsonConverter(typeof(JsonStringEnumConverter<LoaiCumGhe>))]
public enum LoaiCumGhe
{
    GheDoi = 1,
    GheCum
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiYeuCauThue>))]
public enum TrangThaiYeuCauThue
{
    ChoDuyet = 1,
    ChoCoc,
    DaXacNhan,
    TuChoi,
    HetHan,
    DaHuy
}

[JsonConverter(typeof(JsonStringEnumConverter<LoaiLichDiaDiem>))]
public enum LoaiLichDiaDiem
{
    Thue = 1,
    SuatDien,
    BaoTri
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiLichDatDiaDiem>))]
public enum TrangThaiLichDatDiaDiem
{
    DangGiu = 1,
    DaXacNhan,
    HetHan,
    DaHuy
}
