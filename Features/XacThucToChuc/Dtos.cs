using System.ComponentModel.DataAnnotations;
namespace QLSK.Features.XacThucToChuc;
public class GuiYeuCauDto
{
    [Required, MaxLength(200)] public string TenToChuc { get; set; } = "";
    [Required, MaxLength(500)] public string GiayTo { get; set; } = "";
}
public class TuChoiYeuCauDto
{
    [Required, MaxLength(500)] public string LyDo { get; set; } = "";
}
