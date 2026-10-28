using QLSK.Domain.Enums;
namespace QLSK.Domain.Entities;
public class LichSuYeuCauXacThucToChuc
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public string TenToChuc { get; set; } = "";
    public string GiayTo { get; set; } = "";
    public string? LyDo { get; set; }
    public TrangThaiYeuCau TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayXuLy { get; set; }
    public int? NguoiXuLyId { get; set; }
    public TaiKhoan TaiKhoan { get; set; } = null!;
    public TaiKhoan? NguoiXuLy { get; set; }
}
