using QLSK.Domain.Enums;
namespace QLSK.Domain.Entities;
public class ThanhToan
{
    public int Id { get; set; }
    public int DonVeId { get; set; }
    public decimal SoTien { get; set; }
    public decimal SoTienHoan { get; set; }
    public string? MaGiaoDich { get; set; }
    public string? MaLienKet { get; set; }
    public string? MaHoanTien { get; set; }
    public TrangThaiThanhToan TrangThai { get; set; } = TrangThaiThanhToan.DangXuLy;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayThanhToan { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public DonVe DonVe { get; set; } = null!;
}
