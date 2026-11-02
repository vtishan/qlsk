namespace QLSK.Infrastructure.Email;

public class EmailOptions
{
    [Microsoft.Extensions.Configuration.ConfigurationKeyName("Host")] public string MayChuSmtp { get; set; } = "";
    [Microsoft.Extensions.Configuration.ConfigurationKeyName("Port")] public int CongSmtp { get; set; } = 587;
    [Microsoft.Extensions.Configuration.ConfigurationKeyName("User")] public string TenDangNhapSmtp { get; set; } = "";
    [Microsoft.Extensions.Configuration.ConfigurationKeyName("Password")] public string MatKhauSmtp { get; set; } = "";
    [Microsoft.Extensions.Configuration.ConfigurationKeyName("From")] public string EmailGui { get; set; } = "";
    [Microsoft.Extensions.Configuration.ConfigurationKeyName("EnableSsl")] public bool BatMaHoaSsl { get; set; } = true;
}