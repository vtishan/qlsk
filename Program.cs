using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Features.Accounts;
using QLSK.Features.Capabilities;
using QLSK.Features.Catalog;
using QLSK.Features.Organizers;
using QLSK.Features.Requests;
using QLSK.Features.Operations;
using QLSK.Features.Events;
using QLSK.Features.Tickets;
using QLSK.Features.Admissions;
using QLSK.Features.Reports;
using QLSK.Features.Finance;
using System.Threading.RateLimiting;

const string initialize = "--qlsk-init";
const string seedDemo = "--qlsk-seed-demo";
var builder = WebApplication.CreateBuilder(args.Where(arg => arg != initialize && arg != seedDemo).ToArray());
var connection = builder.Configuration.GetConnectionString("QLSK");
var configured = !string.IsNullOrWhiteSpace(connection);
if (configured)
{
    builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
    builder.Services.AddScoped<AccountService>();
    builder.Services.AddScoped<CatalogService>();
    builder.Services.AddScoped<OrganizerService>();
    builder.Services.AddScoped<RequestService>();
    builder.Services.AddHttpClient<RentalPayOs>(client => client.Timeout = TimeSpan.FromSeconds(15));
    builder.Services.AddScoped<RefundService>();
    builder.Services.AddScoped<OperationsService>();
    builder.Services.AddScoped<CatalogAdminService>();
    builder.Services.AddScoped<EventService>();
    builder.Services.AddScoped<ScheduleService>();
    builder.Services.AddScoped<TicketService>();
    builder.Services.AddScoped<OrderService>();
    builder.Services.AddScoped<VoucherService>();
    builder.Services.AddScoped<AdmissionService>();
    builder.Services.AddScoped<StaffGrantService>();
    builder.Services.AddScoped<ReportService>();
    builder.Services.AddScoped<FinanceService>();
    builder.Services.AddScoped<CapabilityService>();
    builder.Services.AddScoped<SessionCookie>();
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
    {
        options.Cookie.Name = "qlsk.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
        options.EventsType = typeof(SessionCookie);
    });
    builder.Services.AddAuthorization();
    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-Token";
        options.Cookie.Name = "qlsk.csrf";
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    });
    builder.Services.AddDataProtection().SetApplicationName("QLSK");
    builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 24 * 1024 * 1024);
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("qlsk-identity", context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(5) }));
    });
}

var app = builder.Build();
if (args.Contains(initialize) || args.Contains(seedDemo))
{
    if (!configured) throw new InvalidOperationException("Cần cấu hình ConnectionStrings:QLSK.");
    await using var scope = app.Services.CreateAsyncScope();
    await DatabaseSetup.Run(scope.ServiceProvider, app.Environment, args.Contains(seedDemo));
    return;
}

var defaults = new DefaultFilesOptions();
app.UseRewriter(FrontendRedirects.FromPreviousLayout());
defaults.DefaultFileNames.Clear();
defaults.DefaultFileNames.Add("index.html");
defaults.DefaultFileNames.Add("giaodienmau.html");
app.UseDefaultFiles(defaults);
app.UseStaticFiles();

if (configured)
{
    app.UseRouting();
    app.Use(ApiRequest.Handle);
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.Use(ApiRequest.ValidateMutation);
    app.MapControllers();
}
else
{
    app.MapGet("/api/capabilities", (HttpContext context) =>
    {
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(new { message = "Chưa cấu hình kết nối SQL Server cho QLSK." }, statusCode: 503);
    });
}
app.Run();

