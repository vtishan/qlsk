using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QLSK.Domain.Entities;

public class DiaDiem
{
    [JsonPropertyName("id")]
    [Column("Id")]
    public int DiaDiemId { get; set; }

    [JsonPropertyName("ten")]
    [Column("Ten")]
    public string TenDiaDiem { get; set; } = "";

    public string DiaChi { get; set; } = "";

    public string? HinhAnh { get; set; }

    public int SucChua { get; set; }

    public decimal GiaThue { get; set; }

    public bool HoatDong { get; set; } = true;

    [JsonPropertyName("rowVersion")]
    [Column("RowVersion")]
    public byte[] PhienBanDong { get; set; } = [];

    [JsonPropertyName("datCho")]
    public ICollection<DatDiaDiem> DanhSachDatDiaDiem { get; set; } = [];
}
