using System.Text;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using QuestPDF.Infrastructure;

using QLSK.Common.Security;
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


// ================================
// 1. KHỞI TẠO ỨNG DỤNG
// ================================

var boDungUngDung = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
boDungUngDung.Services.AddHttpProtection(boDungUngDung.Configuration);
boDungUngDung.Services.AddResponseCompression(options => options.EnableForHttps = true);
boDungUngDung.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
    options.Level = System.IO.Compression.CompressionLevel.Fastest);
boDungUngDung.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
    options.Level = System.IO.Compression.CompressionLevel.Fastest);
var knownProxies = boDungUngDung.Configuration.GetSection("Security:KnownProxies").Get<string[]>() ?? [];
if (knownProxies.Length > 0)
{
    boDungUngDung.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();
        foreach (var proxy in knownProxies) options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
    });
}
boDungUngDung.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 128 * 1024;
    options.Limits.MaxRequestHeadersTotalSize = 16 * 1024;
    options.Limits.MaxRequestLineSize = 4096;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10);
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
    options.Limits.MaxConcurrentConnections = 512;
    options.Limits.MaxConcurrentUpgradedConnections = 16;
});
boDungUngDung.Services.Configure<Microsoft.AspNetCore.Builder.IISServerOptions>(options =>
    options.MaxRequestBodySize = 128 * 1024);
// Development gets a new unpredictable key each run unless user-secrets supplies one.
if (boDungUngDung.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(boDungUngDung.Configuration["Jwt:Key"]))
    boDungUngDung.Configuration["Jwt:Key"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));


// ================================
// 2. CONTROLLER + JSON
// ================================

boDungUngDung.Services
    .AddControllers(tuyChonMvc =>
    {
        tuyChonMvc.Filters.Add<ActiveUserFilter>();
        tuyChonMvc.Filters.Add<PaginationResultFilter>();
        tuyChonMvc.Filters.Add<CatalogCacheFilter>();
    })
    .AddJsonOptions(tuyChonJson =>
    {
        tuyChonJson.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
        tuyChonJson.JsonSerializerOptions.MaxDepth = 32;
    });

boDungUngDung.Services.AddScoped<ActiveUserFilter>();
boDungUngDung.Services.AddOutputCache(options =>
{
    options.SizeLimit = 4 * 1024 * 1024;
    options.MaximumBodySize = 128 * 1024;
    options.AddPolicy("PublicCatalog", policy => policy.Expire(TimeSpan.FromSeconds(30)).Tag("catalog"));
});


// ================================
// 3. DATABASE
// ================================

var chuoiKetNoiCoSoDuLieu =
    boDungUngDung.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(chuoiKetNoiCoSoDuLieu))
{
    throw new InvalidOperationException(
        "Cần cấu hình ConnectionStrings:Default trước khi khởi động."
    );
}

boDungUngDung.Services.AddDbContext<ApplicationDbContext>(tuyChonDb =>
{
    tuyChonDb.UseSqlServer(chuoiKetNoiCoSoDuLieu, sql =>
    {
        sql.CommandTimeout(15);
        sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});


// ================================
// 4. CẤU HÌNH OPTIONS
// ================================

boDungUngDung.Services.Configure<JwtOptions>(
    boDungUngDung.Configuration.GetSection("Jwt")
);

boDungUngDung.Services.Configure<PayOsOptions>(
    boDungUngDung.Configuration.GetSection("PayOS")
);

boDungUngDung.Services.Configure<EmailOptions>(
    boDungUngDung.Configuration.GetSection("Email")
);


// ================================
// 5. ĐĂNG KÝ SERVICE
// ================================

// Tài khoản + xác thực
boDungUngDung.Services.AddScoped<
    IPasswordHasher<TaiKhoan>,
    PasswordHasher<TaiKhoan>
>();

boDungUngDung.Services.AddScoped<JwtService>();
boDungUngDung.Services.AddScoped<TaiKhoanService>();
boDungUngDung.Services.AddScoped<XacThucService>();

// Nghiệp vụ sự kiện
boDungUngDung.Services.AddScoped<SuKienService>();
boDungUngDung.Services.AddScoped<DiaDiemService>();
boDungUngDung.Services.AddScoped<LoaiVeService>();
boDungUngDung.Services.AddScoped<DonVeService>();

// Thanh toán + vé
boDungUngDung.Services.AddScoped<ThanhToanService>();
boDungUngDung.Services.AddScoped<VeService>();
boDungUngDung.Services.AddScoped<XuatVeService>();

// Quản trị + thống kê
boDungUngDung.Services.AddScoped<ThongKeService>();
boDungUngDung.Services.AddScoped<QuanTriService>();

// Hạ tầng
boDungUngDung.Services.AddSingleton<QrService>();
boDungUngDung.Services.AddSingleton<VePdfService>();
boDungUngDung.Services.AddScoped<EmailService>();


// ================================
// 6. PAYOS
// ================================

boDungUngDung.Services.AddHttpClient<PayOsService>(
    clientHttpThanhToan =>
    {
        clientHttpThanhToan.BaseAddress =
            new Uri("https://api-merchant.payos.vn/");
        clientHttpThanhToan.Timeout = TimeSpan.FromSeconds(15);
        clientHttpThanhToan.MaxResponseContentBufferSize = 128 * 1024;
    }
);


// ================================
// 7. BACKGROUND WORKER
// ================================

boDungUngDung.Services.AddHostedService<ExpiredOrderWorker>();
boDungUngDung.Services.AddHostedService<EventStateWorker>();


// ================================
// 8. JWT AUTHENTICATION
// ================================

var cauHinhTokenDangNhap =
    boDungUngDung.Configuration
        .GetSection("Jwt")
        .Get<JwtOptions>()
    ?? new JwtOptions();

if (Encoding.UTF8.GetByteCount(cauHinhTokenDangNhap.KhoaKyJwt) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key phải có ít nhất 32 byte."
    );
}

boDungUngDung.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(tuyChonJwt =>
    {
        tuyChonJwt.IncludeErrorDetails = false;
        tuyChonJwt.MapInboundClaims = true;
        tuyChonJwt.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = cauHinhTokenDangNhap.Issuer,
                ValidAudience = cauHinhTokenDangNhap.Audience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        cauHinhTokenDangNhap.KhoaKyJwt
                    )
                ),

                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ClockSkew = TimeSpan.Zero
            };
    });

boDungUngDung.Services.AddAuthorization();


// ================================
// 9. SWAGGER
// ================================

boDungUngDung.Services.AddEndpointsApiExplorer();

boDungUngDung.Services.AddSwaggerGen(tuyChonSwagger =>
{
    tuyChonSwagger.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "QLSK",
            Version = "v1"
        }
    );

    tuyChonSwagger.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập JWT theo dạng: Bearer {token}"
        }
    );

    tuyChonSwagger.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        }
    );
});


// ================================
// 10. CORS
// ================================

var allowedOrigins =
    boDungUngDung.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? [
        "http://localhost:5000",
        "http://127.0.0.1:5000"
    ];

boDungUngDung.Services.AddCors(tuyChonCors =>
{
    tuyChonCors.AddPolicy(
        "Frontend",
        chinhSachCors =>
        {
            chinhSachCors
                .WithOrigins(allowedOrigins)
                .WithHeaders("Content-Type", "Authorization", "Idempotency-Key")
                .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                .WithExposedHeaders("Retry-After", "X-Has-More");
        }
    );
});


// ================================
// 11. VALIDATION RESPONSE
// ================================

boDungUngDung.Services.Configure<ApiBehaviorOptions>(
    tuyChonApi =>
    {
        tuyChonApi.InvalidModelStateResponseFactory =
            nguCanhAction =>
            {
                var thongDiepLoi =
                    nguCanhAction.ModelState.Values
                        .SelectMany(
                            trangThaiModel => trangThaiModel.Errors
                        )
                        .Select(
                            loiModel => loiModel.ErrorMessage
                        )
                        .FirstOrDefault();

                var noiDungLoi =
                    string.IsNullOrWhiteSpace(thongDiepLoi)
                        ? "Dữ liệu không hợp lệ."
                        : thongDiepLoi;

                return new BadRequestObjectResult(
                    ApiResponse.TaoPhanHoiThatBai(
                        "VALIDATION_ERROR",
                        noiDungLoi
                    )
                );
            };
    }
);


// ================================
// 12. BUILD ỨNG DỤNG
// ================================

var ungDungWeb = boDungUngDung.Build();
if (knownProxies.Length > 0) ungDungWeb.UseForwardedHeaders();


// ================================
// 13. HTTP PIPELINE
// ================================

ungDungWeb.UseMiddleware<SecurityHeadersMiddleware>();
ungDungWeb.UseMiddleware<ApiExceptionMiddleware>();
ungDungWeb.UseRouting();
ungDungWeb.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), branch => branch.UseResponseCompression());

ungDungWeb.UseCors("Frontend");

ungDungWeb.UseDefaultFiles();
ungDungWeb.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var extension = Path.GetExtension(context.File.Name);
        context.Context.Response.Headers.CacheControl = extension.Equals(".html", StringComparison.OrdinalIgnoreCase)
            ? "no-cache" : context.Context.Request.Query.ContainsKey("v")
                ? "public,max-age=31536000,immutable" : "public,max-age=3600";
    }
});


// Swagger chỉ chạy ở Development
if (ungDungWeb.Environment.IsDevelopment())
{
    ungDungWeb.UseSwagger();
    ungDungWeb.UseSwaggerUI();
}


// ================================
// 14. XỬ LÝ HTTP STATUS CODE
// ================================

ungDungWeb.UseStatusCodePages(
    async nguCanhKetQuaTrangThai =>
    {
        var phanHoiYeuCauHttp =
            nguCanhKetQuaTrangThai.HttpContext.Response;

        if (
            phanHoiYeuCauHttp.HasStarted ||
            phanHoiYeuCauHttp.ContentLength.GetValueOrDefault() > 0
        )
        {
            return;
        }

        phanHoiYeuCauHttp.ContentType = "application/json";

        var maTrangThaiHttp =
            phanHoiYeuCauHttp.StatusCode;

        var maKetQua = maTrangThaiHttp switch
        {
            StatusCodes.Status400BadRequest =>
                "VALIDATION_ERROR",

            StatusCodes.Status401Unauthorized =>
                "UNAUTHORIZED",

            StatusCodes.Status403Forbidden =>
                "FORBIDDEN",

            StatusCodes.Status404NotFound =>
                "NOT_FOUND",
            StatusCodes.Status429TooManyRequests => "RATE_LIMIT_EXCEEDED",
            StatusCodes.Status503ServiceUnavailable => "SERVICE_UNAVAILABLE",

            _ =>
                "REQUEST_ERROR"
        };

        await phanHoiYeuCauHttp.WriteAsJsonAsync(
            ApiResponse.TaoPhanHoiThatBai(
                maKetQua,
                $"Yêu cầu thất bại với mã {maTrangThaiHttp}."
            )
        );
    }
);


// ================================
// 15. AUTHENTICATION + AUTHORIZATION
// ================================

ungDungWeb.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), branch =>
    branch.UseMiddleware<ApiCapacityMiddleware>());
ungDungWeb.UseRequestTimeouts();
ungDungWeb.UseAuthentication();
ungDungWeb.UseRateLimiter();
ungDungWeb.UseAuthorization();
ungDungWeb.UseOutputCache();


// ================================
// 16. MAP API
// ================================

ungDungWeb.MapControllers();


// ================================
// 17. LOG KHI ỨNG DỤNG KHỞI ĐỘNG
// ================================

ungDungWeb.Lifetime.ApplicationStarted.Register(() =>
{
    ungDungWeb.Logger.LogInformation(
        "Serving frontend from: {WebRootPath}",
        ungDungWeb.Environment.WebRootPath
    );

    foreach (var urlUngDung in ungDungWeb.Urls)
    {
        var url = urlUngDung.TrimEnd('/');

        ungDungWeb.Logger.LogInformation(
            "Frontend: {FrontendUrl}/ | Backend API: {ApiUrl}/api",
            url,
            url
        );
    }
});


// ================================
// 18. CHẠY ỨNG DỤNG
// ================================

ungDungWeb.Run();


// Dùng cho WebApplicationFactory trong project test
public partial class Program
{
}
