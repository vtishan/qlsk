using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Setup;
using Xunit;

namespace QLSK.Tests;

public class DeploymentSetupTests
{
    private const string ValidPassword = "CustomerSetup!2026";

    [Fact]
    public async Task First_administrator_is_hashed_and_no_business_data_is_created()
    {
        await using var db = await CreateDatabase();
        var hasher = new PasswordHasher<TaiKhoan>();

        var result = await AdminProvisioner.CreateFirstAsync(db, hasher, "  Owner@Customer.test  ", ValidPassword);

        Assert.Equal(AdminProvisioningResult.Created, result);
        var admin = await db.TaiKhoans.SingleAsync();
        Assert.Equal("owner@customer.test", admin.Email);
        Assert.Equal(4, admin.VaiTroId);
        Assert.True(admin.HoatDong);
        Assert.NotEqual(ValidPassword, admin.MatKhau);
        Assert.NotEqual(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(admin, admin.MatKhau, ValidPassword));
        Assert.Empty(await db.SuKiens.ToListAsync());
        Assert.Empty(await db.DiaDiems.ToListAsync());
        Assert.Empty(await db.BanToChucs.ToListAsync());
    }

    [Fact]
    public async Task Existing_administrator_prevents_creation_unlocking_or_password_reset()
    {
        await using var db = await CreateDatabase();
        var existing = new TaiKhoan
        {
            HoTen = "Người quản trị hiện tại", Email = "existing@customer.test", VaiTroId = 4,
            MatKhau = "preserve-existing-password-hash", HoatDong = false
        };
        db.TaiKhoans.Add(existing);
        await db.SaveChangesAsync();

        var result = await AdminProvisioner.CreateFirstAsync(db, new PasswordHasher<TaiKhoan>(), "new@customer.test", ValidPassword);

        Assert.Equal(AdminProvisioningResult.AlreadyConfigured, result);
        var saved = await db.TaiKhoans.SingleAsync();
        Assert.Equal(existing.Id, saved.Id);
        Assert.Equal("existing@customer.test", saved.Email);
        Assert.Equal("Người quản trị hiện tại", saved.HoTen);
        Assert.Equal("preserve-existing-password-hash", saved.MatKhau);
        Assert.False(saved.HoatDong);
    }

    [Fact]
    public async Task Existing_user_email_cannot_be_promoted_by_the_setup_tool()
    {
        await using var db = await CreateDatabase();
        var user = new TaiKhoan
        {
            HoTen = "Thành viên", Email = "member@customer.test", VaiTroId = 1,
            MatKhau = "preserve-user-password-hash", HoatDong = true
        };
        db.TaiKhoans.Add(user);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<SetupValidationException>(() =>
            AdminProvisioner.CreateFirstAsync(db, new PasswordHasher<TaiKhoan>(), user.Email, ValidPassword));

        var saved = await db.TaiKhoans.SingleAsync();
        Assert.Equal(1, saved.VaiTroId);
        Assert.Equal("preserve-user-password-hash", saved.MatKhau);
        Assert.False(await db.TaiKhoans.AnyAsync(account => account.VaiTroId == 4));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Short!1a")]
    [InlineData("onlylowercasecharacters")]
    [InlineData("ONLYUPPERCASECHARACTERS")]
    [InlineData("LongButOnlyLetters")]
    [InlineData("onlylowercase123456789")]
    public async Task Weak_credentials_do_not_create_accounts(string password)
    {
        await using var db = await CreateDatabase();

        await Assert.ThrowsAsync<SetupValidationException>(() =>
            AdminProvisioner.CreateFirstAsync(db, new PasswordHasher<TaiKhoan>(), "owner@customer.test", password));

        Assert.Empty(await db.TaiKhoans.ToListAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("missing-address")]
    [InlineData("Owner <owner@customer.test>")]
    public async Task Invalid_email_does_not_create_accounts(string email)
    {
        await using var db = await CreateDatabase();

        await Assert.ThrowsAsync<SetupValidationException>(() =>
            AdminProvisioner.CreateFirstAsync(db, new PasswordHasher<TaiKhoan>(), email, ValidPassword));

        Assert.Empty(await db.TaiKhoans.ToListAsync());
    }

    private static async Task<ApplicationDbContext> CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"QLSK_Setup_Test_{Guid.NewGuid():N}")
            .Options;
        var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }
}
