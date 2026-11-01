using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using QLSK.Common.Auth;
using QLSK.Common.Exceptions;
using QLSK.Common.Models;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Features.DiaDiem;
using QLSK.Features.DonVe;
using QLSK.Features.LoaiVe;
using QLSK.Features.QuanTri;
using QLSK.Features.SuKien;
using QLSK.Features.TaiKhoan;
using QLSK.Features.ThanhToan;
using QLSK.Features.ThongKe;
using QLSK.Features.Ve;
using QLSK.Features.XacThucToChuc;
using QLSK.Features.XuatVe;
using QLSK.Infrastructure;
using QLSK.Infrastructure.Email;
using QLSK.Infrastructure.File;
using QLSK.Infrastructure.PayOS;
using QLSK.Infrastructure.QR;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers(o => o.Filters.Add<ActiveUserFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddScoped<ActiveUserFilter>();
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Cần cấu hình ConnectionStrings:Default trước khi khởi động.");
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<PayOsOptions>(builder.Configuration.GetSection("PayOS"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IPasswordHasher<TaiKhoan>, PasswordHasher<TaiKhoan>>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<TaiKhoanService>();
builder.Services.AddScoped<XacThucService>();
builder.Services.AddScoped<SuKienService>();
builder.Services.AddScoped<DiaDiemService>();
builder.Services.AddScoped<LoaiVeService>();
builder.Services.AddScoped<DonVeService>();
builder.Services.AddScoped<ThanhToanService>();
builder.Services.AddScoped<VeService>();
builder.Services.AddScoped<XuatVeService>();
builder.Services.AddScoped<ThongKeService>();
builder.Services.AddScoped<QuanTriService>();
builder.Services.AddSingleton<QrService>();
builder.Services.AddSingleton<VePdfService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHttpClient<PayOsService>(c => c.BaseAddress = new Uri("https://api-merchant.payos.vn/"));
builder.Services.AddHostedService<ExpiredOrderWorker>();
builder.Services.AddHostedService<EventStateWorker>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (Encoding.UTF8.GetByteCount(jwt.Key) < 32) throw new InvalidOperationException("Jwt:Key phải có ít nhất 32 byte.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "QLSK", Version = "v1" });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT theo dạng: Bearer {token}"
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5000", "http://127.0.0.1:5000"];
builder.Services.AddCors(o => o.AddPolicy("Frontend", p => p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = context =>
{
    var loi = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
    return new BadRequestObjectResult(ApiResponse.Fail("VALIDATION_ERROR", string.IsNullOrWhiteSpace(loi) ? "Dữ liệu không hợp lệ." : loi));
});

var app = builder.Build();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseCors("Frontend");
app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.HasStarted || response.ContentLength.GetValueOrDefault() > 0) return;
    response.ContentType = "application/json";
    var status = response.StatusCode;
    var code = status switch
    {
        StatusCodes.Status400BadRequest => "VALIDATION_ERROR",
        StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
        StatusCodes.Status403Forbidden => "FORBIDDEN",
        StatusCodes.Status404NotFound => "NOT_FOUND",
        _ => "REQUEST_ERROR"
    };
    await response.WriteAsJsonAsync(ApiResponse.Fail(code, $"Yêu cầu thất bại với mã {status}."));
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("Serving frontend from: {WebRootPath}", app.Environment.WebRootPath);
    foreach (var url in app.Urls)
        app.Logger.LogInformation("Frontend: {FrontendUrl}/ | Backend API: {ApiUrl}/api", url.TrimEnd('/'), url.TrimEnd('/'));
});
app.Run();

public partial class Program { }
