using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Timeouts;
using QLSK.Common.Models;

namespace QLSK.Common.Security;

public static class HttpProtection
{
    public static IServiceCollection AddHttpProtection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ApiCapacity>();
        services.AddSingleton<LoginAttemptLimiter>();
        services.AddRequestTimeouts(options => options.DefaultPolicy = new RequestTimeoutPolicy
        {
            Timeout = TimeSpan.FromSeconds(configuration.GetValue("Security:RequestTimeoutSeconds", 30)),
            TimeoutStatusCode = StatusCodes.Status503ServiceUnavailable,
            WriteTimeoutResponse = context => WriteBusy(context, "REQUEST_TIMEOUT", 5)
        });
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(Ip(context), _ => Window(
                        configuration.GetValue("Security:RequestsPerMinute", 240)))),
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var path = context.Request.Path;
                    if (path.StartsWithSegments("/api/auth") && path != "/api/auth/logout")
                        return RateLimitPartition.GetFixedWindowLimiter("auth:" + Ip(context), _ => Window(
                            configuration.GetValue("Security:AuthRequestsPerMinute", 15)));
                    // The signed gateway callback has its own quota; an order burst cannot starve it.
                    if (path == "/api/payments/webhook")
                        return RateLimitPartition.GetFixedWindowLimiter("webhook:" + Ip(context), _ => Window(180));
                    if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsOptions(context.Request.Method))
                        return RateLimitPartition.GetNoLimiter("read");
                    var subject = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    return RateLimitPartition.GetFixedWindowLimiter("write:" + (subject ?? Ip(context)), _ => Window(
                        configuration.GetValue("Security:WritesPerMinute", 30)));
                }),
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var path = context.Request.Path.Value ?? "";
                    if (!path.EndsWith("/email", StringComparison.OrdinalIgnoreCase) &&
                        !path.EndsWith("/export", StringComparison.OrdinalIgnoreCase))
                        return RateLimitPartition.GetNoLimiter("normal");
                    var subject = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    return RateLimitPartition.GetFixedWindowLimiter("export:" + (subject ?? Ip(context)), _ => Window(5));
                }),
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var path = context.Request.Path.Value ?? "";
                    var expensiveAuth = path is "/api/auth/login" or "/api/auth/register" or "/api/auth/change-password";
                    var export = path.EndsWith("/export", StringComparison.OrdinalIgnoreCase) || path.EndsWith("/email", StringComparison.OrdinalIgnoreCase);
                    return expensiveAuth || export
                        ? RateLimitPartition.GetConcurrencyLimiter(expensiveAuth ? "password-work" : "ticket-export-work", _ => new()
                        {
                            PermitLimit = 4, QueueLimit = 0, QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        })
                        : RateLimitPartition.GetNoLimiter("lightweight");
                }));
            options.OnRejected = async (rejected, token) =>
            {
                var seconds = rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)) : 5;
                rejected.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                await rejected.HttpContext.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai(
                    "RATE_LIMIT_EXCEEDED", "Bạn gửi yêu cầu quá nhanh. Vui lòng thử lại sau."), token);
            };
        });
        return services;
    }

    private static FixedWindowRateLimiterOptions Window(int limit) => new()
    {
        PermitLimit = Math.Clamp(limit, 1, 10000), Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0, AutoReplenishment = true
    };

    private static string Ip(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    public static Task WriteBusy(HttpContext context, string code, int retry)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = retry.ToString(CultureInfo.InvariantCulture);
        return context.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai(
            code, "Dịch vụ đang bận. Vui lòng thử lại sau."));
    }
}

// One bounded admission gate per process. No waiting queue or task accumulation.
public sealed class ApiCapacity(IConfiguration configuration) : IDisposable
{
    public SemaphoreSlim Slots { get; } = new(Math.Clamp(configuration.GetValue("Security:MaxConcurrentRequests", 64), 1, 2048));
    public void Dispose() => Slots.Dispose();
}

public sealed class ApiCapacityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApiCapacity capacity)
    {
        if (!await capacity.Slots.WaitAsync(0, context.RequestAborted))
        {
            await HttpProtection.WriteBusy(context, "SERVER_BUSY", 5);
            return;
        }
        try { await next(context); }
        finally { capacity.Slots.Release(); }
    }
}
