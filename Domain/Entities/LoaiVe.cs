namespace QLSK.Domain.Entities;
public class LoaiVe
{
    public int Id { get; set; }
    public int SuKienId { get; set; }
    public string Ten { get; set; } = "";
    public decimal Gia { get; set; }
    public int SoLuong { get; set; }
    public int DaBan { get; set; }
    public int DangGiu { get; set; }
    public DateTime MoBan { get; set; }
    public DateTime DongBan { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public SuKien SuKien { get; set; } = null!;
    public ICollection<ChiTietDonVe> ChiTiets { get; set; } = [];
    public ICollection<Ve> Ves { get; set; } = [];
}
