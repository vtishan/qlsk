using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using QLSK.Common.Security;
using Xunit;
using Xunit.Abstractions;

namespace QLSK.Tests;

public sealed class ProtectionFactory(Dictionary<string, string>? settings = null) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", "Server=(localdb)\\MSSQLLocalDB;Database=QLSK_Test_NotOpened;Trusted_Connection=True");
        builder.UseSetting("Jwt:Key", ApiFactory.JwtKey);
        builder.UseSetting("PayOS:ChecksumKey", "test-checksum-key");
        foreach (var item in settings ?? []) builder.UseSetting(item.Key, item.Value);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.AddSingleton<ProbeCounter>();
            services.AddControllers().AddApplicationPart(typeof(SecurityProbeController).Assembly);
        });
    }
}

public sealed class ProbeCounter { public int Value; }

// Registered by ProtectionFactory only, never by the production application.
[ApiController, Route("api/security-probe")]
public sealed class SecurityProbeController(ProbeCounter counter, IOutputCacheStore cache) : ControllerBase
{
    [HttpGet("cached"), OutputCache(PolicyName = "PublicCatalog")]
    public IActionResult Cached() => Ok(new { value = Interlocked.Increment(ref counter.Value) });

    [HttpPost("invalidate")]
    public async Task<IActionResult> Invalidate()
    {
        await cache.EvictByTagAsync("catalog", HttpContext.RequestAborted);
        return Ok();
    }

    [HttpGet("wait")]
    public async Task<IActionResult> Wait(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        return Ok();
    }
}

public sealed class HttpProtectionTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Request_budget_rejects_with_429_retry_after_and_json()
    {
        using var factory = new ProtectionFactory(new() { ["Security:RequestsPerMinute"] = "2" });
        using var client = factory.CreateClient();
        for (var i = 0; i < 2; i++) Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/missing")).StatusCode);
        var rejected = await client.GetAsync("/api/missing");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.True(rejected.Headers.RetryAfter?.Delta > TimeSpan.Zero);
        Assert.Contains("RATE_LIMIT_EXCEEDED", await rejected.Content.ReadAsStringAsync());
        Assert.Equal("no-store", rejected.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Forwarded_for_from_an_untrusted_caller_cannot_reset_the_rate_budget()
    {
        using var factory = new ProtectionFactory(new() { ["Security:RequestsPerMinute"] = "2" });
        using var client = factory.CreateClient();
        for (var i = 0; i < 3; i++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/missing");
            request.Headers.Add("X-Forwarded-For", "192.0.2." + (i + 1));
            var response = await client.SendAsync(request);
            Assert.Equal(i == 2 ? HttpStatusCode.TooManyRequests : HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task Auth_has_a_separate_quota_before_password_hashing()
    {
        using var factory = new ProtectionFactory(new() { ["Security:AuthRequestsPerMinute"] = "2" });
        using var client = factory.CreateClient();
        for (var i = 0; i < 2; i++) Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/login", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/auth/login", new { })).StatusCode);
    }

    [Fact]
    public async Task Full_capacity_returns_503_immediately_and_recovers_after_a_slot_is_released()
    {
        using var factory = new ProtectionFactory(new() { ["Security:MaxConcurrentRequests"] = "1" });
        using var client = factory.CreateClient();
        var capacity = factory.Services.GetRequiredService<ApiCapacity>();
        await capacity.Slots.WaitAsync();
        try
        {
            var response = await client.GetAsync("/api/missing");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal(TimeSpan.FromSeconds(5), response.Headers.RetryAfter?.Delta);
            Assert.Contains("SERVER_BUSY", await response.Content.ReadAsStringAsync());
        }
        finally { capacity.Slots.Release(); }
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/missing")).StatusCode);
    }

    [Fact]
    public async Task Long_request_is_cancelled_and_returns_503_without_exception_details()
    {
        using var factory = new ProtectionFactory(new() { ["Security:RequestTimeoutSeconds"] = "1" });
        using var client = factory.CreateClient();
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync("/api/security-probe/wait");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.InRange(timer.Elapsed.TotalSeconds, 0.8, 5);
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("REQUEST_TIMEOUT", text);
        Assert.DoesNotContain("Exception", text);
    }

    [Fact]
    public async Task Fifty_request_burst_has_a_fixed_admission_budget()
    {
        using var factory = new ProtectionFactory(new() { ["Security:RequestsPerMinute"] = "10" });
        using var client = factory.CreateClient();
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var responses = await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => client.GetAsync("/api/missing")));
        Assert.Equal(10, responses.Count(r => r.StatusCode == HttpStatusCode.NotFound));
        Assert.Equal(40, responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests));
        output.WriteLine($"50 concurrent requests: 10 admitted, 40 HTTP 429; elapsed {timer.ElapsedMilliseconds} ms (in-process test server).");
    }

    [Fact]
    public async Task Cache_coalesces_public_reads_and_a_successful_write_invalidates_it()
    {
        using var factory = new ProtectionFactory();
        using var client = factory.CreateClient();
        var first = await client.GetStringAsync("/api/security-probe/cached");
        var second = await client.GetStringAsync("/api/security-probe/cached");
        Assert.Equal(first, second);
        Assert.Equal(1, factory.Services.GetRequiredService<ProbeCounter>().Value);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/api/security-probe/invalidate", null)).StatusCode);
        Assert.NotEqual(first, await client.GetStringAsync("/api/security-probe/cached"));
    }

    [Theory]
    [InlineData("https://attacker.example")]
    [InlineData("null")]
    public async Task Browser_mutation_from_untrusted_origin_is_rejected(string origin)
    {
        using var factory = new ProtectionFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register") { Content = JsonContent.Create(new { }) };
        request.Headers.Add("Origin", origin);
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("ORIGIN_NOT_ALLOWED", await response.Content.ReadAsStringAsync());
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task CORS_does_not_allow_arbitrary_origins_or_credentials()
    {
        using var factory = new ProtectionFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/orders");
        request.Headers.Add("Origin", "https://attacker.example");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        var response = await client.SendAsync(request);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
    }

    [Fact]
    public async Task Cookies_and_query_tokens_do_not_authenticate_api_requests()
    {
        using var factory = new ProtectionFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "sk_token=pretend-token");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/accounts/me?access_token=pretend-token")).StatusCode);
    }

    [Fact]
    public async Task Oversized_body_is_rejected_before_json_processing()
    {
        using var factory = new ProtectionFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = new string('a', 128 * 1024), matKhau = "unused" });
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Contains("REQUEST_TOO_LARGE", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    public async Task Malformed_unsigned_webhooks_return_400(string data)
    {
        using var factory = new ProtectionFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent("{\"code\":\"00\",\"success\":true,\"data\":" + data + ",\"signature\":\"bad\"}", System.Text.Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/payments/webhook", content)).StatusCode);
    }

    [Fact]
    public async Task Security_headers_cover_static_pages_and_api_errors()
    {
        using var factory = new ProtectionFactory();
        using var client = factory.CreateClient();
        foreach (var path in new[] { "/", "/api/missing" })
        {
            var response = await client.GetAsync(path);
            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
            Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
            Assert.Contains("object-src 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
        }
    }
}
