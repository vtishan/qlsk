using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class ThongBao
{
    public int Id { get; set; }
    public int? TaiKhoanId { get; set; }
    public int? VeMoiId { get; set; }
    public string TieuDe { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public string LoaiThongBao { get; set; } = "";
    public KenhGuiThongBao KenhGui { get; set; }
    public TrangThaiGuiThongBao TrangThaiGui { get; set; }
    public bool DaDoc { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayGui { get; set; }
}

public class KhieuNai
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public int? SuKienId { get; set; }
    public int? VeId { get; set; }
    public int? YeuCauThueId { get; set; }
    public string TieuDe { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public TrangThaiKhieuNai TrangThai { get; set; }
    public int? NguoiXuLyId { get; set; }
    public DateTime NgayTao { get; set; }
    public string? KetQuaXuLy { get; set; }
}

public class DanhGia
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public int SuKienId { get; set; }
    public int SoSao { get; set; }
    public string? BinhLuan { get; set; }
    public DateTime NgayDanhGia { get; set; }
    public TrangThaiDanhGia TrangThai { get; set; }
}

public class Banner
{
    public int Id { get; set; }
    public string TieuDe { get; set; } = "";
    public string HinhAnh { get; set; } = "";
    public string? DuongDan { get; set; }
    public int ThuTu { get; set; }
    public TrangThaiHienThi TrangThai { get; set; }
}

public class CauHinhHeThong
{
    public int Id { get; set; }
    public string Khoa { get; set; } = "";
    public string GiaTri { get; set; } = "";
    public string? MoTa { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public int NguoiCapNhatId { get; set; }
}

public class NhatKyHeThong
{
    public int Id { get; set; }
    public int? TaiKhoanId { get; set; }
    public string HanhDong { get; set; } = "";
    public string DoiTuong { get; set; } = "";
    public int? DoiTuongId { get; set; }
    public DateTime ThoiGian { get; set; }
    public string? DuLieuThayDoi { get; set; }
}

public class ChinhSach
{
    public int Id { get; set; }
    public string TenChinhSach { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public int PhienBan { get; set; }
    public DateOnly NgayHieuLuc { get; set; }
    public TrangThaiChinhSach TrangThai { get; set; }
}
