using System.Text.Json.Serialization;

using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class HoSoBTC
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public string TenToChuc { get; set; } = "";
    public string NguoiDaiDien { get; set; } = "";
    public string? MaSoThue { get; set; }
    public string DiaChi { get; set; } = "";
    public string ThongTinLienHe { get; set; } = "";
    [JsonIgnore]
    public string? ThongTinNhanTienMaHoa { get; set; }
    public TrangThaiHoSoToChuc TrangThai { get; set; }
    public string? LyDoTuChoi { get; set; }
    public DateTime? NgayNop { get; set; }
    public DateTime? HanDuyet { get; set; }
    public int? NguoiDuyetId { get; set; }
    public DateTime? NgayDuyet { get; set; }
}

public class GiayToBTC
{
    public int Id { get; set; }
    public int HoSoBTCId { get; set; }
    public string LoaiGiayTo { get; set; } = "";
    public string KhoaLuuTru { get; set; } = "";
    public string TenTep { get; set; } = "";
    public string LoaiNoiDung { get; set; } = "";
    public DateTime NgayTaiLen { get; set; }
}

public class PhanCongSuKien
{
    public int Id { get; set; }
    public int SuKienId { get; set; }
    public int? SuatDienId { get; set; }
    public int? TaiKhoanId { get; set; }
    public string? EmailMoi { get; set; }
    [JsonIgnore]
    public string? MaBamLoiMoi { get; set; }
    public DateTime? HanLoiMoi { get; set; }
    public int NguoiPhanCongId { get; set; }
    public string LoaiQuyen { get; set; } = "";
    public DateTime NgayPhanCong { get; set; }
    public DateTime? NgayThuHoi { get; set; }
    public TrangThaiPhanCongSuKien TrangThai { get; set; }
}
