using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using QLSK.Domain.Enums;

namespace QLSK.Domain.Entities;

public class DatDiaDiem
{
    [JsonPropertyName("id")]
    [Column("Id")]
    public int DatDiaDiemId { get; set; }

    public int DiaDiemId { get; set; }

    public int SuKienId { get; set; }

    [JsonPropertyName("batDau")]
    [Column("BatDau")]
    public DateTime ThoiGianBatDau { get; set; }

    [JsonPropertyName("ketThuc")]
    [Column("KetThuc")]
    public DateTime ThoiGianKetThuc { get; set; }

    public decimal TienCoc { get; set; }

    public decimal TongTien { get; set; }

    public TrangThaiDatCho TrangThai { get; set; } = TrangThaiDatCho.ChoXacNhan;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("rowVersion")]
    [Column("RowVersion")]
    public byte[] PhienBanDong { get; set; } = [];

    public DiaDiem DiaDiem { get; set; } = null!;

    public SuKien SuKien { get; set; } = null!;
}
