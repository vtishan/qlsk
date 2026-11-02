using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class Ve
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int VeId { get; set; }

    public int DonVeId { get; set; }

    public int LoaiVeId { get; set; }

    public string MaVe { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("maQR")]
    [System.ComponentModel.DataAnnotations.Schema.Column("MaQR")]
    public string NoiDungMaQr { get; set; } = "";

    public TrangThaiVe TrangThai { get; set; } = TrangThaiVe.ChuaSuDung;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    public DateTime? NgayCheckIn { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rowVersion")]
    [System.ComponentModel.DataAnnotations.Schema.Column("RowVersion")]
    public byte[] PhienBanDong { get; set; } = [];

    public DonVe DonVe { get; set; } = null!;

    public LoaiVe LoaiVe { get; set; } = null!;

    [System.Text.Json.Serialization.JsonPropertyName("xuatVes")]
    public ICollection<XuatVe> LichSuXuatVe { get; set; } = [];
}
