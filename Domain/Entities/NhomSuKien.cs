using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class SuKien
{
    public int Id { get; set; }
    public int BTCId { get; set; }
    public int DanhMucId { get; set; }
    public string TenSuKien { get; set; } = "";
    public string MoTa { get; set; } = "";
    public string AnhBia { get; set; } = "";
    public TrangThaiSuKien TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
    public int? PhienBanCongKhaiId { get; set; }
    public bool ChoPhepTuHuyVe { get; set; }
    public DateTime? HanHoanKhiHoan { get; set; }
}

public class PhienBanSuKien
{
    public int Id { get; set; }
    public int SuKienId { get; set; }
    public int SoPhienBan { get; set; }
    public LoaiYeuCauSuKien LoaiYeuCau { get; set; }
    public string NoiDung { get; set; } = "";
    public string? LyDo { get; set; }
    public TrangThaiDuyetPhienBan TrangThaiDuyet { get; set; }
    public int NguoiGuiId { get; set; }
    public DateTime? NgayGui { get; set; }
    public int? NguoiDuyetId { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? KetQuaDuyet { get; set; }
}

public class SuatDien
{
    public int Id { get; set; }
    public int SuKienId { get; set; }
    public int DiaDiemId { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public DateTime ThoiGianMoBan { get; set; }
    public DateTime ThoiGianDongBan { get; set; }
    public string SoDoChot { get; set; } = "";
    public int SucChua { get; set; }
    public TrangThaiSuatDien TrangThai { get; set; }
}

public class DanhMucSuKien
{
    public int Id { get; set; }
    public string TenDanhMuc { get; set; } = "";
    public string? MoTa { get; set; }
    public TrangThaiHienThi TrangThai { get; set; }
}
