namespace QLSK.Domain.Entities;
public class VaiTro
{
    public int Id { get; set; }
    public string Ten { get; set; } = "";
    public string MoTa { get; set; } = "";
    public ICollection<TaiKhoan> TaiKhoans { get; set; } = [];
}
