using QLSK.Domain.Enums;
namespace QLSK.Domain.Entities;
public class DatDiaDiem
{
    public int Id { get; set; }
    public int DiaDiemId { get; set; }
    public int SuKienId { get; set; }
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }
    public decimal TienCoc { get; set; }
    public decimal TongTien { get; set; }
    public TrangThaiDatCho TrangThai { get; set; } = TrangThaiDatCho.ChoXacNhan;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
    public DiaDiem DiaDiem { get; set; } = null!;
    public SuKien SuKien { get; set; } = null!;
}
