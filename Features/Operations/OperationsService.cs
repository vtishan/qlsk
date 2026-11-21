using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Organizers;
using QLSK.Features.Requests;
using QLSK.Features.Finance;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Operations;

public sealed class OperationsService(AppDbContext db)
{
    internal static readonly string[] AdminOperations = [
        "work.accounts.list", "work.accounts.detail", "work.accounts.access", "work.accounts.assign",
        "work.settings.list", "work.settings.detail", "work.settings.update",
        "work.audit.list", "work.audit.detail", "work.monitoring.list"
    ];
    internal const string EventReviewPermission = "events.review";
    internal const string MonitorPermission = "monitoring.view";
    private static readonly Dictionary<string, string> SettingsKeys = new()
    {
        ["review.holidays"] = "Ngày nghỉ xét duyệt (danh sách ngày JSON)",
        ["booking.serviceFee"] = "Phí dịch vụ trên mỗi đơn (₫)",
        ["settlement.closeDays"] = "Số ngày chờ khóa sổ sau khi kết thúc sự kiện (mặc định 3)",
        ["settlement.responseHours"] = "Hạn BTC phản hồi đối soát (giờ, mặc định 48)"
    };
    private static readonly (int Id, string Label, string[] Operations)[] Groups =
    [
        (1, "Xét duyệt nội dung và hồ sơ", [OrganizerService.ReviewPermission, EventReviewPermission, RequestService.SupportPermission]),
        (2, "Thuê địa điểm và hỗ trợ", [RequestService.RentalPermission, RequestService.SupportPermission]),
        (3, "Tài chính, hoàn tiền và giám sát", [RefundService.ReviewPermission, FinanceService.Permission, MonitorPermission]),
        (4, "Người dùng", [])
    ];

    public IQueryable<SuKien> EventScope(TaiKhoan actor, bool seeAllForAdmin = true)
    {
        var rows = db.SuKien.AsNoTracking();
        if (seeAllForAdmin && AccountAccess.Admin(actor)) return rows;
        var owned = db.HoSoBTC.Where(x => x.TaiKhoanId == actor.Id && x.TrangThai == TrangThaiHoSoToChuc.DaDuyet);
        var grants = db.PhanCongSuKien.Where(x => x.TaiKhoanId == actor.Id && x.TrangThai == TrangThaiPhanCongSuKien.DangHieuLuc);
        return rows.Where(x => owned.Any(o => o.Id == x.BTCId) || grants.Any(g => g.SuKienId == x.Id));
    }

    internal IQueryable<SuatDien> SessionScope(TaiKhoan actor, bool seeAllForAdmin = true)
    {
        if (seeAllForAdmin && AccountAccess.Admin(actor)) return db.SuatDien.AsNoTracking();
        return db.SuatDien.AsNoTracking().Where(s =>
            db.SuKien.Any(e => e.Id == s.SuKienId && db.HoSoBTC.Any(o => o.Id == e.BTCId && o.TaiKhoanId == actor.Id &&
                o.TrangThai == TrangThaiHoSoToChuc.DaDuyet)) ||
            db.PhanCongSuKien.Any(g => g.TaiKhoanId == actor.Id && g.SuKienId == s.SuKienId &&
                (!g.SuatDienId.HasValue || g.SuatDienId == s.Id) && g.TrangThai == TrangThaiPhanCongSuKien.DangHieuLuc));
    }

    public async Task<ScopeDto> Scope(TaiKhoan actor, CancellationToken ct)
    {
        var review = AccountAccess.Platform(actor, EventReviewPermission);
        var eventScope = review ? db.SuKien.AsNoTracking() : EventScope(actor);
        var sessionScope = review ? db.SuatDien.AsNoTracking() : SessionScope(actor);
        var events = await eventScope.OrderBy(e => e.Id).Select(e => new { e.Id, e.TenSuKien }).Take(200).ToListAsync(ct);
        var ids = events.Select(e => e.Id).ToArray();
        var sessions = await sessionScope.Where(s => ids.Contains(s.SuKienId)).OrderBy(s => s.ThoiGianBatDau)
            .Select(s => new { s.Id, s.SuKienId, s.SoDoChot, s.DiaDiemId }).ToListAsync(ct);
        var venues = sessions.Select(s => s.DiaDiemId).Distinct().ToArray();
        var zones = await db.KhuVuc.AsNoTracking().Where(z => venues.Contains(z.DiaDiemId))
            .Select(z => new { z.Id, z.DiaDiemId, z.TenKhuVuc }).ToListAsync(ct);
        var drafts = await db.PhienBanSuKien.Where(v => ids.Contains(v.SuKienId) && v.TrangThaiDuyet == TrangThaiDuyetPhienBan.Nhap)
            .Where(v => v.NguoiGuiId == actor.Id || actor.VaiTro == VaiTroTaiKhoan.QuanTriVien)
            .Select(v => new { v.Id, v.SuKienId, v.SoPhienBan }).ToListAsync(ct);
        var options = new Dictionary<string, IReadOnlyList<ScopeOptionDto>>
        {
            ["category"] = await db.DanhMucSuKien.AsNoTracking().Where(c => c.TrangThai == TrangThaiHienThi.Hien).OrderBy(c => c.TenDanhMuc)
                .Take(200).Select(c => new ScopeOptionDto(c.Id, c.TenDanhMuc)).ToListAsync(ct),
            ["venue"] = await db.DiaDiem.AsNoTracking().Where(v => v.TrangThai == TrangThaiDiaDiem.HoatDong).OrderBy(v => v.TenDiaDiem)
                .Take(200).Select(v => new ScopeOptionDto(v.Id, v.TenDiaDiem)).ToListAsync(ct)
        };
        if (AccountAccess.Admin(actor))
        {
            options["permissionGroup"] = Groups.Select(g => new ScopeOptionDto(g.Id, g.Label)).ToArray();
            options["organizer"] = await db.HoSoBTC.AsNoTracking().Where(h => h.TrangThai == TrangThaiHoSoToChuc.DaDuyet)
                .OrderBy(h => h.TenToChuc).Take(200).Select(h => new ScopeOptionDto(h.Id, h.TenToChuc)).ToListAsync(ct);
        }
        var eventCards = events.Select(e =>
        {
            var eventOptions = new Dictionary<string, IReadOnlyList<ScopeOptionDto>>
            {
                ["editableVersion"] = drafts.Where(v => v.SuKienId == e.Id).Select(v => new ScopeOptionDto(v.Id, $"Bản nháp {v.SoPhienBan}")).ToArray()
            };
            return new ScopeEventDto(e.Id, e.TenSuKien, sessions.Where(s => s.SuKienId == e.Id)
                .Select(s => new ScopeSessionDto(s.Id, Catalog.CatalogService.JsonText(s.SoDoChot).GetValueOrDefault("name") ?? $"Suất {s.Id}",
                    new Dictionary<string, IReadOnlyList<ScopeOptionDto>>
                    {
                        ["zone"] = zones.Where(z => z.DiaDiemId == s.DiaDiemId).Select(z => new ScopeOptionDto(z.Id, z.TenKhuVuc)).ToArray()
                    }))
                .ToArray(), eventOptions);
        }).ToArray();
        return new(eventCards, options);
    }

    public async Task<DashboardDto> Dashboard(TaiKhoan actor, CancellationToken ct)
    {
        var metrics = new List<MetricDto>();
        if (AccountAccess.Platform(actor, OrganizerService.ReviewPermission))
            metrics.Add(new("Hồ sơ BTC chờ duyệt", await db.HoSoBTC.CountAsync(h => h.TrangThai == TrangThaiHoSoToChuc.ChoDuyet, ct)));
        if (AccountAccess.Platform(actor, EventReviewPermission))
            metrics.Add(new("Phiên bản sự kiện chờ duyệt", await db.PhienBanSuKien.CountAsync(v => v.TrangThaiDuyet == TrangThaiDuyetPhienBan.ChoDuyet, ct)));
        if (AccountAccess.Platform(actor, RequestService.RentalPermission))
            metrics.Add(new("Yêu cầu thuê chờ duyệt", await db.YeuCauThue.CountAsync(r => r.TrangThai == TrangThaiYeuCauThue.ChoDuyet, ct)));
        if (AccountAccess.Platform(actor, RequestService.SupportPermission))
            metrics.Add(new("Yêu cầu hỗ trợ mới", await db.KhieuNai.CountAsync(r => r.TrangThai == TrangThaiKhieuNai.ChoXuLy, ct)));
        if (AccountAccess.Platform(actor, RefundService.ReviewPermission))
            metrics.Add(new("Yêu cầu hoàn chờ xét", await db.YeuCauHoanTien.CountAsync(r => r.TrangThai == TrangThaiYeuCauHoanTien.ChoDuyet, ct)));
        if (actor.VaiTro is VaiTroTaiKhoan.BanToChuc or VaiTroTaiKhoan.QuanLySuKien)
            metrics.Add(new("Sự kiện trong phạm vi", await EventScope(actor).CountAsync(ct)));

        var logs = await db.NhatKyHeThong.AsNoTracking().Where(row => row.TaiKhoanId == actor.Id)
            .OrderByDescending(row => row.Id).Take(12).ToListAsync(ct);
        return new(metrics, logs.Select(row => new ActivityDto(row.HanhDong switch {
            "ChapNhanDieuKhoan" => "Chấp nhận điều khoản", "CapNhatHoSo" => "Cập nhật hồ sơ", "NopHoSo" => "Nộp hồ sơ BTC", _ => row.HanhDong
        }, row.DoiTuong switch {
            nameof(TaiKhoan) => "Tài khoản", nameof(ChinhSach) => "Chính sách", nameof(HoSoBTC) => "Hồ sơ Ban tổ chức",
            nameof(SuKien) => "Sự kiện", nameof(PhienBanSuKien) => "Phiên bản sự kiện", nameof(VeMoi) => "Vé mời",
            nameof(PhanCongSuKien) => "Phân công nhân sự", _ => row.DoiTuong
        }, Stamp(row.ThoiGian))).ToArray());
    }

    public async Task<PageDto<AdminAccountDto>> Accounts(TaiKhoan actor, string? status, string? cursor, CancellationToken ct)
    {
        AdminOnly(actor);
        var after = After(cursor);
        const int size = 35;
        var query = db.TaiKhoan.AsNoTracking().Where(t => t.Id > after);
        if (status == "ACTIVE") query = query.Where(t => t.TrangThai != TrangThaiTaiKhoan.DaKhoa);
        else if (status == "LOCKED") query = query.Where(t => t.TrangThai == TrangThaiTaiKhoan.DaKhoa);
        var rows = await query.OrderBy(t => t.Id).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(t => AccountRow(actor, t, false)).ToArray(), size, t => t.Id);
    }

    public async Task<AdminAccountDto> Account(TaiKhoan actor, int id, CancellationToken ct)
    {
        AdminOnly(actor);
        var item = await db.TaiKhoan.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id, ct) ?? throw Missing();
        return AccountRow(actor, item, true);
    }

    public async Task<AdminAccountDto> ChangeAccount(TaiKhoan actor, int id, AccountAdminRequest request, CancellationToken ct)
    {
        AdminOnly(actor);
        if (actor.Id == id) throw Denied();
        var reason = Required(request.Reason, 1000, "lý do");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:account:{id}", ct);
        var item = await db.TaiKhoan.SingleOrDefaultAsync(t => t.Id == id, ct) ?? throw Missing();
        if (AccountAccess.Admin(item)) throw Denied();
        if (request.ExpectedState != AccountState(item)) throw Conflict("Trạng thái tài khoản đã thay đổi.");
        switch (request.Action)
        {
            case "lock":
                item.TrangThai = TrangThaiTaiKhoan.DaKhoa;
                break;
            case "unlock":
                item.TrangThai = TrangThaiTaiKhoan.HoatDong;
                break;
            case "revoke_sessions":
                break;
            case "assign":
                if (item.VaiTro is not (VaiTroTaiKhoan.NguoiDung or VaiTroTaiKhoan.QuanLyHeThong)) throw Denied();
                var group = Groups.SingleOrDefault(g => g.Id == request.PermissionGroupId);
                if (group.Id == 0) throw Invalid("Chọn nhóm quyền.");
                item.VaiTro = group.Operations.Length == 0 ? VaiTroTaiKhoan.NguoiDung : VaiTroTaiKhoan.QuanLyHeThong;
                item.QuyenQuanLy = group.Operations.Length == 0 ? null : JsonSerializer.Serialize(group.Operations.ToDictionary(o => o, _ => true));
                break;
            default: throw Invalid("Thao tác tài khoản không hợp lệ.");
        }
        item.PhienBanDangNhap++;
        SqlOperations.Audit(db, actor, request.Action, nameof(TaiKhoan), id, reason);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return AccountRow(actor, item, true);
    }

    private static AdminAccountDto AccountRow(TaiKhoan actor, TaiKhoan item, bool detail)
    {
        var actions = new List<string>();
        if (detail && actor.Id != item.Id && !AccountAccess.Admin(item))
        {
            actions.Add("access");
            if (item.VaiTro is VaiTroTaiKhoan.NguoiDung or VaiTroTaiKhoan.QuanLyHeThong) actions.Add("assign");
        }
        return new(item.Id, item.HoTen, AccountAccess.RoleLabel(item.VaiTro),
            item.EmailXacMinhLuc.HasValue || item.SoDienThoaiXacMinhLuc.HasValue ? "Đã xác minh" : "Chưa xác minh", AccountState(item),
            item.TrangThai == TrangThaiTaiKhoan.DaKhoa ? "Đã khóa" : "Hoạt động", actions,
            detail ? [new("Tên đăng nhập", item.TenDangNhap), new("Email", item.Email), new("Điện thoại", item.SoDienThoai)] : null);
    }

    private static string AccountState(TaiKhoan actor) => actor.TrangThai == TrangThaiTaiKhoan.DaKhoa ? "LOCKED" : "ACTIVE";

    public async Task<PageDto<SettingDto>> Settings(TaiKhoan actor, CancellationToken ct)
    {
        AdminOnly(actor);
        var keys = SettingsKeys.Keys.ToArray();
        var values = await db.CauHinhHeThong.AsNoTracking().Where(x => keys.Contains(x.Khoa)).ToDictionaryAsync(x => x.Khoa, ct);
        return new(SettingsKeys.Select(x => SettingRow(x.Key, values.GetValueOrDefault(x.Key))).ToArray());
    }

    public async Task<SettingDto> Setting(TaiKhoan actor, string key, CancellationToken ct)
    {
        AdminOnly(actor);
        if (!SettingsKeys.ContainsKey(key)) throw Missing();
        return SettingRow(key, await db.CauHinhHeThong.AsNoTracking().SingleOrDefaultAsync(x => x.Khoa == key, ct));
    }

    public async Task<SettingDto> Set(TaiKhoan actor, SettingRequest request, CancellationToken ct)
    {
        AdminOnly(actor);
        var key = Required(request.Key, 100, "khóa cấu hình");
        if (!SettingsKeys.ContainsKey(key)) throw Missing();
        var value = Required(request.Value, 10000, "giá trị");
        var reason = Required(request.Reason, 1000, "lý do");
        if (key == "review.holidays")
        {
            DateOnly[] days;
            try { days = JsonSerializer.Deserialize<DateOnly[]>(value) ?? throw Invalid("Nhập danh sách ngày JSON."); }
            catch (JsonException) { throw Invalid("Nhập danh sách ngày JSON theo định dạng YYYY-MM-DD."); }
            if (days.Length > 366) throw Invalid("Mỗi cấu hình hỗ trợ tối đa 366 ngày nghỉ.");
            value = JsonSerializer.Serialize(days.Distinct().Order());
        }
        else if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var fee) || fee < 0)
            throw Invalid("Phí dịch vụ phải là số nguyên không âm.");
        if (key == "settlement.closeDays" && int.Parse(value, CultureInfo.InvariantCulture) > 365 ||
            key == "settlement.responseHours" && (int.Parse(value, CultureInfo.InvariantCulture) < 1 || int.Parse(value, CultureInfo.InvariantCulture) > 720))
            throw Invalid("Ngày khóa sổ từ 0 đến 365; hạn phản hồi từ 1 đến 720 giờ.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:setting:{key}", ct);
        var row = await db.CauHinhHeThong.SingleOrDefaultAsync(x => x.Khoa == key, ct);
        if (row == null)
        {
            row = new() { Khoa = key };
            db.CauHinhHeThong.Add(row);
        }
        row.GiaTri = value;
        row.NgayCapNhat = DateTime.UtcNow;
        row.NguoiCapNhatId = actor.Id;
        await db.SaveChangesAsync(ct);
        SqlOperations.Audit(db, actor, "Điều chỉnh cấu hình", nameof(CauHinhHeThong), row.Id, reason);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return SettingRow(key, row);
    }

    private static SettingDto SettingRow(string key, CauHinhHeThong? row) =>
        new(key, SettingsKeys[key], row?.GiaTri ?? "Chưa cấu hình", Stamp(row?.NgayCapNhat), ["update"]);

    public async Task<PageDto<AuditDto>> Audit(TaiKhoan actor, string? cursor, CancellationToken ct)
    {
        AdminOnly(actor);
        const int size = 50;
        var after = After(cursor);
        var rows = await db.NhatKyHeThong.AsNoTracking().Where(x => x.Id > after).OrderBy(x => x.Id).Take(size + 1)
            .Select(x => new { Row = x, Name = db.TaiKhoan.Where(t => t.Id == x.TaiKhoanId).Select(t => t.HoTen).FirstOrDefault() }).ToListAsync(ct);
        return Page(rows.Select(x => AuditRow(x.Row, x.Name, false)).ToArray(), size, x => x.Id);
    }

    public async Task<AuditDto> AuditRecord(TaiKhoan actor, int id, CancellationToken ct)
    {
        AdminOnly(actor);
        var row = await db.NhatKyHeThong.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id, ct) ?? throw Missing();
        var name = await db.TaiKhoan.Where(t => t.Id == row.TaiKhoanId).Select(t => t.HoTen).FirstOrDefaultAsync(ct);
        return AuditRow(row, name, true);
    }

    private static AuditDto AuditRow(NhatKyHeThong entry, string? name, bool detail) =>
        new(entry.Id, entry.HanhDong, name, entry.DoiTuong, Stamp(entry.ThoiGian),
            detail ? [new("Mã đối tượng", entry.DoiTuongId?.ToString()), new("Ghi chú", SqlOperations.AuditNote(entry.DuLieuThayDoi))] : null);

    public async Task<PageDto<ServiceMonitorDto>> Monitoring(TaiKhoan actor, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, MonitorPermission);
        var now = DateTime.UtcNow;
        return new([
            new("Webhook thanh toán", "Dữ liệu SQL Server", $"{await db.WebhookThanhToan.CountAsync(w => w.TrangThaiXuLy == TrangThaiXuLyWebhook.ChoXuLy || w.TrangThaiXuLy == TrangThaiXuLyWebhook.Loi, ct)} bản ghi cần kiểm tra", Stamp(now)),
            new("Giữ ghế", "Dữ liệu SQL Server", $"{await db.GiuGhe.CountAsync(h => h.TrangThai == TrangThaiGiuGhe.DangGiu && h.HetHan < now, ct)} lượt giữ đã hết hạn", Stamp(now)),
            new("Cọc thuê", "Dữ liệu SQL Server", $"{await db.YeuCauThue.CountAsync(r => r.TrangThai == TrangThaiYeuCauThue.ChoCoc && r.HanDatCoc < now, ct)} yêu cầu đã quá hạn cọc", Stamp(now))
        ]);
    }

    private static void AdminOnly(TaiKhoan actor)
    {
        if (!AccountAccess.Admin(actor)) throw Denied();
    }
}
