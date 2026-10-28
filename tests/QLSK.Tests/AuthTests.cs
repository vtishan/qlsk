using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Features.TaiKhoan;
using Xunit;

namespace QLSK.Tests;

public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact(DisplayName = "POST /api/auth/register")]
    public async Task Post_api_auth_register()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/register",
            new DangKyDto
            {
                HoTen = "Test register",
                Email = $"register-{Guid.NewGuid():N}@test.local",
                MatKhau = "Abc@123456"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/auth/login")]
    public async Task Post_api_auth_login()
    {
        var email = $"login-{Guid.NewGuid():N}@test.local";
        await SeedLoginAccount(email, "Abc@123456");

        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login",
            new DangNhapDto
            {
                Email = email,
                MatKhau = "Abc@123456"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task SeedLoginAccount(string email, string password)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();
        var account = new TaiKhoan
        {
            HoTen = "Test login",
            Email = email,
            VaiTroId = 1,
            HoatDong = true
        };
        account.MatKhau = hasher.HashPassword(account, password);
        db.TaiKhoans.Add(account);
        await db.SaveChangesAsync();
    }
}
