using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using QLSK.Data;
using QLSK.Data.Seed;
using QLSK.Domain.Enums;
using QLSK.Features.Accounts;

namespace QLSK.Common.Data;

internal static class DatabaseSetup
{
    public static async Task Run(IServiceProvider services, IHostEnvironment environment, bool seedDemo)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var bootstrap = config.GetSection("QLSK:BootstrapAdmin");
        if (!environment.IsDevelopment() && (seedDemo || !string.IsNullOrWhiteSpace(bootstrap["Email"])))
            throw new InvalidOperationException("Khởi tạo tài khoản qua cấu hình và nạp dữ liệu thử chỉ áp dụng trong Development.");
        var db = services.GetRequiredService<AppDbContext>();
        var connection = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        if (string.IsNullOrWhiteSpace(connection.InitialCatalog) ||
            new[] { "master", "tempdb", "model", "msdb" }.Contains(connection.InitialCatalog, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Chỉ khởi tạo cơ sở dữ liệu QLSK được chỉ định tên riêng.");

        var creator = db.GetService<IRelationalDatabaseCreator>();
        if (!await creator.ExistsAsync())
        {
            try { await creator.CreateAsync(); }
            catch (SqlException ex) when (ex.Number == 1801) { /* Another initializer created the database. */ }
        }

        await using (var upgrade = await db.Database.BeginTransactionAsync())
        {
            await SqlOperations.Lock(db, "qlsk:schema-upgrade", CancellationToken.None);
            var expected = db.Model.GetEntityTypes().Select(t => "dbo." + t.GetTableName()).ToHashSet();
            var actual = await TableNames(db);
            if (actual.Count == 0)
            {
                await Script(db, "Migrations.000-initial-schema.sql");
                actual = await TableNames(db);
            }
            if (!expected.SetEquals(actual))
                throw new InvalidOperationException($"Cần lược đồ QLSK gồm {expected.Count} bảng nghiệp vụ để nâng cấp.");

            const string schema = "QLSK.SchemaVersion";
            var version = await Version(db, schema);
            if (version > 2) throw new InvalidOperationException("Lược đồ SQL mới hơn phiên bản ứng dụng.");
            if (version < 1) await Script(db, "Migrations.001-qlsk-features.sql");
            if (version < 2) await Script(db, "Migrations.002-initial-categories.sql");
            if (version < 2) await Stamp(db, schema, 2);

            if (!string.IsNullOrWhiteSpace(bootstrap["Email"]) &&
                !await db.TaiKhoan.AnyAsync(t => t.VaiTro == VaiTroTaiKhoan.QuanTriVien))
            {
                var admin = AccountService.NewAccount(bootstrap["Name"] ?? "Quản trị QLSK", bootstrap["UserName"] ?? "qlsk-admin",
                    bootstrap["Email"], null, bootstrap["Password"]);
                admin.VaiTro = VaiTroTaiKhoan.QuanTriVien;
                db.TaiKhoan.Add(admin);
                await db.SaveChangesAsync();
            }
            await upgrade.CommitAsync();
            Console.WriteLine($"Đã áp dụng migration SQL cho {connection.InitialCatalog} ({actual.Count} bảng).");
        }
        if (seedDemo) await DemoSeed.Run(services, db, config);
    }

    private static Task<List<string>> TableNames(AppDbContext db) => db.Database.SqlQueryRaw<string>(
        "SELECT SCHEMA_NAME(schema_id) + '.' + name AS Value FROM sys.tables WHERE is_ms_shipped = 0").ToListAsync();

    // Keep migration history outside the 37 business tables, including on databases created by EnsureCreated.
    internal static Task<int> Version(AppDbContext db, string key) => db.Database.SqlQuery<int>(
        $"SELECT CONVERT(int, value) AS Value FROM sys.extended_properties WHERE class = 0 AND name = {key}").SingleOrDefaultAsync();

    internal static Task Stamp(AppDbContext db, string key, int version) => db.Database.ExecuteSqlInterpolatedAsync($"""
        IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0 AND name = {key})
            EXEC sys.sp_updateextendedproperty @name={key}, @value={version};
        ELSE
            EXEC sys.sp_addextendedproperty @name={key}, @value={version};
        """);

    internal static async Task Script(AppDbContext db, string name)
    {
        using var stream = typeof(DatabaseSetup).Assembly.GetManifestResourceStream("QLSK.Data." + name)
            ?? throw new InvalidOperationException($"Thiếu tập lệnh {name}.");
        using var reader = new StreamReader(stream);
        // Execute each SQL Server batch on the caller's transaction and connection.
        foreach (var batch in Regex.Split(await reader.ReadToEndAsync(), @"(?im)^GO[ \t]*\r?$"))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
            command.CommandText = batch;
            await command.ExecuteNonQueryAsync();
        }
    }
}
