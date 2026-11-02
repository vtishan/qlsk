using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.DonVe;
public class TaoDonDto
{
    [System.Text.Json.Serialization.JsonPropertyName("items")] [Required, MinLength(1)] public List<DatVeItemDto> ChiTietDatVe { get; set; } = [];
}
public class DatVeItemDto
{
    [Range(1, int.MaxValue)] public int LoaiVeId { get; set; }
    [Range(1, 20)] public int SoLuong { get; set; }
}
