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
    private readonly string databaseName = $"QLSK_Test_{Guid.NewGuid():N}";
    private IHost? initializedHost;
    private string ConnectionString => $"Server=.\\SQL2022EXP;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        try
        {
            // Database setup belongs to the test host, never application startup.
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            RequireOwnedDatabase(db);
            db.Database.Migrate();
            initializedHost = host;
            return host;
        }
        catch
        {
            host.Dispose();
            throw;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
        builder.UseSetting("Jwt:Issuer", "QLSK");
        builder.UseSetting("Jwt:Audience", "QLSK.Client");
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("PayOS:ClientId", "test-client-id");
        builder.UseSetting("PayOS:ApiKey", "test-api-key");
        builder.UseSetting("PayOS:ChecksumKey", "test-checksum-key");
        builder.UseSetting("Email:Host", "");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<EmailService>();
            services.AddScoped<EmailService, TestEmailService>();
            services.AddHttpClient<PayOsService>(client =>
                client.BaseAddress = new Uri("https://api-merchant.payos.vn/"))
                .ConfigurePrimaryHttpMessageHandler(() => new TestPayOsHandler());

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(ConnectionString));
        });
    }

    public HttpClient CreateAuthenticatedClient(int userId, string role, string? passwordHashOverride = null)
    {
        var passwordHash = passwordHashOverride;
        if (passwordHash is null)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            passwordHash = db.TaiKhoans.AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.MatKhau)
                .Single();
        }
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken(userId, role, passwordHash));
        return client;
    }

    public async Task WithDb(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(db);
    }

    public async Task<T> WithDb<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(db);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && initializedHost is not null)
        {
            try
            {
                using var scope = initializedHost.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                RequireOwnedDatabase(db);
                db.Database.EnsureDeleted();
            }
            catch
            {
                // The test database may not have been created when host startup fails.
            }
        }

        base.Dispose(disposing);
    }

    private void RequireOwnedDatabase(ApplicationDbContext db)
    {
        var connection = new SqlConnectionStringBuilder(db.Database.GetConnectionString()
            ?? throw new InvalidOperationException("An isolated SQL Server test connection is required."));
        if (connection.InitialCatalog != databaseName || connection.DataSource != ".\\SQL2022EXP")
            throw new InvalidOperationException("Test database setup and cleanup require this factory's isolated local database.");
    }

    private static string CreateToken(int userId, string role, string passwordHash)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"Test {role}"),
            new Claim(ClaimTypes.Email, $"test-{userId}@local.test"),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtService.PasswordFingerprintClaim, JwtService.PasswordFingerprint(passwordHash))
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var token = new JwtSecurityToken(
            "QLSK",
            "QLSK.Client",
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Gateway responses exist only in this test assembly. Unexpected calls are rejected.
public sealed class TestPayOsHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Post || request.RequestUri?.Host != "api-merchant.payos.vn" ||
            request.RequestUri.AbsolutePath != "/v2/payment-requests" || request.Content is null)
            throw new InvalidOperationException("Unexpected payment gateway request in isolated API tests.");

        using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
        var orderCode = body.RootElement.GetProperty("orderCode").GetInt64();
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                code = "00",
                data = new
                {
                    checkoutUrl = $"https://checkout.test/pay/{orderCode}",
                    paymentLinkId = $"test-payment-{orderCode}"
                }
            })
        };
    }
}

public sealed class TestEmailService() : EmailService(Options.Create(new EmailOptions()))
{
    public override Task Gui(
        string to,
        string subject,
        string body,
        byte[]? attachment = null) => Task.CompletedTask;
}
