namespace QLSK.Common.Auth;
public class JwtOptions
{
    public string Issuer { get; set; } = "QLSK";
    public string Audience { get; set; } = "QLSK.Client";
    public string Key { get; set; } = "";
    public int ExpireMinutes { get; set; } = 120;
}
