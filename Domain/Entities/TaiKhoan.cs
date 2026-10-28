using System.Text.Json.Serialization;

namespace QLSK.Domain.Entities;
public class TaiKhoan
{
    public int Id { get; set; }
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    [JsonIgnore]
    public string MatKhau { get; set; } = "";
    public int VaiTroId { get; set; }
    public bool HoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
    public VaiTro VaiTro { get; set; } = null!;
    public YeuCauXacThucToChuc? YeuCauXacThuc { get; set; }
    public BanToChuc? BanToChuc { get; set; }
    public ICollection<LichSuYeuCauXacThucToChuc> LichSuYeuCaus { get; set; } = [];
    public ICollection<DonVe> DonVes { get; set; } = [];
}
