using QLSK.Domain.Enums;
namespace QLSK.Domain.Entities;
public class DonVe
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public int SuKienId { get; set; }
    public int SoLuong { get; set; }
    public decimal TongTien { get; set; }
    public TrangThaiDon TrangThai { get; set; } = TrangThaiDon.ChoThanhToan;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? HetHan { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public TaiKhoan TaiKhoan { get; set; } = null!;
    public SuKien SuKien { get; set; } = null!;
    public ICollection<ChiTietDonVe> ChiTiets { get; set; } = [];
    public ICollection<ThanhToan> ThanhToans { get; set; } = [];
    public ICollection<Ve> Ves { get; set; } = [];
}
