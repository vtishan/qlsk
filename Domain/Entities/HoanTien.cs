namespace QLSK.Domain.Entities;

// An append-only receipt prevents retries from recording the same refund twice.
public class HoanTien
{
    public int HoanTienId { get; set; }
    public int ThanhToanId { get; set; }
    public string MaHoanTien { get; set; } = "";
    public decimal SoTien { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}
