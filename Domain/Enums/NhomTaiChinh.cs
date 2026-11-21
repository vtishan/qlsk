using System.Text.Json.Serialization;

namespace QLSK.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiSuDungVoucher>))]
public enum TrangThaiSuDungVoucher
{
    DangGiu = 1,
    DaSuDung,
    DaNha
}

[JsonConverter(typeof(JsonStringEnumConverter<PhuongThucThanhToan>))]
public enum PhuongThucThanhToan
{
    PayOS = 1,
    ChuyenKhoan,
    MienPhi
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiGiaoDichThanhToan>))]
public enum TrangThaiGiaoDichThanhToan
{
    ChoThanhToan = 1,
    ThanhCong,
    ThatBai,
    HetHan,
    NghiVan,
    ThuTrungCanHoan
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiXuLyWebhook>))]
public enum TrangThaiXuLyWebhook
{
    ChoXuLy = 1,
    DaXuLy,
    NghiVan,
    Loi
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiYeuCauHoanTien>))]
public enum TrangThaiYeuCauHoanTien
{
    ChoDuyet = 1,
    DaDuyet,
    TuChoi,
    DangChuyen,
    HoanTat,
    ThatBai
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiChuyenTien>))]
public enum TrangThaiChuyenTien
{
    ChoChuyen = 1,
    DangChuyen,
    ThanhCong,
    ThatBai
}

[JsonConverter(typeof(JsonStringEnumConverter<LoaiPhiHopDong>))]
public enum LoaiPhiHopDong
{
    PhanTram = 1,
    MoiVe
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiHopDong>))]
public enum TrangThaiHopDong
{
    Nhap = 1,
    CoHieuLuc,
    HetHan,
    DaHuy
}

[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiDoiSoat>))]
public enum TrangThaiDoiSoat
{
    ChoXacNhan = 1,
    DaXacNhan,
    TranhChap,
    ChoGiaiNgan,
    HoanTat
}
