using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;

namespace QLSK.LocalData;

public sealed record NormalizationResult(int Accounts, int Organizers, int Venues, int Events);

public static partial class LocalEventSeeder
{
    private const string LegacyDescriptionMarker = " Đây là sự kiện mẫu phục vụ trải nghiệm hệ thống QLSK.";
    private static readonly string[] Cities = ["TP. Hồ Chí Minh", "Hà Nội", "Đà Nẵng", "Huế", "Hải Phòng",
        "Cần Thơ", "Đà Lạt", "Nha Trang", "Quy Nhơn", "Vũng Tàu"];
    private static readonly string[] OrganizerNames = ["Kết Nối Văn Hóa", "Sáng Tạo Số", "Nhịp Sống Năng Động",
        "Hành Trình Tri Thức", "Cộng Đồng Xanh"];
    private static readonly string[] OrganizerDescriptions = [
        "Tổ chức các chương trình âm nhạc, nghệ thuật và giao lưu văn hóa.",
        "Kết nối cộng đồng công nghệ và phát triển các hoạt động sáng tạo.",
        "Tổ chức hoạt động thể thao, ẩm thực và trải nghiệm cuộc sống.",
        "Phát triển hội thảo, chương trình học tập và kết nối doanh nghiệp.",
        "Kết nối các hoạt động cộng đồng, du lịch có trách nhiệm và gia đình."
    ];
    private static readonly string[] VenueNames = ["Trung tâm sự kiện Bến Sông", "Hội trường Thăng Long",
        "Trung tâm hội nghị Sông Hàn", "Sảnh tiệc Hương Giang", "Trung tâm sự kiện Hải Âu",
        "Không gian sự kiện Cửu Long", "Khu sự kiện Đồi Thông", "Sảnh tiệc Biển Xanh",
        "Hội trường An Nhiên", "Khu sự kiện Gió Biển"];

    // This explicit local maintenance command updates recognized records in place.
    // Reading the plan is detached so --dry-run never changes tracked entities or the database.
    public static async Task<NormalizationResult> NormalizeExistingAsync(ApplicationDbContext db, bool dryRun = false)
    {
        var changes = new List<(object Entity, Dictionary<string, object?> Values)>();
        var accounts = await db.TaiKhoans.AsNoTracking().ToListAsync();
        var organizers = await db.BanToChucs.AsNoTracking().ToListAsync();
        var ownedOrganizerIds = new HashSet<int>();
        var accountChanges = 0;
        var organizerChanges = 0;
        for (var i = 1; i <= OrganizerNames.Length; i++)
        {
            var oldEmail = $"demo.btc{i:00}@qlsk.local";
            var newEmail = $"organizer{i:00}@qlsk.local";
            var user = accounts.SingleOrDefault(x => x.Email == oldEmail || x.Email == newEmail);
            if (user is null || user.VaiTroId != 2) continue;
            var values = new Dictionary<string, object?>();
            if (user.Email == oldEmail)
            {
                if (accounts.Any(x => x.Id != user.Id && x.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("The local organizer address is already assigned to another account.");
                values[nameof(TaiKhoan.Email)] = newEmail;
            }
            if (user.HoTen == $"Ban tổ chức mẫu {i:00}") values[nameof(TaiKhoan.HoTen)] = OrganizerNames[i - 1];
            if (values.Count > 0) { changes.Add((user, values)); accountChanges++; }

            var organizer = organizers.SingleOrDefault(x => x.TaiKhoanId == user.Id);
            if (organizer is null) continue;
            ownedOrganizerIds.Add(organizer.Id);
            values = new Dictionary<string, object?>();
            if (organizer.Ten == $"SựKiện Hub Demo {i:00}") values[nameof(BanToChuc.Ten)] = OrganizerNames[i - 1];
            if (organizer.MoTa == "Ban tổ chức mẫu phục vụ trải nghiệm hệ thống.")
                values[nameof(BanToChuc.MoTa)] = OrganizerDescriptions[i - 1];
            if (values.Count > 0) { changes.Add((organizer, values)); organizerChanges++; }
        }

        var venues = await db.DiaDiems.AsNoTracking().ToListAsync();
        var venueChanges = 0;
        for (var i = 0; i < Cities.Length; i++)
        {
            var oldName = $"Không gian sự kiện Demo {i + 1:00} - {Cities[i]}";
            var oldAddress = $"Khu trải nghiệm mẫu {i + 1:00}, {Cities[i]} (địa điểm giả lập)";
            var venue = venues.SingleOrDefault(x => x.Ten == oldName && x.DiaChi == oldAddress);
            if (venue is null) continue;
            var newAddress = $"Khu trung tâm, {Cities[i]}";
            if (venues.Any(x => x.Id != venue.Id && x.Ten == VenueNames[i] && x.DiaChi == newAddress))
                throw new InvalidOperationException("The local venue name and address are already assigned to another venue.");
            changes.Add((venue, new Dictionary<string, object?>
            {
                [nameof(DiaDiem.Ten)] = VenueNames[i], [nameof(DiaDiem.DiaChi)] = newAddress
            }));
            venueChanges++;
        }

        var eventChanges = 0;
        var images = Enumerable.Range(1, EventCount).Select(i => $"/assets/images/events/event-{i:000}.jpg").ToHashSet();
        var events = await db.SuKiens.AsNoTracking().Where(x => ownedOrganizerIds.Contains(x.BanToChucId)).ToListAsync();
        foreach (var item in events)
        {
            if (item.HinhAnh is null || !images.Contains(item.HinhAnh) || !item.MoTa.Contains(LegacyDescriptionMarker)) continue;
            changes.Add((item, new Dictionary<string, object?>
            {
                [nameof(SuKien.MoTa)] = item.MoTa.Replace(LegacyDescriptionMarker, "", StringComparison.Ordinal)
            }));
            eventChanges++;
        }

        if (!dryRun)
        {
            foreach (var (entity, values) in changes)
            {
                var id = (int)db.Entry(entity).Property("Id").CurrentValue!;
                var tracked = db.ChangeTracker.Entries().SingleOrDefault(x =>
                    x.Entity.GetType() == entity.GetType() && (int)x.Property("Id").CurrentValue! == id);
                var entry = tracked ?? db.Attach(entity);
                foreach (var (property, value) in values) entry.Property(property).CurrentValue = value;
            }
            if (changes.Count > 0) await db.SaveChangesAsync();
        }
        return new NormalizationResult(accountChanges, organizerChanges, venueChanges, eventChanges);
    }
}
