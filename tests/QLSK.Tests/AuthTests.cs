using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Features.TaiKhoan;
using Xunit;

namespace QLSK.Tests;

public class AuthTests(ApiFactory mayChuKiemThu) : IClassFixture<ApiFactory>
{
    [Fact(DisplayName = "POST /api/auth/register")]
    public async Task Post_api_auth_register()
    {
        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateClient().PostAsJsonAsync(
            "/api/auth/register",
            new DangKyDto
            {
                HoTen = "Test register",
                Email = $"register-{Guid.NewGuid():N}@test.local",
                MatKhau = "Abc@123456"
            });

        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
    }

    [Fact(DisplayName = "POST /api/auth/login")]
    public async Task Post_api_auth_login()
    {
        var emailTaiKhoan = $"login-{Guid.NewGuid():N}@test.local";
        await SeedLoginAccount(emailTaiKhoan, "Abc@123456");

        var phanHoiYeuCauHttp = await mayChuKiemThu.CreateClient().PostAsJsonAsync(
            "/api/auth/login",
            new DangNhapDto
            {
                Email = emailTaiKhoan,
                MatKhau = "Abc@123456"
            });

        Assert.Equal(HttpStatusCode.OK, phanHoiYeuCauHttp.StatusCode);
    }

    private async Task SeedLoginAccount(string emailTaiKhoan, string matKhauNhapLieu)
    {
        using var phamViDichVu = mayChuKiemThu.Services.CreateScope();
        var nguCanhDuLieu = phamViDichVu.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var boBamMatKhau = phamViDichVu.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();
        var taiKhoanCanXuLy = new TaiKhoan
        {
            HoTen = "Test login",
            Email = emailTaiKhoan,
            VaiTroId = 1,
            HoatDong = true
        };
        taiKhoanCanXuLy.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanCanXuLy, matKhauNhapLieu);
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoanCanXuLy);
        await nguCanhDuLieu.SaveChangesAsync();
    }
}
