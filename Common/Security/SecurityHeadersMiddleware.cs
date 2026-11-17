using QLSK.Common.Models;

namespace QLSK.Common.Security;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["X-Request-ID"] = context.TraceIdentifier;
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            // Existing pages use inline event handlers. Disallow eval, plugins, framing and remote scripts.
            headers.ContentSecurityPolicy = "default-src 'self'; script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline'; img-src 'self' data: blob: https: http:; " +
                "font-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'self'; " +
                "frame-ancestors 'none'; form-action 'self'";
            if (context.Request.IsHttps) headers.StrictTransportSecurity = "max-age=31536000";
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                headers.CacheControl = "no-store";
                headers.Pragma = "no-cache";
            }
            return Task.CompletedTask;
        });

        // Auth uses an explicit Bearer header, never cookies/query strings. Also reject untrusted browser writes.
        var request = context.Request;
        if (request.Path.StartsWithSegments("/api") && request.ContentLength > 128 * 1024)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("REQUEST_TOO_LARGE", "Nội dung yêu cầu vượt giới hạn cho phép."));
            return;
        }
        if (request.Path.StartsWithSegments("/api") &&
            request.Path != "/api/payments/webhook" &&
            !HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method) && !HttpMethods.IsOptions(request.Method) &&
            request.Headers.TryGetValue("Origin", out var origin))
        {
            var sameOrigin = string.Equals(origin, $"{request.Scheme}://{request.Host}", StringComparison.OrdinalIgnoreCase);
            var trusted = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            if (!sameOrigin && !trusted.Contains(origin.ToString(), StringComparer.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("ORIGIN_NOT_ALLOWED", "Nguồn yêu cầu không được phép."));
                return;
            }
        }
        await next(context);
    }
}
