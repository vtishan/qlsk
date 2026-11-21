using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class DiaDiem
{
    public int Id { get; set; }
    public string TenDiaDiem { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public int SucChua { get; set; }
    public decimal GiaThue { get; set; }
    public string? TienIch { get; set; }
    public TrangThaiDiaDiem TrangThai { get; set; }
}

public class KhuVuc
{
    public int Id { get; set; }
    public int DiaDiemId { get; set; }
    public string TenKhuVuc { get; set; } = "";
    public int SucChua { get; set; }
}

public class Ghe
{
    public int Id { get; set; }
    public int KhuVucId { get; set; }
    public int? CumGheId { get; set; }
    public string MaGhe { get; set; } = "";
    public LoaiGheNgoi LoaiGhe { get; set; }
    public TrangThaiGhe TrangThai { get; set; }
}

public class CumGhe
{
    public int Id { get; set; }
    public int KhuVucId { get; set; }
    public string TenCum { get; set; } = "";
    public LoaiCumGhe LoaiCum { get; set; }
    public int SoCho { get; set; }
}

public class YeuCauThue
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public int DiaDiemId { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public decimal TongTien { get; set; }
    public decimal TienCoc { get; set; }
    public DateTime? HanDatCoc { get; set; }
    public int ChinhSachId { get; set; }
    public TrangThaiYeuCauThue TrangThai { get; set; }
    public int? NguoiDuyetId { get; set; }
    public string? LyDoXuLy { get; set; }
    public DateTime? NgayHuy { get; set; }
    public DateTime? NgayTao { get; set; }
    public string? NoiDungYeuCau { get; set; }
}

public class LichDatDiaDiem
{
    public int Id { get; set; }
    public int DiaDiemId { get; set; }
    public int? YeuCauThueId { get; set; }
    public int? SuatDienId { get; set; }
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }
    public LoaiLichDiaDiem LoaiLich { get; set; }
    public TrangThaiLichDatDiaDiem TrangThai { get; set; }
}
