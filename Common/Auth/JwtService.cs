using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QLSK.Domain.Entities;

namespace QLSK.Common.Auth;
public class JwtService(IOptions<JwtOptions> options)
{
    public const string PasswordFingerprintClaim = "pwd";
    private readonly JwtOptions cfg = options.Value;

    public static string PasswordFingerprint(string passwordHash)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)));

    public string Tao(TaiKhoan user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.HoTen),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.VaiTro.Ten),
            new Claim(PasswordFingerprintClaim, PasswordFingerprint(user.MatKhau))
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.Key));
        var token = new JwtSecurityToken(cfg.Issuer, cfg.Audience, claims,
            expires: DateTime.UtcNow.AddMinutes(cfg.ExpireMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
