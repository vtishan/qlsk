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
        await using var nguCanhDuLieu = await CreateDatabase();
        var boBamMatKhau = new PasswordHasher<TaiKhoan>();

        var ketQuaThucHien = await AdminProvisioner.CreateFirstAdminAsync(nguCanhDuLieu, boBamMatKhau, "  Owner@Customer.test  ", ValidPassword);

        Assert.Equal(AdminProvisioningResult.Created, ketQuaThucHien);
        var taiKhoanQuanTri = await nguCanhDuLieu.DanhSachTaiKhoan.SingleAsync();
        Assert.Equal("owner@customer.test", taiKhoanQuanTri.Email);
        Assert.Equal(4, taiKhoanQuanTri.VaiTroId);
        Assert.True(taiKhoanQuanTri.HoatDong);
        Assert.NotEqual(ValidPassword, taiKhoanQuanTri.MatKhauBam);
        Assert.NotEqual(PasswordVerificationResult.Failed, boBamMatKhau.VerifyHashedPassword(taiKhoanQuanTri, taiKhoanQuanTri.MatKhauBam, ValidPassword));
        Assert.Empty(await nguCanhDuLieu.DanhSachSuKien.ToListAsync());
        Assert.Empty(await nguCanhDuLieu.DanhSachDiaDiem.ToListAsync());
        Assert.Empty(await nguCanhDuLieu.DanhSachBanToChuc.ToListAsync());
    }

    [Fact]
    public async Task Existing_administrator_prevents_creation_unlocking_or_password_reset()
    {
        await using var nguCanhDuLieu = await CreateDatabase();
        var taiKhoanDaTonTai = new TaiKhoan
        {
            HoTen = "Người quản trị hiện tại", Email = "existing@customer.test", VaiTroId = 4,
            MatKhauBam = "preserve-existing-password-hash", HoatDong = false
        };
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoanDaTonTai);
        await nguCanhDuLieu.SaveChangesAsync();

        var ketQuaThucHien = await AdminProvisioner.CreateFirstAdminAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), "new@customer.test", ValidPassword);

        Assert.Equal(AdminProvisioningResult.AlreadyConfigured, ketQuaThucHien);
        var taiKhoanDaLuu = await nguCanhDuLieu.DanhSachTaiKhoan.SingleAsync();
        Assert.Equal(taiKhoanDaTonTai.TaiKhoanId, taiKhoanDaLuu.TaiKhoanId);
        Assert.Equal("existing@customer.test", taiKhoanDaLuu.Email);
        Assert.Equal("Người quản trị hiện tại", taiKhoanDaLuu.HoTen);
        Assert.Equal("preserve-existing-password-hash", taiKhoanDaLuu.MatKhauBam);
        Assert.False(taiKhoanDaLuu.HoatDong);
    }

    [Fact]
    public async Task Existing_user_email_cannot_be_promoted_by_the_setup_tool()
    {
        await using var nguCanhDuLieu = await CreateDatabase();
        var taiKhoanNguoiDung = new TaiKhoan
        {
            HoTen = "Thành viên", Email = "member@customer.test", VaiTroId = 1,
            MatKhauBam = "preserve-user-password-hash", HoatDong = true
        };
        nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoanNguoiDung);
        await nguCanhDuLieu.SaveChangesAsync();

        await Assert.ThrowsAsync<SetupValidationException>(() =>
            AdminProvisioner.CreateFirstAdminAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), taiKhoanNguoiDung.Email, ValidPassword));

        var taiKhoanDaLuu = await nguCanhDuLieu.DanhSachTaiKhoan.SingleAsync();
        Assert.Equal(1, taiKhoanDaLuu.VaiTroId);
        Assert.Equal("preserve-user-password-hash", taiKhoanDaLuu.MatKhauBam);
        Assert.False(await nguCanhDuLieu.DanhSachTaiKhoan.AnyAsync(taiKhoanCanXuLy => taiKhoanCanXuLy.VaiTroId == 4));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Short!1a")]
    [InlineData("onlylowercasecharacters")]
    [InlineData("ONLYUPPERCASECHARACTERS")]
    [InlineData("LongButOnlyLetters")]
    [InlineData("onlylowercase123456789")]
    public async Task Weak_credentials_do_not_create_accounts(string matKhauNhapLieu)
    {
        await using var nguCanhDuLieu = await CreateDatabase();

        await Assert.ThrowsAsync<SetupValidationException>(() =>
            AdminProvisioner.CreateFirstAdminAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), "owner@customer.test", matKhauNhapLieu));

        Assert.Empty(await nguCanhDuLieu.DanhSachTaiKhoan.ToListAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("missing-address")]
    [InlineData("Owner <owner@customer.test>")]
    public async Task Invalid_email_does_not_create_accounts(string emailTaiKhoan)
    {
        await using var nguCanhDuLieu = await CreateDatabase();

        await Assert.ThrowsAsync<SetupValidationException>(() =>
            AdminProvisioner.CreateFirstAdminAsync(nguCanhDuLieu, new PasswordHasher<TaiKhoan>(), emailTaiKhoan, ValidPassword));

        Assert.Empty(await nguCanhDuLieu.DanhSachTaiKhoan.ToListAsync());
    }

    private static async Task<ApplicationDbContext> CreateDatabase()
    {
        var tuyChonCoSoDuLieu = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"QLSK_Setup_Test_{Guid.NewGuid():N}")
            .Options;
        var nguCanhDuLieu = new ApplicationDbContext(tuyChonCoSoDuLieu);
        await nguCanhDuLieu.Database.EnsureCreatedAsync();
        return nguCanhDuLieu;
    }
}
