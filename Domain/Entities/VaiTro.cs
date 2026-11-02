namespace QLSK.Domain.Entities;

public class VaiTro
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Id")]
    public int VaiTroId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("ten")]
    [System.ComponentModel.DataAnnotations.Schema.Column("Ten")]
    public string TenVaiTro { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("moTa")]
    [System.ComponentModel.DataAnnotations.Schema.Column("MoTa")]
    public string MoTaVaiTro { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("taiKhoans")]
    public ICollection<TaiKhoan> DanhSachTaiKhoan { get; set; } = [];
}
