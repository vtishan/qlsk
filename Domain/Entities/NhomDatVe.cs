using System.Text.Json.Serialization;

using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class LoaiVe
{
    public int Id { get; set; }
    public int SuatDienId { get; set; }
    public int KhuVucId { get; set; }
    public string TenLoaiVe { get; set; } = "";
    public NhomVe NhomVe { get; set; }
    public decimal GiaVe { get; set; }
    public int SoLuong { get; set; }
    public string? QuyenLoi { get; set; }
    public DateTime BatDauBan { get; set; }
    public DateTime KetThucBan { get; set; }
}

public class DonHang
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public int SuatDienId { get; set; }
    public int? HopDongId { get; set; }
    public int ChinhSachId { get; set; }
    public decimal TienVe { get; set; }
    public decimal TienGiam { get; set; }
    public decimal PhiDichVu { get; set; }
    public decimal PhiNenTang { get; set; }
    public decimal TongTien { get; set; }
    public TrangThaiDonHang TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime HanThanhToan { get; set; }
    public string? KhoaTaoDon { get; set; }
    public string? DauVanTayTaoDon { get; set; }
}

public class ChiTietDonHang
{
    public int Id { get; set; }
    public int DonHangId { get; set; }
    public int LoaiVeId { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}

public class GiuGhe
{
    public int Id { get; set; }
    public int SuatDienId { get; set; }
    public int? GheId { get; set; }
    public int ChiTietDonHangId { get; set; }
    public DateTime HetHan { get; set; }
    public TrangThaiGiuGhe TrangThai { get; set; }
}

public class Ve
{
    public int Id { get; set; }
    public int? ChiTietDonHangId { get; set; }
    public int? GiuGheId { get; set; }
    public int LoaiVeId { get; set; }
    public int SuatDienId { get; set; }
    public int? GheId { get; set; }
    public string MaVe { get; set; } = "";
    public string MaQR { get; set; } = "";
    public TrangThaiVe TrangThai { get; set; }
    public DateTime NgayPhatHanh { get; set; }
    public DateTime? DaCheckInLuc { get; set; }
}

public class VeMoi
{
    public int Id { get; set; }
    public int VeId { get; set; }
    public string HoTenKhachMoi { get; set; } = "";
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public int? TaiKhoanNhanId { get; set; }
    [JsonIgnore]
    public string? MaBamNhanVe { get; set; }
    public DateTime? HanNhanVe { get; set; }
    public DateTime? DaGuiLuc { get; set; }
    public DateTime? DaNhanLuc { get; set; }
    public TrangThaiVeMoi TrangThai { get; set; }
}

public class NhatKyCheckIn
{
    public int Id { get; set; }
    public int VeId { get; set; }
    public int NhanVienId { get; set; }
    public DateTime ThoiGianQuet { get; set; }
    public string KetQua { get; set; } = "";
    public HinhThucCheckIn HinhThuc { get; set; }
}

public class Voucher
{
    public int Id { get; set; }
    public string MaVoucher { get; set; } = "";
    public int? SuKienId { get; set; }
    public int? SuatDienId { get; set; }
    public int? KhuVucId { get; set; }
    public LoaiGiamGia LoaiGiam { get; set; }
    public decimal GiaTri { get; set; }
    public decimal? MucGiamToiDa { get; set; }
    public decimal GiaTriDonToiThieu { get; set; }
    public int SoLuong { get; set; }
    public int LuotMoiTaiKhoan { get; set; }
    public string NguonTaiTro { get; set; } = "";
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public TrangThaiVoucher TrangThai { get; set; }
}
