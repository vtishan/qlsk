namespace QLSK.Domain.Entities;
public class BanToChuc
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public string Ten { get; set; } = "";
    public string MoTa { get; set; } = "";
    public DateTime NgayXacThuc { get; set; } = DateTime.UtcNow;
    public TaiKhoan TaiKhoan { get; set; } = null!;
    public ICollection<SuKien> SuKiens { get; set; } = [];
}
