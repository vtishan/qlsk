using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Catalog;

public sealed class CatalogService(AppDbContext db)
{
    internal static readonly string[] PublicOperations = [
        "catalog.banners", "catalog.categories", "catalog.eventFilterOptions", "catalog.events", "catalog.event", "catalog.sessions",
        "catalog.venues", "catalog.venue", "catalog.venueAvailability", "catalog.policies", "catalog.reviews"
    ];

    internal IQueryable<SuKien> Published() => db.SuKien.AsNoTracking().Where(e =>
        e.PhienBanCongKhaiId.HasValue &&
        db.PhienBanSuKien.Any(v => v.Id == e.PhienBanCongKhaiId && v.SuKienId == e.Id && v.TrangThaiDuyet == TrangThaiDuyetPhienBan.DaDuyet) &&
        (e.TrangThai == TrangThaiSuKien.DaDuyet || e.TrangThai == TrangThaiSuKien.DangMoBan ||
         e.TrangThai == TrangThaiSuKien.Hoan || e.TrangThai == TrangThaiSuKien.DaHuy || e.TrangThai == TrangThaiSuKien.HoanTat));

    internal static bool SaleOpen(SuatDien session, TrangThaiSuKien eventStatus, DateTime now) =>
        (eventStatus is TrangThaiSuKien.DangMoBan or TrangThaiSuKien.DaDuyet) &&
        (session.TrangThai is TrangThaiSuatDien.DangMoBan or TrangThaiSuatDien.DaXacNhan) &&
        session.ThoiGianMoBan <= now && now < session.ThoiGianDongBan;

    internal IQueryable<ChinhSach> PublishedPolicies(string? type = null)
    {
        var today = VietnamDate(DateTime.UtcNow);
        var policies = db.ChinhSach.AsNoTracking().Where(p => p.TrangThai == TrangThaiChinhSach.CoHieuLuc && p.NgayHieuLuc <= today);
        if (type == null) return policies;
        var title = type switch
        {
            "registration" => "Điều khoản sử dụng",
            "venue-rental" => "Chính sách thuê địa điểm",
            "ticketing" => "Chính sách đặt vé",
            _ => throw Invalid("Loại chính sách không hợp lệ.")
        };
        return policies.Where(p => p.TenChinhSach == title);
    }

    private IQueryable<SuatDien> VisibleSessions() => db.SuatDien.AsNoTracking().Where(s =>
        s.TrangThai != TrangThaiSuatDien.Nhap && db.DiaDiem.Any(v => v.Id == s.DiaDiemId && v.TrangThai != TrangThaiDiaDiem.DaAn));

    internal IQueryable<LichDatDiaDiem> Busy(DateTime now) => db.LichDatDiaDiem.AsNoTracking().Where(row =>
        row.TrangThai == TrangThaiLichDatDiaDiem.DaXacNhan ||
        (row.TrangThai == TrangThaiLichDatDiaDiem.DangGiu && (row.YeuCauThueId == null ||
            db.YeuCauThue.Any(r => r.Id == row.YeuCauThueId && r.TrangThai == TrangThaiYeuCauThue.ChoCoc && r.HanDatCoc > now))));

    public async Task<PageDto<BannerDto>> Banners(CancellationToken ct)
    {
        var rows = await db.Banner.AsNoTracking().Where(b => b.TrangThai == TrangThaiHienThi.Hien)
            .OrderBy(b => b.ThuTu).ThenBy(b => b.Id).Take(20).ToListAsync(ct);
        return new(rows.Select(b => new BannerDto(b.Id, b.TieuDe, null, SafeImage(b.HinhAnh))).ToArray());
    }

    public async Task<PageDto<CategoryDto>> Categories(CancellationToken ct) =>
        new(await db.DanhMucSuKien.AsNoTracking().Where(c => c.TrangThai == TrangThaiHienThi.Hien)
            .OrderBy(c => c.TenDanhMuc).Select(c => new CategoryDto(c.Id, c.TenDanhMuc)).ToListAsync(ct));

    public async Task<EventFilterOptionsDto> EventFilterOptions(CancellationToken ct)
    {
        var sessions = SessionRows(VisibleSessions());
        var prices = await db.LoaiVe.AsNoTracking()
            .Where(t => t.NhomVe != NhomVe.VeMoi && t.SoLuong > 0 && t.GiaVe >= 0 &&
                sessions.Any(s => s.Session.Id == t.SuatDienId))
            .Select(t => t.GiaVe).Distinct().OrderBy(price => price).ToListAsync(ct);
        var bands = prices.Chunk(Math.Max(1, (prices.Count + 3) / 4))
            .Select(chunk => new PriceBandDto(chunk[0], chunk[^1])).ToArray();
        var venues = await db.DiaDiem.AsNoTracking().Where(v => v.TrangThai == TrangThaiDiaDiem.HoatDong)
            .OrderBy(v => v.TenDiaDiem).ThenBy(v => v.Id)
            .Select(v => new VenueFilterOptionDto(v.Id, v.TenDiaDiem, v.DiaChi, v.GiaThue)).ToListAsync(ct);
        string[] commonCities = ["Hà Nội", "Hồ Chí Minh", "Đà Nẵng"];
        // Venue addresses store free-form text. Use explicit city and province names when present.
        var cities = commonCities.Concat(venues.SelectMany(v => v.Address.Split(','))
            .Select(part => part.Trim())
            .Where(part => part.StartsWith("Tỉnh ", StringComparison.OrdinalIgnoreCase) ||
                part.StartsWith("Thành phố ", StringComparison.OrdinalIgnoreCase))
            .Select(part => part[(part.StartsWith("Tỉnh ", StringComparison.OrdinalIgnoreCase)
                ? "Tỉnh ".Length : "Thành phố ".Length)..].Trim())
            .Where(name => name.Length is > 0 and <= 120)
            .Distinct(StringComparer.OrdinalIgnoreCase).Order())
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var categories = await Categories(ct);
        return new(bands, cities, venues, categories.Items.Select(c => c.Name).ToArray());
    }

    public async Task<PageDto<EventCardDto>> Events(EventFilter filter, TaiKhoan? actor, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var after = After(filter.Cursor);
        var size = Math.Clamp(filter.Limit ?? 8, 1, 40);
        if (filter.MinPrice < 0 || filter.MaxPrice < 0 || filter.MinPrice > filter.MaxPrice)
            throw Invalid("Khoảng giá không hợp lệ.");

        var published = Published().Where(e => e.Id > after);
        var sessions = VisibleSessions();
        var ticketTypes = db.LoaiVe.AsNoTracking().Where(t => t.NhomVe != NhomVe.VeMoi);
        var priceMatch = ticketTypes;
        if (filter.MinPrice.HasValue) priceMatch = priceMatch.Where(t => t.GiaVe >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue) priceMatch = priceMatch.Where(t => t.GiaVe <= filter.MaxPrice.Value);
        if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue)
            sessions = sessions.Where(s => priceMatch.Any(t => t.SuatDienId == s.Id));

        if (Optional(filter.Q, 120, "từ khóa") is { } q)
        {
            var matchingVenues = VisibleSessions().Where(s => db.DiaDiem.Any(v => v.Id == s.DiaDiemId &&
                (v.TenDiaDiem.Contains(q) || v.DiaChi.Contains(q))));
            published = published.Where(e => e.TenSuKien.Contains(q) || e.MoTa.Contains(q) ||
                matchingVenues.Any(s => s.SuKienId == e.Id));
        }
        if (Optional(filter.Category, 120, "danh mục") is { } category)
            published = published.Where(e => db.DanhMucSuKien.Any(c => c.Id == e.DanhMucId && c.TenDanhMuc == category && c.TrangThai == TrangThaiHienThi.Hien));
        if (Optional(filter.Organizer, 120, "Ban tổ chức") is { } organizer)
            published = published.Where(e => db.HoSoBTC.Any(o => o.Id == e.BTCId && o.TenToChuc.Contains(organizer)));
        if (filter.VenueId.HasValue) sessions = sessions.Where(s => s.DiaDiemId == filter.VenueId.Value);
        if (Optional(filter.Venue, 120, "địa điểm") is { } venue)
            sessions = sessions.Where(s => db.DiaDiem.Any(v => v.Id == s.DiaDiemId && v.TenDiaDiem.Contains(venue)));
        if (Optional(filter.City, 120, "thành phố") is { } city)
            sessions = sessions.Where(s => db.DiaDiem.Any(v => v.Id == s.DiaDiemId && v.DiaChi.Contains(city)));
        if (Optional(filter.Session, 120, "suất diễn") is { } name)
            sessions = sessions.Where(s => s.SoDoChot.Contains(name));
        if (filter.Date is { } date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue).AddHours(-7);
            var end = start.AddDays(1);
            sessions = sessions.Where(s => s.ThoiGianBatDau < end && s.ThoiGianKetThuc > start);
        }
        if (filter.Upcoming)
            sessions = sessions.Where(s => s.ThoiGianBatDau >= now &&
                (s.TrangThai == TrangThaiSuatDien.DaXacNhan || s.TrangThai == TrangThaiSuatDien.DangMoBan ||
                 s.TrangThai == TrangThaiSuatDien.DongBan));
        if (filter.Availability == "available")
        {
            var sold = db.Ve.Where(v => v.TrangThai == TrangThaiVe.DaPhatHanh || v.TrangThai == TrangThaiVe.DaCheckIn);
            var holds = db.GiuGhe.Where(h => h.TrangThai == TrangThaiGiuGhe.DangGiu && h.HetHan > now);
            published = published.Where(e => e.TrangThai == TrangThaiSuKien.DaDuyet || e.TrangThai == TrangThaiSuKien.DangMoBan);
            sessions = sessions.Where(s =>
                (s.TrangThai == TrangThaiSuatDien.DaXacNhan || s.TrangThai == TrangThaiSuatDien.DangMoBan) &&
                s.ThoiGianMoBan <= now && now < s.ThoiGianDongBan &&
                sold.Count(v => v.SuatDienId == s.Id) + holds.Count(h => h.SuatDienId == s.Id) < s.SucChua &&
                priceMatch.Any(t => t.SuatDienId == s.Id && t.BatDauBan <= now && now < t.KetThucBan &&
                    t.SoLuong > sold.Count(v => v.LoaiVeId == t.Id) +
                        holds.Count(h => db.ChiTietDonHang.Any(d => d.Id == h.ChiTietDonHangId && d.LoaiVeId == t.Id)) &&
                    db.Ghe.Any(g => g.KhuVucId == t.KhuVucId && g.TrangThai == TrangThaiGhe.SuDung &&
                        !sold.Any(v => v.SuatDienId == s.Id && v.GheId == g.Id) &&
                        !holds.Any(h => h.SuatDienId == s.Id && h.GheId == g.Id))));
        }
        if (filter.Featured)
        {
            var links = await db.Banner.AsNoTracking().Where(b => b.TrangThai == TrangThaiHienThi.Hien)
                .Select(b => b.DuongDan).ToListAsync(ct);
            var ids = links.Select(FeaturedId).Where(id => id > 0).Distinct().ToArray();
            published = published.Where(e => ids.Contains(e.Id));
        }
        var result = await (
            from e in published
            join categoryRow in db.DanhMucSuKien on e.DanhMucId equals categoryRow.Id
            where sessions.Any(s => s.SuKienId == e.Id)
            orderby e.Id
            select new
            {
                e.Id, e.TenSuKien, categoryRow.TenDanhMuc, e.AnhBia,
                Start = sessions.Where(s => s.SuKienId == e.Id).OrderBy(s => s.ThoiGianBatDau).ThenBy(s => s.Id)
                    .Select(s => (DateTime?)s.ThoiGianBatDau).FirstOrDefault(),
                Place = (from s in sessions join v in db.DiaDiem on s.DiaDiemId equals v.Id
                         where s.SuKienId == e.Id orderby s.ThoiGianBatDau, s.Id select v.TenDiaDiem).FirstOrDefault(),
                Price = priceMatch.Where(t => sessions.Any(s => s.SuKienId == e.Id && s.Id == t.SuatDienId))
                    .Min(t => (decimal?)t.GiaVe)
            }).Take(size + 1).ToListAsync(ct);
        var favorites = actor?.SuKienYeuThich?.ToHashSet();
        return Page(result.Select(r => new EventCardDto(r.Id, r.TenSuKien, r.TenDanhMuc, SafeImage(r.AnhBia), Stamp(r.Start), r.Place, r.Price,
            favorites?.Contains(r.Id) == true)).ToArray(), size, r => r.Id);
    }

    public async Task<EventDetailDto> Event(int id, TaiKhoan? actor, CancellationToken ct)
    {
        var item = await (
            from e in Published()
            join c in db.DanhMucSuKien on e.DanhMucId equals c.Id
            join o in db.HoSoBTC on e.BTCId equals o.Id
            join version in db.PhienBanSuKien on e.PhienBanCongKhaiId equals version.Id
            where e.Id == id
            select new { Event = e, c.TenDanhMuc, o.TenToChuc, version.NoiDung }).SingleOrDefaultAsync(ct) ?? throw Missing();
        var sessions = await SessionRows(VisibleSessions().Where(s => s.SuKienId == id))
            .OrderBy(row => row.Session.ThoiGianBatDau).ThenBy(row => row.Session.Id).ToListAsync(ct);
        var sessionIds = sessions.Select(row => row.Session.Id).ToArray();
        var prices = (await db.LoaiVe.AsNoTracking().Where(t => sessionIds.Contains(t.SuatDienId) && t.NhomVe != NhomVe.VeMoi)
            .OrderBy(t => t.GiaVe).ThenBy(t => t.Id).ToListAsync(ct)).ToLookup(t => t.SuatDienId);
        var now = DateTime.UtcNow;
        var content = JsonText(item.NoiDung);
        var canReview = actor is not null && await CanReview(actor.Id, id, ct);
        var favorite = actor?.SuKienYeuThich?.Contains(id) == true;
        return new(id, item.Event.TenSuKien, SafeImage(item.Event.AnhBia), item.TenDanhMuc, item.TenToChuc,
            item.Event.MoTa, content.GetValueOrDefault("participants"), content.GetValueOrDefault("program"),
            content.GetValueOrDefault("rules"), content.GetValueOrDefault("policy"),
            item.Event.TrangThai.ToString(), sessions.Select(row => ToSession(row, now, prices[row.Session.Id]
                .Select(t => new PriceDto(t.TenLoaiVe, t.GiaVe, Stamp(t.BatDauBan), Stamp(t.KetThucBan), t.QuyenLoi)).ToArray())).ToArray(),
            canReview ? ["review"] : [], favorite);
    }

    public async Task<PageDto<SessionDto>> Sessions(DateTimeOffset from, DateTimeOffset to, string? city, string? category,
        string? cursor, CancellationToken ct)
    {
        CheckRange(from, to);
        const int size = 1000;
        var after = After(cursor);
        var start = from.UtcDateTime;
        var end = to.UtcDateTime;
        var rows = SessionRows(VisibleSessions().Where(s => s.Id > after && s.ThoiGianBatDau < end && s.ThoiGianKetThuc > start));
        if (Optional(city, 120, "thành phố") is { } cityName) rows = rows.Where(s => s.Place.DiaChi.Contains(cityName));
        if (Optional(category, 120, "danh mục") is { } categoryName)
            rows = rows.Where(s => db.DanhMucSuKien.Any(c => c.Id == s.Show.DanhMucId && c.TenDanhMuc == categoryName));
        var page = await rows.OrderBy(s => s.Session.Id).Take(size + 1).ToListAsync(ct);
        var now = DateTime.UtcNow;
        return Page(page.Select(row => ToSession(row, now)).ToArray(), size, row => row.Id);
    }

    private sealed class SessionRow
    {
        public required SuatDien Session { get; init; }
        public required DiaDiem Place { get; init; }
        public required SuKien Show { get; init; }
    }
    private IQueryable<SessionRow> SessionRows(IQueryable<SuatDien> s) =>
        from session in s join place in db.DiaDiem on session.DiaDiemId equals place.Id
        join evt in Published() on session.SuKienId equals evt.Id
        select new SessionRow { Session = session, Place = place, Show = evt };

    private static SessionDto ToSession(SessionRow row, DateTime now, IReadOnlyList<PriceDto>? prices = null)
    {
        var session = row.Session;
        var name = JsonText(session.SoDoChot).GetValueOrDefault("name") ?? "Suất diễn " + session.Id;
        return new(session.Id, name, Stamp(session.ThoiGianBatDau), Stamp(session.ThoiGianKetThuc),
            new(row.Place.Id, row.Place.TenDiaDiem, row.Place.DiaChi), new(row.Show.Id, row.Show.TenSuKien),
            SaleOpen(session, row.Show.TrangThai, now), prices);
    }

    public async Task<PageDto<VenueDto>> Venues(VenueFilter filter, CancellationToken ct)
    {
        const int size = 12;
        var after = After(filter.Cursor);
        var query = db.DiaDiem.AsNoTracking().Where(v => v.Id > after && v.TrangThai == TrangThaiDiaDiem.HoatDong);
        if (Optional(filter.Q, 120, "từ khóa") is { } q) query = query.Where(v => v.TenDiaDiem.Contains(q) || v.DiaChi.Contains(q));
        if (Optional(filter.City, 120, "thành phố") is { } city) query = query.Where(v => v.DiaChi.Contains(city));
        if (filter.MinCapacity.HasValue) query = query.Where(v => v.SucChua >= filter.MinCapacity.Value);
        if (filter.MaxPrice.HasValue) query = query.Where(v => v.GiaThue <= filter.MaxPrice.Value);
        if (Optional(filter.Amenities, 120, "tiện ích") is { } facilities)
            query = query.Where(v => v.TienIch != null && v.TienIch.Contains(facilities));
        if (filter.Date is { } date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue).AddHours(-7);
            var end = start.AddDays(1);
            var busy = Busy(DateTime.UtcNow);
            query = query.Where(v => !busy.Any(l => l.DiaDiemId == v.Id && l.BatDau < end && l.KetThuc > start));
        }
        var rows = await query.OrderBy(v => v.Id).Take(size + 1)
            .Select(v => new VenueDto(v.Id, v.TenDiaDiem, v.DiaChi, v.SucChua, v.GiaThue, v.TienIch, null, null)).ToListAsync(ct);
        return Page(rows, size, v => v.Id);
    }

    public async Task<VenueDto> Venue(int id, CancellationToken ct)
    {
        var v = await db.DiaDiem.AsNoTracking().SingleOrDefaultAsync(v => v.Id == id && v.TrangThai == TrangThaiDiaDiem.HoatDong, ct) ?? throw Missing();
        var zones = await db.KhuVuc.AsNoTracking().Where(z => z.DiaDiemId == id).OrderBy(z => z.Id)
            .Select(z => new VenueZoneDto(z.TenKhuVuc, z.SucChua, db.Ghe.Any(g => g.KhuVucId == z.Id) ? "Ghế đánh số" : "Khu vực tự do")).ToListAsync(ct);
        return new(v.Id, v.TenDiaDiem, v.DiaChi, v.SucChua, v.GiaThue, v.TienIch, zones);
    }

    public async Task<PageDto<AvailabilityDto>> Availability(int id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        CheckRange(from, to);
        if (!await db.DiaDiem.AnyAsync(v => v.Id == id && v.TrangThai == TrangThaiDiaDiem.HoatDong, ct)) throw Missing();
        var start = from.UtcDateTime;
        var end = to.UtcDateTime;
        var intervals = await Busy(DateTime.UtcNow).Where(a => a.DiaDiemId == id && a.BatDau < end && a.KetThuc > start)
            .OrderBy(a => a.BatDau).Select(a => new { a.BatDau, a.KetThuc }).ToListAsync(ct);
        return new(intervals.Select(a => new AvailabilityDto(Stamp(a.BatDau), Stamp(a.KetThuc), "Bận")).ToArray());
    }

    public async Task<PageDto<PolicyDto>> Policies(string? type, CancellationToken ct) =>
        new(await PublishedPolicies(type).OrderByDescending(p => p.NgayHieuLuc).ThenByDescending(p => p.PhienBan)
            .Take(60).Select(p => new PolicyDto(p.Id, p.TenChinhSach, p.NoiDung)).ToListAsync(ct));

    public async Task<PageDto<ReviewDto>> Reviews(int id, string? cursor, CancellationToken ct)
    {
        if (!await Published().AnyAsync(e => e.Id == id, ct)) throw Missing();
        const int size = 30;
        var after = After(cursor);
        var rows = await db.DanhGia.AsNoTracking().Where(r => r.SuKienId == id && r.Id > after && r.TrangThai == TrangThaiDanhGia.CongKhai)
            .OrderBy(r => r.Id).Take(size + 1).Select(r => new ReviewDto(r.Id, r.SoSao, r.BinhLuan)).ToListAsync(ct);
        return Page(rows, size, r => r.Id);
    }

    public async Task<int> Review(TaiKhoan actor, int id, ReviewRequest request, CancellationToken ct)
    {
        if (!await Published().AnyAsync(e => e.Id == id, ct)) throw Missing();
        if (request.Rating is < 1 or > 5) throw Invalid("Điểm đánh giá phải từ 1 đến 5.");
        if (!await CanReview(actor.Id, id, ct)) throw Denied();
        var review = new DanhGia {
            TaiKhoanId = actor.Id, SuKienId = id, SoSao = request.Rating, BinhLuan = Optional(request.Comment, 800, "nhận xét"),
            NgayDanhGia = DateTime.UtcNow, TrangThai = TrangThaiDanhGia.ChoDuyet
        };
        db.DanhGia.Add(review);
        await db.SaveChangesAsync(ct);
        return review.Id;
    }

    private async Task<bool> CanReview(int account, int eventId, CancellationToken ct)
    {
        if (await db.DanhGia.AnyAsync(r => r.TaiKhoanId == account && r.SuKienId == eventId, ct)) return false;
        return await (from t in db.Ve join s in db.SuatDien on t.SuatDienId equals s.Id
                      join line in db.ChiTietDonHang on t.ChiTietDonHangId equals line.Id
                      join order in db.DonHang on line.DonHangId equals order.Id
                      where t.TrangThai == TrangThaiVe.DaCheckIn && s.SuKienId == eventId && order.TaiKhoanId == account
                      select t.Id).AnyAsync(ct);
    }

    private static void CheckRange(DateTimeOffset from, DateTimeOffset to)
    {
        if (from.Year < 1970 || to.Year > 9998 || to <= from || to - from > TimeSpan.FromDays(62))
            throw Invalid("Khoảng thời gian phải hợp lệ và dài tối đa 62 ngày.");
    }

    private static int FeaturedId(string? link)
    {
        if (link?.StartsWith("/#event/", StringComparison.Ordinal) == true) link = link[1..];
        return link?.StartsWith("#event/", StringComparison.Ordinal) == true &&
            int.TryParse(link[7..].Split('/', '?')[0], out var id) ? id : 0;
    }

    internal static string? SafeImage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith("//") || value.Contains('\\')) return null;
        if (value.StartsWith('/')) return Uri.TryCreate(value, UriKind.Relative, out _) ? value : null;
        if (Uri.TryCreate(value, UriKind.Absolute, out var url)) return url.Scheme == Uri.UriSchemeHttps ? value : null;
        return Uri.TryCreate(value, UriKind.Relative, out _) ? value : null;
    }

    internal static Dictionary<string, string> JsonText(string json)
    {
        using var content = JsonDocument.Parse(json);
        return content.RootElement.EnumerateObject().Where(property => property.Value.ValueKind == JsonValueKind.String)
            .ToDictionary(property => property.Name, property => property.Value.GetString()!, StringComparer.OrdinalIgnoreCase);
    }
}
