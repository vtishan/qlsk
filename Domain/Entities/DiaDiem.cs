namespace QLSK.Domain.Entities;
public class DiaDiem
{
    public int Id { get; set; }
    public string Ten { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public string? HinhAnh { get; set; }
    public int SucChua { get; set; }
    public decimal GiaThue { get; set; }
    public bool HoatDong { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
    public ICollection<DatDiaDiem> DatCho { get; set; } = [];
}
