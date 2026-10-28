namespace QLSK.Infrastructure.PayOS;
public class PayOsOptions
{
    public string ClientId { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ChecksumKey { get; set; } = "";
    public string ReturnUrl { get; set; } = "http://localhost:5291/pages/don-ve/don-cua-toi.html";
    public string CancelUrl { get; set; } = "http://localhost:5291/pages/don-ve/don-cua-toi.html";
}
