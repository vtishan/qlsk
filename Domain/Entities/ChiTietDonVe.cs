namespace QLSK.Domain.Entities;
public class ChiTietDonVe
{
    public int Id { get; set; }
    public int DonVeId { get; set; }
    public int LoaiVeId { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
    public DonVe DonVe { get; set; } = null!;
    public LoaiVe LoaiVe { get; set; } = null!;
}
