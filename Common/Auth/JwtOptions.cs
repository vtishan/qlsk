namespace QLSK.Common.Auth;
public class JwtOptions
{
    public string Issuer { get; set; } = "QLSK";
    public string Audience { get; set; } = "QLSK.Client";
    [Microsoft.Extensions.Configuration.ConfigurationKeyName("Key")] public string KhoaKyJwt { get; set; } = "";
    public int ExpireMinutes { get; set; } = 120;
}
