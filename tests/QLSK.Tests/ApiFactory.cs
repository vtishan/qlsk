using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QLSK.Common.Auth;
using QLSK.Data;
using QLSK.Infrastructure.Email;
using QLSK.Infrastructure.PayOS;

namespace QLSK.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string JwtKey = "QLSK_TEST_ONLY_KEY_NOT_FOR_DEPLOYMENT_32_BYTES";
    private readonly string tenCoSoDuLieuKiemThu = $"QLSK_Test_{Guid.NewGuid():N}";
    private IHost? hostKiemThuDaKhoiTao;
    private string ConnectionString => $"Server=.\\SQL2022EXP;Database={tenCoSoDuLieuKiemThu};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    protected override IHost CreateHost(IHostBuilder boDungHost)
    {
        var hostKiemThu = base.CreateHost(boDungHost);
        try
        {
            // Database setup belongs to the test host, never application startup.
            using var phamViDichVu = hostKiemThu.Services.CreateScope();
            var nguCanhDuLieu = phamViDichVu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            KiemTraCoSoDuLieuThuocPhienKiemThu(nguCanhDuLieu);
            nguCanhDuLieu.Database.Migrate();
            hostKiemThuDaKhoiTao = hostKiemThu;
            return hostKiemThu;
        }
        catch
        {
            hostKiemThu.Dispose();
            throw;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder boDungWebHost)
    {
        boDungWebHost.UseEnvironment("Testing");
        boDungWebHost.UseSetting("ConnectionStrings:Default", ConnectionString);
        boDungWebHost.UseSetting("Jwt:Issuer", "QLSK");
        boDungWebHost.UseSetting("Jwt:Audience", "QLSK.Client");
        boDungWebHost.UseSetting("Jwt:Key", JwtKey);
        boDungWebHost.UseSetting("PayOS:ClientId", "test-client-id");
        boDungWebHost.UseSetting("PayOS:ApiKey", "test-api-key");
        boDungWebHost.UseSetting("PayOS:ChecksumKey", "test-checksum-key");
        boDungWebHost.UseSetting("Email:Host", "");

        boDungWebHost.ConfigureServices(danhSachDichVu =>
        {
            danhSachDichVu.RemoveAll<IHostedService>();
            danhSachDichVu.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            danhSachDichVu.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            danhSachDichVu.RemoveAll<EmailService>();
            danhSachDichVu.AddScoped<EmailService, TestEmailService>();
            danhSachDichVu.AddHttpClient<PayOsService>(clientYeuCauHttp =>
                clientYeuCauHttp.BaseAddress = new Uri("https://api-merchant.payos.vn/"))
                .ConfigurePrimaryHttpMessageHandler(() => new TestPayOsHandler());

            danhSachDichVu.AddDbContext<ApplicationDbContext>(tuyChonCoSoDuLieu =>
                tuyChonCoSoDuLieu.UseSqlServer(ConnectionString));
        });
    }

    public HttpClient CreateAuthenticatedClient(int taiKhoanId, string tenVaiTro, string? matKhauBamThayThe = null)
    {
        var matKhauDaBam = matKhauBamThayThe;
        if (matKhauDaBam is null)
        {
            using var phamViDichVu = Services.CreateScope();
            var nguCanhDuLieu = phamViDichVu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            matKhauDaBam = nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking()
                .Where(taiKhoanDb => taiKhoanDb.TaiKhoanId == taiKhoanId)
                .Select(taiKhoanDb => taiKhoanDb.MatKhauBam)
                .Single();
        }
        var clientYeuCauHttp = CreateClient();
        clientYeuCauHttp.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TaoTokenTaiKhoanKiemThu(taiKhoanId, tenVaiTro, matKhauDaBam));
        return clientYeuCauHttp;
    }

    public async Task ThucThiVoiCoSoDuLieuKiemThu(Func<ApplicationDbContext, Task> thaoTacCanThucThi)
    {
        using var phamViDichVu = Services.CreateScope();
        var nguCanhDuLieu = phamViDichVu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await thaoTacCanThucThi(nguCanhDuLieu);
    }

    public async Task<TKetQua> ThucThiVoiCoSoDuLieuKiemThu<TKetQua>(Func<ApplicationDbContext, Task<TKetQua>> thaoTacCanThucThi)
    {
        using var phamViDichVu = Services.CreateScope();
        var nguCanhDuLieu = phamViDichVu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await thaoTacCanThucThi(nguCanhDuLieu);
    }

    protected override void Dispose(bool dangGiaiPhongTaiNguyen)
    {
        if (dangGiaiPhongTaiNguyen && hostKiemThuDaKhoiTao is not null)
        {
            try
            {
                using var phamViDichVu = hostKiemThuDaKhoiTao.Services.CreateScope();
                var nguCanhDuLieu = phamViDichVu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                KiemTraCoSoDuLieuThuocPhienKiemThu(nguCanhDuLieu);
                nguCanhDuLieu.Database.EnsureDeleted();
            }
            catch
            {
                // The test database may not have been created when host startup fails.
            }
        }

        base.Dispose(dangGiaiPhongTaiNguyen);
    }

    private void KiemTraCoSoDuLieuThuocPhienKiemThu(ApplicationDbContext nguCanhDuLieu)
    {
        var boPhanTichChuoiKetNoi = new SqlConnectionStringBuilder(nguCanhDuLieu.Database.GetConnectionString()
            ?? throw new InvalidOperationException("An isolated SQL Server test connection is required."));
        if (boPhanTichChuoiKetNoi.InitialCatalog != tenCoSoDuLieuKiemThu || boPhanTichChuoiKetNoi.DataSource != ".\\SQL2022EXP")
            throw new InvalidOperationException("Test database setup and cleanup require this factory's isolated local database.");
    }

    private static string TaoTokenTaiKhoanKiemThu(int taiKhoanId, string tenVaiTro, string matKhauDaBam)
    {
        var cacClaimTaiKhoan = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, taiKhoanId.ToString()),
            new Claim(ClaimTypes.Name, $"Test {tenVaiTro}"),
            new Claim(ClaimTypes.Email, $"test-{taiKhoanId}@local.test"),
            new Claim(ClaimTypes.Role, tenVaiTro),
            new Claim(JwtService.PasswordFingerprintClaim, JwtService.TaoDauVanTayMatKhau(matKhauDaBam))
        };
        var khoaKyDangNhap = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var tokenDangNhap = new JwtSecurityToken(
            "QLSK",
            "QLSK.Client",
            cacClaimTaiKhoan,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(khoaKyDangNhap, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(tokenDangNhap);
    }
}

// Gateway responses exist only in this test assembly. Unexpected calls are rejected.
public sealed class TestPayOsHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage yeuCauHttp, CancellationToken tokenHuyThaoTac)
    {
        if (yeuCauHttp.Method != HttpMethod.Post || yeuCauHttp.RequestUri?.Host != "api-merchant.payos.vn" ||
            yeuCauHttp.RequestUri.AbsolutePath != "/v2/payment-requests" || yeuCauHttp.Content is null)
            throw new InvalidOperationException("Unexpected payment gateway request in isolated API tests.");

        using var taiLieuJsonPhanHoi = JsonDocument.Parse(await yeuCauHttp.Content.ReadAsStringAsync(tokenHuyThaoTac));
        var maDonVe = taiLieuJsonPhanHoi.RootElement.GetProperty("orderCode").GetInt64();
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                code = "00",
                data = new
                {
                    checkoutUrl = $"https://checkout.test/pay/{maDonVe}",
                    paymentLinkId = $"test-payment-{maDonVe}"
                }
            })
        };
    }
}

public sealed class TestEmailService() : EmailService(Options.Create(new EmailOptions()))
{
    public override Task GuiEmailVe(
        string emailNhan,
        string tieuDe,
        string noiDung,
        byte[]? tepPdfVe = null) => Task.CompletedTask;
}
