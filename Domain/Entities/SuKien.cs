using QLSK.Domain.Enums;
namespace QLSK.Domain.Entities;
public class SuKien
{
    public int Id { get; set; }
    public int BanToChucId { get; set; }
    public int LoaiSuKienId { get; set; }
    public string Ten { get; set; } = "";
    public string MoTa { get; set; } = "";
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }
    public int SoNguoi { get; set; }
    public TrangThaiSuKien TrangThai { get; set; } = TrangThaiSuKien.ChoDuyet;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
    public BanToChuc BanToChuc { get; set; } = null!;
    public LoaiSuKien LoaiSuKien { get; set; } = null!;
    public DatDiaDiem? DatDiaDiem { get; set; }
    public ICollection<LoaiVe> LoaiVes { get; set; } = [];
    public ICollection<DonVe> DonVes { get; set; } = [];
}
