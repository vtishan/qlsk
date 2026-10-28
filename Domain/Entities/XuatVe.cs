namespace QLSK.Domain.Entities;
public class XuatVe
{
    public int Id { get; set; }
    public int VeId { get; set; }
    public string Kieu { get; set; } = "";
    public string? DiaChi { get; set; }
    public bool ThanhCong { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public Ve Ve { get; set; } = null!;
}
