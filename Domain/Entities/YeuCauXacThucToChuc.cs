using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class YeuCauXacThucToChuc
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int YeuCauXacThucToChucId { get; set; }

    public int TaiKhoanId { get; set; }

    public string TenToChuc { get; set; } = "";

    public string GiayTo { get; set; } = "";

    public string? LyDo { get; set; }

    public TrangThaiYeuCau TrangThai { get; set; } = TrangThaiYeuCau.ChoDuyet;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    public DateTime? NgayXuLy { get; set; }

    public int? NguoiXuLyId { get; set; }

    public TaiKhoan TaiKhoan { get; set; } = null!;

    public TaiKhoan? NguoiXuLy { get; set; }
}
