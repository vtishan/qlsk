using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.LocalData;

public static partial class LocalEventSeeder
{
    public const int EventCount = 100;

    public static async Task<int> SeedAsync(ApplicationDbContext db, IPasswordHasher<TaiKhoan> hasher,
        string contentRoot, string password, DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("A password is required to create local organizer accounts.");

        var path = Path.Combine(contentRoot, "tests", "QLSK.LocalData", "catalog", "events.json");
        await using var stream = File.OpenRead(path);
        var catalog = await JsonSerializer.DeserializeAsync<Catalog>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var samples = catalog?.Events.OrderBy(x => x.Number).ToArray() ?? [];
        if (samples.Length != EventCount ||
            !samples.Select(x => x.Number).SequenceEqual(Enumerable.Range(1, EventCount)) ||
            samples.Select(x => x.Title).Distinct().Count() != EventCount)
            throw new InvalidOperationException("The local catalog must contain 100 uniquely numbered and named events.");

        foreach (var sample in samples)
        {
            var expectedImage = $"/assets/images/events/event-{sample.Number:000}.jpg";
            if (string.IsNullOrWhiteSpace(sample.Title) || sample.Title.Length > 200 ||
                string.IsNullOrWhiteSpace(sample.Category) || sample.Category.Length > 120 ||
                sample.Description.Length > 4000 || sample.Image != expectedImage)
                throw new InvalidOperationException($"Invalid local event {sample.Number}.");
            if (!File.Exists(Path.Combine(contentRoot, "wwwroot", expectedImage.TrimStart('/'))))
                throw new FileNotFoundException($"Missing local image: {expectedImage}");
        }

        await NormalizeExistingAsync(db);
        var now = utcNow ?? DateTime.UtcNow;
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;

        var organizers = new List<BanToChuc>();
        for (var i = 1; i <= 5; i++)
        {
            var email = $"organizer{i:00}@qlsk.local";
            var user = await db.TaiKhoans.Include(x => x.BanToChuc).SingleOrDefaultAsync(x => x.Email == email);
            if (user is null)
            {
                user = new TaiKhoan
                {
                    Email = email, HoTen = OrganizerNames[i - 1], VaiTroId = 2,
                    HoatDong = true, NgayTao = now
                };
                user.MatKhau = hasher.HashPassword(user, password);
                db.TaiKhoans.Add(user);
            }
            else if (user.VaiTroId != 2)
            {
                throw new InvalidOperationException($"Local organizer email is already used by another role: {email}");
            }

            var organizer = user.BanToChuc;
            if (organizer is null)
            {
                organizer = new BanToChuc
                {
                    TaiKhoan = user, Ten = OrganizerNames[i - 1],
                    MoTa = OrganizerDescriptions[i - 1], NgayXacThuc = now
                };
                db.BanToChucs.Add(organizer);
            }
            organizers.Add(organizer);
        }

        var categories = await db.LoaiSuKiens.ToDictionaryAsync(x => x.Ten, StringComparer.OrdinalIgnoreCase);
        foreach (var name in samples.Select(x => x.Category).Distinct())
        {
            if (categories.ContainsKey(name)) continue;
            var category = new LoaiSuKien { Ten = name, MoTa = $"Các sự kiện về {name.ToLowerInvariant()}.", HoatDong = true };
            db.LoaiSuKiens.Add(category);
            categories.Add(name, category);
        }

        var venues = new List<DiaDiem>();
        for (var i = 0; i < Cities.Length; i++)
        {
            var name = VenueNames[i];
            var address = $"Khu trung tâm, {Cities[i]}";
            var venue = await db.DiaDiems.SingleOrDefaultAsync(x => x.Ten == name && x.DiaChi == address);
            if (venue is null)
            {
                venue = new DiaDiem
                {
                    Ten = name, DiaChi = address, SucChua = 600 + i * 100,
                    GiaThue = 2_000_000 + i * 500_000, HoatDong = true
                };
                db.DiaDiems.Add(venue);
            }
            venues.Add(venue);
        }

        var organizerIds = organizers.Where(x => x.Id != 0).Select(x => x.Id).ToArray();
        var existingEvents = await db.SuKiens.Where(x => organizerIds.Contains(x.BanToChucId)).ToListAsync();
        var imagePaths = samples.Select(x => x.Image).ToHashSet();
        var knownEvents = existingEvents.Where(x => x.HinhAnh != null && imagePaths.Contains(x.HinhAnh)).ToList();
        var firstDay = now.Date.AddDays(7).AddHours(11); // 18:00 in Vietnam; persisted dates are UTC.
        if (knownEvents.Count > 0)
        {
            var anchor = knownEvents.OrderBy(x => x.NgayTao).First();
            var anchorSample = samples.Single(x => x.Image == anchor.HinhAnh);
            firstDay = anchor.BatDau.AddDays(1 - anchorSample.Number);
        }

        var venueIds = venues.Where(x => x.Id != 0).Select(x => x.Id).ToArray();
        var bookings = await db.DatDiaDiems.Where(x => venueIds.Contains(x.DiaDiemId) &&
            (x.TrangThai == TrangThaiDatCho.ChoXacNhan || x.TrangThai == TrangThaiDatCho.DaXacNhan)).ToListAsync();
        var added = 0;
        foreach (var sample in samples)
        {
            // Keep existing event edits and associations intact on repeated explicit runs.
            if (existingEvents.Any(x => x.HinhAnh == sample.Image || x.Ten == sample.Title)) continue;
            var index = sample.Number - 1;
            var venue = venues[index % venues.Count];
            var start = firstDay.AddDays(index);
            if (start <= now.AddDays(1)) start = now.Date.AddDays(7 + index).AddHours(11);
            while (bookings.Any(x => (x.DiaDiem == venue || (venue.Id != 0 && x.DiaDiemId == venue.Id)) &&
                x.BatDau < start.AddHours(3) && x.KetThuc > start)) start = start.AddDays(1);
            var capacity = Math.Min(120 + index % 8 * 40, venue.SucChua);
            var regularCount = Math.Max(1, capacity * 4 / 5);
            var price = sample.Category == "Cộng đồng" ? 0 : 80_000 + index % 6 * 40_000;
            var entity = new SuKien
            {
                BanToChuc = organizers[index / 10 % organizers.Count], LoaiSuKien = categories[sample.Category],
                Ten = sample.Title, MoTa = sample.Description, HinhAnh = sample.Image,
                BatDau = start, KetThuc = start.AddHours(3), SoNguoi = capacity,
                TrangThai = TrangThaiSuKien.DaDuyet, NgayTao = now
            };
            entity.LoaiVes.Add(new LoaiVe
            {
                Ten = "Vé tiêu chuẩn", Gia = price, SoLuong = regularCount,
                MoBan = now.AddDays(-1), DongBan = start
            });
            if (capacity > regularCount)
                entity.LoaiVes.Add(new LoaiVe
                {
                    Ten = "Vé đồng hành", Gia = price == 0 ? 0 : price + 100_000,
                    SoLuong = capacity - regularCount, MoBan = now.AddDays(-1), DongBan = start
                });
            var booking = new DatDiaDiem
            {
                SuKien = entity, DiaDiem = venue, BatDau = start, KetThuc = entity.KetThuc,
                TongTien = venue.GiaThue, TienCoc = venue.GiaThue * 0.3m,
                TrangThai = TrangThaiDatCho.DaXacNhan
            };
            db.SuKiens.Add(entity);
            db.DatDiaDiems.Add(booking);
            bookings.Add(booking);
            added++;
        }

        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return added;
    }

    private sealed class Catalog
    {
        public List<SampleEvent> Events { get; set; } = [];
    }

    private sealed class SampleEvent
    {
        public int Number { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
    }
}
