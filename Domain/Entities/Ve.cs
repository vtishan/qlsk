using QLSK.Domain.Enums;
namespace QLSK.Domain.Entities;
public class Ve
{
    public int Id { get; set; }
    public int DonVeId { get; set; }
    public int LoaiVeId { get; set; }
    public string MaVe { get; set; } = "";
    public string MaQR { get; set; } = "";
    public TrangThaiVe TrangThai { get; set; } = TrangThaiVe.ChuaSuDung;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCheckIn { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public DonVe DonVe { get; set; } = null!;
    public LoaiVe LoaiVe { get; set; } = null!;
    public ICollection<XuatVe> XuatVes { get; set; } = [];
}
