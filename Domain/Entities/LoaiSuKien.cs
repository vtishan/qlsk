namespace QLSK.Domain.Entities;
public class LoaiSuKien
{
    public int Id { get; set; }
    public string Ten { get; set; } = "";
    public string MoTa { get; set; } = "";
    public bool HoatDong { get; set; } = true;
    public ICollection<SuKien> SuKiens { get; set; } = [];
}
