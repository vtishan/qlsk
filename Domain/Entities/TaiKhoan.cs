using System.Text.Json.Serialization;

using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class TaiKhoan
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string HoTen { get; set; } = "";
    public string TenDangNhap { get; set; } = "";

    [JsonIgnore]
    public string MatKhauBam { get; set; } = "";

    public VaiTroTaiKhoan VaiTro { get; set; } = VaiTroTaiKhoan.NguoiDung;
    public TrangThaiTaiKhoan TrangThai { get; set; } = TrangThaiTaiKhoan.HoatDong;
    public DateTime NgayTao { get; set; }
    public DateTime? EmailXacMinhLuc { get; set; }
    public DateTime? SoDienThoaiXacMinhLuc { get; set; }

    [JsonIgnore]
    public string? MaBamKhoiPhuc { get; set; }

    public DateTime? HanKhoiPhuc { get; set; }
    public int PhienBanDangNhap { get; set; }

    [JsonIgnore]
    public string? QuyenQuanLy { get; set; }

    [JsonIgnore]
    public int[]? SuKienYeuThich { get; set; }

    [JsonIgnore]
    public int[]? SuKienDaXem { get; set; }
}
