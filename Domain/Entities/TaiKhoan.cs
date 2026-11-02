using System.Text.Json.Serialization;

namespace QLSK.Domain.Entities;

public class TaiKhoan
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int TaiKhoanId { get; set; }

    public string HoTen { get; set; } = "";

    public string Email { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("matKhau")]
    [System.ComponentModel.DataAnnotations.Schema.Column("MatKhau")]
    [JsonIgnore]
    public string MatKhauBam { get; set; } = "";

    public int VaiTroId { get; set; }

    public bool HoatDong { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonPropertyName("rowVersion")]
    [System.ComponentModel.DataAnnotations.Schema.Column("RowVersion")]
    public byte[] PhienBanDong { get; set; } = [];

    public VaiTro VaiTro { get; set; } = null!;

    public YeuCauXacThucToChuc? YeuCauXacThuc { get; set; }

    public BanToChuc? BanToChuc { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("lichSuYeuCaus")]
    public ICollection<LichSuYeuCauXacThucToChuc> LichSuYeuCauXacThuc { get; set; } = [];

    [System.Text.Json.Serialization.JsonPropertyName("donVes")]
    public ICollection<DonVe> DanhSachDonVe { get; set; } = [];
}
