using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class SuDungVoucher
{
    public int Id { get; set; }
    public int VoucherId { get; set; }
    public int DonHangId { get; set; }
    public decimal SoTienGiam { get; set; }
    public DateTime HetHanGiuLuot { get; set; }
    public DateTime? DaChotLuc { get; set; }
    public TrangThaiSuDungVoucher TrangThai { get; set; }
}

public class GiaoDichThanhToan
{
    public int Id { get; set; }
    public int? DonHangId { get; set; }
    public int? YeuCauThueId { get; set; }
    public string? MaDonNhaCungCap { get; set; }
    public string? MaGiaoDich { get; set; }
    public decimal SoTienYeuCau { get; set; }
    public decimal? SoTienThucThu { get; set; }
    public PhuongThucThanhToan PhuongThuc { get; set; }
    public TrangThaiGiaoDichThanhToan TrangThai { get; set; }
    public DateTime? NgayThanhToan { get; set; }
    public string? ChungTu { get; set; }
}

public class WebhookThanhToan
{
    public int Id { get; set; }
    public int? GiaoDichId { get; set; }
    public string MaSuKienWebhook { get; set; } = "";
    public string NoiDungDaLoc { get; set; } = "";
    public DateTime ThoiGianNhan { get; set; }
    public TrangThaiXuLyWebhook TrangThaiXuLy { get; set; }
}

public class YeuCauHoanTien
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public int GiaoDichId { get; set; }
    public int? VeId { get; set; }
    public string KhoaChongLap { get; set; } = "";
    public decimal SoTienYeuCau { get; set; }
    public decimal? SoTienDuyet { get; set; }
    public string LoaiYeuCau { get; set; } = "";
    public string LyDo { get; set; } = "";
    public TrangThaiYeuCauHoanTien TrangThai { get; set; }
    public DateTime NgayYeuCau { get; set; }
    public int? NguoiDuyetId { get; set; }
    public DateTime? NgayDuyet { get; set; }
}

public class GiaoDichHoanTien
{
    public int Id { get; set; }
    public int YeuCauHoanTienId { get; set; }
    public string KhoaChongLap { get; set; } = "";
    public string? MaGiaoDichHoan { get; set; }
    public decimal SoTien { get; set; }
    public TrangThaiChuyenTien TrangThai { get; set; }
    public DateTime? NgayXuLy { get; set; }
    public string? ChungTu { get; set; }
}

public class HopDong
{
    public int Id { get; set; }
    public int BTCId { get; set; }
    public int? SuKienId { get; set; }
    public LoaiPhiHopDong LoaiPhi { get; set; }
    public decimal? TyLePhi { get; set; }
    public decimal? PhiMoiVe { get; set; }
    public string NoiDung { get; set; } = "";
    public DateTime NgayKy { get; set; }
    public DateTime HieuLucTu { get; set; }
    public DateTime? HieuLucDen { get; set; }
    public TrangThaiHopDong TrangThai { get; set; }
}

public class DoiSoat
{
    public int Id { get; set; }
    public int SuKienId { get; set; }
    public int PhienBan { get; set; }
    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public decimal TongDoanhThu { get; set; }
    public decimal TongTienHoan { get; set; }
    public decimal PhiNenTang { get; set; }
    public decimal TienDieuChinh { get; set; }
    public decimal TienBTCNhan { get; set; }
    public string ChiTietDoiSoat { get; set; } = "";
    public DateTime HanXacNhan { get; set; }
    public int? NguoiDuyetId { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public TrangThaiDoiSoat TrangThai { get; set; }
}

public class GiaiNgan
{
    public int Id { get; set; }
    public int DoiSoatId { get; set; }
    public string KhoaChongLap { get; set; } = "";
    public string? MaGiaoDich { get; set; }
    public decimal SoTien { get; set; }
    public TrangThaiChuyenTien TrangThai { get; set; }
    public DateTime? NgayGiaiNgan { get; set; }
    public string? ChungTu { get; set; }
}
