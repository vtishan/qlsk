using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QLSK.Domain.Entities;

public class ChiTietDonVe
{
    [JsonPropertyName("id")]
    [Column("Id")]
    public int ChiTietDonVeId { get; set; }

    public int DonVeId { get; set; }

    public int LoaiVeId { get; set; }

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public decimal ThanhTien { get; set; }

    public DonVe DonVe { get; set; } = null!;

    public LoaiVe LoaiVe { get; set; } = null!;
}
