using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QLSK.Data;
using QLSK.Domain.Entities;

namespace QLSK.LocalData;

internal static class LocalDataProgram
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args.Contains("--help"))
            {
                Console.WriteLine("Local data maintenance; never run as part of backend startup.");
                Console.WriteLine("--normalize-existing [--dry-run] [--root PROJECT_DIRECTORY]");
                Console.WriteLine("--populate [--root PROJECT_DIRECTORY] (new account password: LOCALDATA_PASSWORD environment variable)");
                return args.Length == 0 ? 1 : 0;
            }
            var normalize = args.Contains("--normalize-existing");
            var populate = args.Contains("--populate");
            var dryRun = args.Contains("--dry-run");
            if (normalize == populate || (populate && dryRun))
                throw new ArgumentException("Choose exactly one operation; --dry-run is available for normalization only.");
            string? rootArgument = null;
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "--root")
                {
                    if (++i >= args.Length) throw new ArgumentException("--root requires the project directory.");
                    rootArgument = args[i];
                }
                else if (args[i] is not ("--normalize-existing" or "--populate" or "--dry-run"))
                    throw new ArgumentException("Unknown command argument. Use --help.");
            }
            var root = ResolveRoot(rootArgument);
            var config = new ConfigurationBuilder().SetBasePath(root)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: false)
                .AddJsonFile("appsettings.Local.json", optional: true)
                .AddEnvironmentVariables().Build();
            var connectionString = config.GetConnectionString("Default") ?? "";
            LocalSqlGuard.Validate(connectionString, requireTestDatabase: populate);
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options;
            await using var db = new ApplicationDbContext(options);
            if (normalize)
            {
                var result = await LocalEventSeeder.NormalizeExistingAsync(db, dryRun);
                Console.WriteLine(JsonSerializer.Serialize(new { operation = "normalize-existing", dryRun, result }));
            }
            else
            {
                var password = Environment.GetEnvironmentVariable("LOCALDATA_PASSWORD") ?? "";
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Set LOCALDATA_PASSWORD for newly created organizer accounts.");
                var added = await LocalEventSeeder.SeedAsync(db, new PasswordHasher<TaiKhoan>(), root, password);
                Console.WriteLine(JsonSerializer.Serialize(new { operation = "populate", addedEvents = added }));
            }
            return 0;
        }
        catch (ArgumentException error) { Console.Error.WriteLine(error.Message); return 2; }
        catch (InvalidOperationException error) when (!error.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
        { Console.Error.WriteLine(error.Message); return 2; }
        catch
        {
            Console.Error.WriteLine("Unable to complete the local database operation. Check the local configuration and migration state.");
            return 3;
        }
    }

    private static string ResolveRoot(string? value)
    {
        if (value is not null)
        {
            var root = Path.GetFullPath(value);
            if (!File.Exists(Path.Combine(root, "QLSK.csproj"))) throw new ArgumentException("--root must identify the QLSK project directory.");
            return root;
        }
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            for (var current = new DirectoryInfo(start); current is not null; current = current.Parent)
                if (File.Exists(Path.Combine(current.FullName, "QLSK.csproj"))) return current.FullName;
        throw new ArgumentException("Unable to find the project directory. Pass --root explicitly.");
    }
}
