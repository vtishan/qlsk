using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Catalog;

public sealed class CatalogAdminService(AppDbContext db, CatalogService catalog)
{
    internal static readonly string[] Operations = [
        "work.venues.list", "work.venues.detail", "work.venues.create", "work.venues.edit", "work.venues.visibility",
        "work.catalog.list", "work.catalog.detail", "work.catalog.create", "work.catalog.edit", "work.catalog.visibility",
        "work.policies.list", "work.policies.detail", "work.policies.create", "work.policies.publish",
        "work.zones.list", "work.zones.detail", "work.zones.create", "work.zones.update",
        "work.venueBlocks.list", "work.venueBlocks.detail", "work.venueBlocks.create", "work.venueBlocks.release"
    ];
    private const int Size = 30;

    private static void Check(TaiKhoan actor)
    {
        if (!AccountAccess.Admin(actor)) throw Denied();
    }

    public Task<PageDto<CatalogAdminDto>> List(TaiKhoan actor, string resource, string? status, string? cursor, CancellationToken ct) =>
        Read(actor, resource, status, After(cursor), null, ct);

    public async Task<CatalogAdminDto> Detail(TaiKhoan actor, string resource, int id, CancellationToken ct) =>
        (await Read(actor, resource, null, 0, id, ct)).Items.SingleOrDefault() ?? throw Missing();

    private async Task<PageDto<CatalogAdminDto>> Read(TaiKhoan actor, string resource, string? status, int after, int? id, CancellationToken ct)
    {
        Check(actor);
        IEnumerable<CatalogAdminDto> rows;
        switch (resource)
        {
            case "venues":
                var locations = db.DiaDiem.AsNoTracking().Where(r => r.Id > after && (!id.HasValue || r.Id == id));
                if (status == "ACTIVE") locations = locations.Where(r => r.TrangThai == TrangThaiDiaDiem.HoatDong);
                if (status == "HIDDEN") locations = locations.Where(r => r.TrangThai == TrangThaiDiaDiem.DaAn);
                rows = (await locations.OrderBy(r => r.Id).Take(Size + 1).ToListAsync(ct)).Select(r =>
                    new CatalogAdminDto(r.Id, r.TenDiaDiem, Address: r.DiaChi, Capacity: r.SucChua, Rate: r.GiaThue, Amenities: r.TienIch,
                        Status: VenueStatus(r.TrangThai), StatusLabel: r.TrangThai == TrangThaiDiaDiem.HoatDong ? "Đang mở" : "Tạm dừng hoặc ẩn",
                        AllowedActions: ["edit", "visibility"]));
                break;
            case "event-categories":
                var categories = db.DanhMucSuKien.AsNoTracking().Where(r => r.Id > after && (!id.HasValue || r.Id == id));
                if (status != null) categories = categories.Where(r => r.TrangThai == (status == "ACTIVE" ? TrangThaiHienThi.Hien : TrangThaiHienThi.An));
                rows = (await categories.OrderBy(r => r.Id).Take(Size + 1).ToListAsync(ct)).Select(r =>
                    new CatalogAdminDto(r.Id, r.TenDanhMuc, Description: r.MoTa, Status: Display(r.TrangThai), AllowedActions: ["edit", "visibility"]));
                break;
            case "banners":
                var banners = db.Banner.AsNoTracking().Where(r => r.Id > after && (!id.HasValue || r.Id == id));
                if (status != null) banners = banners.Where(r => r.TrangThai == (status == "ACTIVE" ? TrangThaiHienThi.Hien : TrangThaiHienThi.An));
                rows = (await banners.OrderBy(r => r.Id).Take(Size + 1).ToListAsync(ct)).Select(r =>
                    new CatalogAdminDto(r.Id, r.TieuDe, Position: r.ThuTu, Description: r.HinhAnh, Status: Display(r.TrangThai),
                        AllowedActions: ["edit", "visibility"], Fields: [new("Hình ảnh", r.HinhAnh), new("Liên kết", r.DuongDan)]));
                break;
            case "policies":
                var policies = db.ChinhSach.AsNoTracking().Where(r => r.Id > after && (!id.HasValue || r.Id == id));
                if (status != null)
                {
                    var phase = Enum.GetValues<TrangThaiChinhSach>().SingleOrDefault(x => PolicyStatus(x) == status);
                    policies = policies.Where(r => r.TrangThai == phase);
                }
                rows = (await policies.OrderBy(r => r.Id).Take(Size + 1).ToListAsync(ct)).Select(r =>
                    new CatalogAdminDto(r.Id, Title: r.TenChinhSach, Description: r.NoiDung, VersionLabel: r.PhienBan.ToString(),
                        EffectiveFrom: new DateTimeOffset(r.NgayHieuLuc.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(7)),
                        Status: PolicyStatus(r.TrangThai), AllowedActions: r.TrangThai == TrangThaiChinhSach.Nhap ? ["publish"] : []));
                break;
            case "zones":
                var zones = await (from r in db.KhuVuc.AsNoTracking() join v in db.DiaDiem on r.DiaDiemId equals v.Id
                                   where r.Id > after && (!id.HasValue || r.Id == id) orderby r.Id
                                   select new { Zone = r, v.TenDiaDiem, Seats = db.Ghe.Count(g => g.KhuVucId == r.Id) }).Take(Size + 1).ToListAsync(ct);
                rows = zones.Select(r => new CatalogAdminDto(r.Zone.Id, r.Zone.TenKhuVuc, Capacity: r.Zone.SucChua, VenueName: r.TenDiaDiem,
                    Seating: r.Seats > 0 ? "Ghế đánh số" : "Khu vực tự do", AllowedActions: ["update"]));
                break;
            case "venue-blocks":
                var blocks = await (from r in db.LichDatDiaDiem.AsNoTracking() join v in db.DiaDiem on r.DiaDiemId equals v.Id
                                    where r.Id > after && (!id.HasValue || r.Id == id) && r.LoaiLich == LoaiLichDiaDiem.BaoTri orderby r.Id
                                    select new { Block = r, v.TenDiaDiem }).Take(Size + 1).ToListAsync(ct);
                rows = blocks.Select(r => new CatalogAdminDto(r.Block.Id, Label: "Bảo trì", VenueName: r.TenDiaDiem,
                    From: Stamp(r.Block.BatDau), To: Stamp(r.Block.KetThuc),
                    Status: r.Block.TrangThai == TrangThaiLichDatDiaDiem.DaXacNhan ? "ACTIVE" : "CANCELLED",
                    AllowedActions: r.Block.TrangThai == TrangThaiLichDatDiaDiem.DaXacNhan ? ["release"] : []));
                break;
            default: throw Missing();
        }
        return Page(rows.ToArray(), Size, r => r.Id);
    }

    public async Task<int> Create(TaiKhoan actor, string resource, CatalogChangeRequest request, CancellationToken ct)
    {
        Check(actor);
        object row;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        switch (resource)
        {
            case "venues":
                var venue = new DiaDiem { TrangThai = TrangThaiDiaDiem.HoatDong };
                SetVenue(venue, request);
                row = venue;
                break;
            case "event-categories":
                row = new DanhMucSuKien { TenDanhMuc = Required(request.Name, 120, "tên danh mục"),
                    MoTa = Optional(request.Description, 1000, "mô tả"), TrangThai = TrangThaiHienThi.Hien };
                break;
            case "banners":
                var banner = new Banner { TrangThai = TrangThaiHienThi.An };
                SetBanner(banner, request);
                row = banner;
                break;
            case "policies":
                var title = Required(request.Title, 120, "tên chính sách");
                var date = request.EffectiveFrom?.ToOffset(TimeSpan.FromHours(7)).DateTime ?? throw Invalid("Chọn ngày hiệu lực.");
                await SqlOperations.Lock(db, "qlsk:policy-versions", ct);
                var version = await db.ChinhSach.Where(p => p.TenChinhSach == title).MaxAsync(p => (int?)p.PhienBan, ct) ?? 0;
                row = new ChinhSach { TenChinhSach = title, NoiDung = Required(request.Content, 12000, "nội dung chính sách"),
                    NgayHieuLuc = DateOnly.FromDateTime(date), PhienBan = version + 1, TrangThai = TrangThaiChinhSach.Nhap };
                break;
            case "zones":
                return await CreateZone(actor, request, tx, ct);
            case "venue-blocks":
                var venueId = request.VenueId ?? throw Invalid("Chọn địa điểm.");
                var start = request.From?.UtcDateTime ?? throw Invalid("Chọn thời điểm bắt đầu.");
                var end = request.To?.UtcDateTime ?? throw Invalid("Chọn thời điểm kết thúc.");
                if (start >= end || end <= DateTime.UtcNow) throw Invalid("Khoảng bảo trì không hợp lệ.");
                await SqlOperations.Lock(db, $"qlsk:venue:{venueId}", ct);
                if (!await db.DiaDiem.AnyAsync(v => v.Id == venueId, ct)) throw Missing();
                if (await catalog.Busy(DateTime.UtcNow).AnyAsync(r => r.DiaDiemId == venueId && r.BatDau < end && r.KetThuc > start, ct))
                    throw Conflict("Khung bảo trì trùng lịch đã giữ hoặc xác nhận.");
                row = new LichDatDiaDiem { DiaDiemId = venueId, BatDau = start, KetThuc = end,
                    LoaiLich = LoaiLichDiaDiem.BaoTri, TrangThai = TrangThaiLichDatDiaDiem.DaXacNhan };
                break;
            default: throw Missing();
        }
        db.Add(row);
        await db.SaveChangesAsync(ct);
        var id = (int)db.Entry(row).Property("Id").CurrentValue!;
        SqlOperations.Audit(db, actor, "Tạo danh mục", resource, id, Optional(request.Reason, 1000, "ghi chú"));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return id;
    }

    public async Task<CatalogAdminDto> Update(TaiKhoan actor, string resource, int id, CatalogChangeRequest request, CancellationToken ct)
    {
        Check(actor);
        var action = request.Action;
        var reason = Required(request.Reason, 1000, "lý do");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:catalog:{resource}:{id}", ct);
        switch (resource)
        {
            case "venues":
                await SqlOperations.Lock(db, $"qlsk:venue:{id}", ct);
                var v = await db.DiaDiem.SingleOrDefaultAsync(r => r.Id == id, ct) ?? throw Missing();
                Match(request.ExpectedState, VenueStatus(v.TrangThai));
                if (action == "edit")
                {
                    SetVenue(v, request);
                    if (v.SucChua < (await db.KhuVuc.Where(z => z.DiaDiemId == id).SumAsync(z => (int?)z.SucChua, ct) ?? 0))
                        throw Conflict("Sức chứa địa điểm nhỏ hơn tổng sức chứa các khu vực đã khai.");
                }
                else v.TrangThai = action switch {
                    "show" => TrangThaiDiaDiem.HoatDong, "hide" => TrangThaiDiaDiem.DaAn, _ => throw Invalid("Chọn thao tác hợp lệ.")
                };
                break;
            case "event-categories":
                var category = await db.DanhMucSuKien.SingleOrDefaultAsync(r => r.Id == id, ct) ?? throw Missing();
                Match(request.ExpectedState, Display(category.TrangThai));
                if (action == "edit")
                {
                    category.TenDanhMuc = Required(request.Name, 120, "tên danh mục");
                    category.MoTa = Optional(request.Description, 1000, "mô tả");
                }
                else category.TrangThai = Visibility(action);
                break;
            case "banners":
                var banner = await db.Banner.SingleOrDefaultAsync(r => r.Id == id, ct) ?? throw Missing();
                Match(request.ExpectedState, Display(banner.TrangThai));
                if (action == "edit") SetBanner(banner, request);
                else banner.TrangThai = Visibility(action);
                break;
            case "policies":
                var policy = await db.ChinhSach.SingleOrDefaultAsync(r => r.Id == id, ct) ?? throw Missing();
                if (action != "publish" || policy.TrangThai != TrangThaiChinhSach.Nhap || request.ExpectedState != "DRAFT")
                    throw Conflict("Chỉ công bố phiên bản đang là bản nháp.");
                policy.TrangThai = TrangThaiChinhSach.CoHieuLuc;
                break;
            case "zones":
                if (action != "update") throw Invalid("Chọn thao tác khu vực hợp lệ.");
                var zone = await db.KhuVuc.SingleOrDefaultAsync(r => r.Id == id, ct) ?? throw Missing();
                await SqlOperations.Lock(db, $"qlsk:venue:{zone.DiaDiemId}", ct);
                await CheckZoneCapacity(zone.DiaDiemId, id, request.Capacity, ct);
                zone.SucChua = request.Capacity!.Value;
                if (!string.IsNullOrWhiteSpace(request.SeatLayout))
                {
                    if (await db.LoaiVe.AnyAsync(t => t.KhuVucId == id, ct))
                        throw Conflict("Sơ đồ đang được dùng. Tạo khu vực mới để thay bố trí.");
                    await db.Ghe.Where(g => g.KhuVucId == id).ExecuteDeleteAsync(ct);
                    AddSeats(zone, request.SeatLayout);
                }
                else if (await db.Ghe.CountAsync(g => g.KhuVucId == id, ct) > zone.SucChua)
                    throw Conflict("Sức chứa mới nhỏ hơn số ghế hiện có.");
                break;
            case "venue-blocks":
                var block = await db.LichDatDiaDiem.SingleOrDefaultAsync(r => r.Id == id && r.LoaiLich == LoaiLichDiaDiem.BaoTri, ct) ?? throw Missing();
                await SqlOperations.Lock(db, $"qlsk:venue:{block.DiaDiemId}", ct);
                if (action != "release" || block.TrangThai != TrangThaiLichDatDiaDiem.DaXacNhan)
                    throw Conflict("Lịch bảo trì đã thay đổi.");
                block.TrangThai = TrangThaiLichDatDiaDiem.DaHuy;
                break;
            default: throw Missing();
        }
        SqlOperations.Audit(db, actor, action ?? "Cập nhật danh mục", resource, id, reason);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Detail(actor, resource, id, ct);
    }

    private async Task<int> CreateZone(TaiKhoan actor, CatalogChangeRequest request, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx, CancellationToken ct)
    {
        var place = request.VenueId ?? throw Invalid("Chọn địa điểm.");
        await SqlOperations.Lock(db, $"qlsk:venue:{place}", ct);
        await CheckZoneCapacity(place, null, request.Capacity, ct);
        var zone = new KhuVuc { DiaDiemId = place, TenKhuVuc = Required(request.Name, 120, "tên khu vực"), SucChua = request.Capacity!.Value };
        db.KhuVuc.Add(zone);
        await db.SaveChangesAsync(ct);
        if (request.Seating == "ASSIGNED") AddSeats(zone, Required(request.SeatLayout, 12000, "mã ghế"));
        else if (request.Seating != "GENERAL") throw Invalid("Chọn loại chỗ hợp lệ.");
        SqlOperations.Audit(db, actor, "Tạo khu vực", nameof(KhuVuc), zone.Id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return zone.Id;
    }

    private async Task CheckZoneCapacity(int venue, int? zone, int? capacity, CancellationToken ct)
    {
        var max = await db.DiaDiem.Where(v => v.Id == venue).Select(v => (int?)v.SucChua).SingleOrDefaultAsync(ct) ?? throw Missing();
        var other = await db.KhuVuc.Where(z => z.DiaDiemId == venue && z.Id != zone).SumAsync(z => (int?)z.SucChua, ct) ?? 0;
        if (!capacity.HasValue || capacity.Value < 1 || capacity.Value > max - other)
            throw Invalid("Tổng sức chứa khu vực vượt sức chứa địa điểm.");
    }

    private void AddSeats(KhuVuc zone, string layout)
    {
        var codes = layout.Split([',', ';', '\r', '\n', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (codes.Length < 1 || codes.Length > zone.SucChua || codes.Distinct(StringComparer.OrdinalIgnoreCase).Count() != codes.Length ||
            codes.Any(code => code.Length > 32 || !code.All(c => char.IsLetterOrDigit(c) || c is '-' or '_')))
            throw Invalid("Nhập các mã ghế khác nhau, mỗi mã tối đa 32 ký tự và trong sức chứa khu vực.");
        db.Ghe.AddRange(codes.Select(label => new Ghe { KhuVucId = zone.Id, MaGhe = label,
            LoaiGhe = LoaiGheNgoi.GheDon, TrangThai = TrangThaiGhe.SuDung }));
    }

    private static void SetVenue(DiaDiem venue, CatalogChangeRequest request)
    {
        if (request.Capacity is null or < 1 || request.Rate is null or < 0 || decimal.Truncate(request.Rate.Value) != request.Rate.Value)
            throw Invalid("Giá thuê và sức chứa không hợp lệ.");
        venue.TenDiaDiem = Required(request.Name, 200, "tên địa điểm");
        venue.DiaChi = Required(request.Address, 300, "địa chỉ");
        venue.SucChua = request.Capacity.Value;
        venue.GiaThue = request.Rate.Value;
        venue.TienIch = Optional(request.Amenities, 1800, "tiện ích");
    }

    private static void SetBanner(Banner banner, CatalogChangeRequest request)
    {
        var image = Required(request.Image ?? request.Description, 500, "đường dẫn ảnh");
        banner.HinhAnh = CatalogService.SafeImage(image) ?? throw Invalid("Đường dẫn ảnh không hợp lệ.");
        banner.TieuDe = Required(request.Name, 200, "tiêu đề");
        banner.ThuTu = request.Position is >= 0 ? request.Position.Value : 0;
        var target = Optional(request.Target, 500, "liên kết");
        if (target != null && !target.StartsWith("#event/", StringComparison.Ordinal))
            throw Invalid("Liên kết banner cần dẫn đến chi tiết sự kiện QLSK.");
        banner.DuongDan = target ?? banner.DuongDan;
    }

    private static void Match(string? expected, string actual)
    {
        if (expected != actual) throw Conflict("Dữ liệu đã thay đổi. Mở lại chi tiết trước khi lưu.");
    }
    private static string Display(TrangThaiHienThi status) => status == TrangThaiHienThi.Hien ? "ACTIVE" : "HIDDEN";
    private static TrangThaiHienThi Visibility(string? action) => action switch {
        "show" => TrangThaiHienThi.Hien, "hide" => TrangThaiHienThi.An, _ => throw Invalid("Chọn thao tác hiển thị hợp lệ.")
    };
    private static string VenueStatus(TrangThaiDiaDiem status) => status == TrangThaiDiaDiem.HoatDong ? "ACTIVE" : "HIDDEN";
    private static string PolicyStatus(TrangThaiChinhSach status) => status switch {
        TrangThaiChinhSach.Nhap => "DRAFT", TrangThaiChinhSach.CoHieuLuc => "ACTIVE", _ => "EXPIRED"
    };
}
