using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QLSK.Domain.Entities;

namespace QLSK.Common.Auth;
public class JwtService(IOptions<JwtOptions> tuyChonJwt)
{
    public const string PasswordFingerprintClaim = "pwd";
    private readonly JwtOptions cauHinh = tuyChonJwt.Value;

    public static string TaoDauVanTayMatKhau(string matKhauDaBam)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(matKhauDaBam)));

    public string TaoTokenDangNhap(TaiKhoan taiKhoan)
    {
        var cacClaimTaiKhoan = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, taiKhoan.TaiKhoanId.ToString()),
            new Claim(ClaimTypes.Name, taiKhoan.HoTen),
            new Claim(ClaimTypes.Email, taiKhoan.Email),
            new Claim(ClaimTypes.Role, taiKhoan.VaiTro.TenVaiTro),
            new Claim(PasswordFingerprintClaim, TaoDauVanTayMatKhau(taiKhoan.MatKhauBam))
        };
        var khoaKyDangNhap = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cauHinh.KhoaKyJwt));
        var tokenDangNhap = new JwtSecurityToken(cauHinh.Issuer, cauHinh.Audience, cacClaimTaiKhoan,
            expires: DateTime.UtcNow.AddMinutes(cauHinh.ExpireMinutes),
            signingCredentials: new SigningCredentials(khoaKyDangNhap, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(tokenDangNhap);
    }
}
