using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QLSK.Domain.Entities;

public class BanToChuc
{
    [JsonPropertyName("id")]
    [Column("Id")]
    public int BanToChucId { get; set; }

    public int TaiKhoanId { get; set; }

    [JsonPropertyName("ten")]
    [Column("Ten")]
    public string TenBanToChuc { get; set; } = "";

    [JsonPropertyName("moTa")]
    [Column("MoTa")]
    public string MoTaBanToChuc { get; set; } = "";

    public DateTime NgayXacThuc { get; set; } = DateTime.UtcNow;

    public TaiKhoan TaiKhoan { get; set; } = null!;

    [JsonPropertyName("suKiens")]
    public ICollection<SuKien> DanhSachSuKien { get; set; } = [];
}
