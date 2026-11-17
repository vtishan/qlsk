using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using QLSK.Data;
using Xunit;

namespace QLSK.Tests;

public class MigrationTests
{
    [Fact]
    public void Migrations_use_structured_operations_in_both_directions()
    {
        using var nguCanhDuLieu = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=localhost;Database=QLSK_Model_Inspection;Trusted_Connection=True;TrustServerCertificate=True")
                .Options);
        var tapHopMigration = nguCanhDuLieu.GetService<IMigrationsAssembly>();

        Assert.NotEmpty(tapHopMigration.Migrations);
        foreach (var kieuMigration in tapHopMigration.Migrations.Values)
        {
            var migration = tapHopMigration.CreateMigration(kieuMigration, nguCanhDuLieu.Database.ProviderName!);
            Assert.DoesNotContain(migration.UpOperations, thaoTac => thaoTac is SqlOperation);
            Assert.DoesNotContain(migration.DownOperations, thaoTac => thaoTac is SqlOperation);
        }
    }

    [Fact]
    public async Task Security_upgrade_preserves_existing_accounts_and_password_hashes()
    {
        using var factory = new ApiFactory();
        await factory.ThucThiVoiCoSoDuLieuKiemThu(async db =>
        {
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync("20261031170820_AddVenueImagePath");
            const string email = "preserved@test.local";
            const string hash = "existing-test-hash-must-stay-identical";
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [TaiKhoan] ([HoTen], [Email], [MatKhau], [VaiTroId], [HoatDong], [NgayTao]) VALUES ({"Preserved account"}, {email}, {hash}, {1}, {true}, {DateTime.UtcNow})");
            await migrator.MigrateAsync();
            var account = await db.DanhSachTaiKhoan.AsNoTracking().SingleAsync(a => a.Email == email);
            Assert.Equal(hash, account.MatKhauBam);
            Assert.Equal("Preserved account", account.HoTen);
            Assert.Equal(0, account.TokenVersion);
            Assert.Equal(1, account.VaiTroId);
        });
    }

    [Fact]
    public async Task All_migrations_can_be_reverted_and_reapplied_on_an_isolated_database()
    {
        using var ungDungKiemThu = new ApiFactory();
        await ungDungKiemThu.ThucThiVoiCoSoDuLieuKiemThu(async nguCanhDuLieu =>
        {
            var danhSachMigration = nguCanhDuLieu.Database.GetMigrations().ToArray();
            Assert.Equal(danhSachMigration, await nguCanhDuLieu.Database.GetAppliedMigrationsAsync());

            var boThucThiMigration = nguCanhDuLieu.GetService<IMigrator>();
            await boThucThiMigration.MigrateAsync(Migration.InitialDatabase);
            Assert.Empty(await nguCanhDuLieu.Database.GetAppliedMigrationsAsync());

            await boThucThiMigration.MigrateAsync();
            await nguCanhDuLieu.Database.MigrateAsync();
            Assert.Equal(danhSachMigration, await nguCanhDuLieu.Database.GetAppliedMigrationsAsync());

            var danhSachVaiTro = await nguCanhDuLieu.DanhSachVaiTro.AsNoTracking()
                .OrderBy(vaiTro => vaiTro.VaiTroId).ToListAsync();
            Assert.Equal(
                new[]
                {
                    (1, "NguoiDung", "Người dùng"),
                    (2, "BanToChuc", "Ban tổ chức"),
                    (3, "QuanLySuKien", "Quản lý sự kiện"),
                    (4, "QuanTriVien", "Quản trị viên")
                },
                danhSachVaiTro.Select(vaiTro => (vaiTro.VaiTroId, vaiTro.TenVaiTro, vaiTro.MoTaVaiTro)));

            Assert.Empty(await nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.YeuCauXacThucToChuc.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.LichSuYeuCauXacThucToChuc.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachBanToChuc.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachLoaiSuKien.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachSuKien.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachDiaDiem.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachDatDiaDiem.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachLoaiVe.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachDonVe.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.ChiTietDonVe.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachThanhToan.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.DanhSachVe.AsNoTracking().ToListAsync());
            Assert.Empty(await nguCanhDuLieu.LichSuXuatVe.AsNoTracking().ToListAsync());
        });
    }
}
