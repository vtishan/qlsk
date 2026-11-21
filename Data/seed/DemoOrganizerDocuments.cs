using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;
using QLSK.Features.Organizers;

namespace QLSK.Data.Seed;

internal static class DemoOrganizerDocuments
{
    // The caller owns the database transaction and removes the returned path on rollback.
    internal static async Task<string> Create(IServiceProvider services, AppDbContext db)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Demo organizer documents require the seed transaction.");

        var profiles = await db.HoSoBTC.FromSqlRaw(
            "SELECT h.* FROM dbo.HoSoBTC h INNER JOIN #SeedOrganizer s ON s.Id = h.Id").ToListAsync();
        if (profiles.Count == 0) throw new InvalidOperationException("No demo organizer profiles were seeded.");
        var resource = typeof(DemoOrganizerDocuments).Assembly;
        await using var source = resource.GetManifestResourceStream("QLSK.Data.Seed.organizer-demo.png")
            ?? throw new InvalidOperationException("Missing embedded demo organizer document.");
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer);

        var protection = services.GetRequiredService<IDataProtectionProvider>();
        var documentProtector = protection.CreateProtector("QLSK.Organizer.Document.v1");
        var holderProtector = protection.CreateProtector("QLSK.Organizer.Holder.v1");
        var folder = services.GetRequiredService<OrganizerService>().Storage();
        Directory.CreateDirectory(folder);
        var key = Guid.NewGuid().ToString("N");
        var path = Path.Combine(folder, key + ".enc");
        var ownedFile = false;
        try
        {
            await using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                ownedFile = true;
                await output.WriteAsync(documentProtector.Protect(buffer.ToArray()));
            }
            foreach (var profile in profiles)
            {
                profile.ThongTinNhanTienMaHoa = holderProtector.Protect(JsonSerializer.Serialize(new
                {
                    bankName = "Ngân hàng minh họa DEMO", bankAccount = "0000000000", bankOwner = profile.TenToChuc,
                    Branch = "Dữ liệu mẫu, không dùng để giao dịch",
                    legal = new { RegistrationType = "organization", Position = "Đại diện demo", HeadOffice = profile.DiaChi },
                    website = "https://qlsk.example", demo = true
                }));
                db.GiayToBTC.Add(new GiayToBTC
                {
                    HoSoBTCId = profile.Id, LoaiGiayTo = "Đăng ký doanh nghiệp", KhoaLuuTru = key,
                    TenTep = "DEMO-ho-so-ban-to-chuc.png", LoaiNoiDung = "image/png", NgayTaiLen = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();
            return path;
        }
        catch
        {
            if (ownedFile) File.Delete(path);
            throw;
        }
    }
}
