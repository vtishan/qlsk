using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Catalog;
using QLSK.Features.Operations;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Events;

public sealed class EventService(AppDbContext db, CatalogService catalog)
{
    internal static readonly string[] ReadOperations = [
        "work.events.list", "work.events.detail", "work.versions.list", "work.versions.detail",
        "work.sessions.list", "work.sessions.detail", "work.ticketTypes.list", "work.ticketTypes.detail"];
    internal static readonly string[] OrganizerOperations = [
        "work.events.create", "work.versions.create", "work.versions.submit", "work.sessions.create",
        "work.sessions.updateDraft", "work.ticketTypes.create", "work.ticketTypes.updateDraft", "work.events.complete", "work.events.publish", "work.events.reopen"];
    internal static readonly string[] ReviewOperations = ["work.reviews.list", "work.reviews.detail", "work.reviews.decide", "work.events.suspend", "work.events.complete"];

    internal Task<bool> Owns(TaiKhoan actor, int eventId, CancellationToken ct) => db.SuKien.AnyAsync(e => e.Id == eventId &&
        db.HoSoBTC.Any(o => o.Id == e.BTCId && o.TaiKhoanId == actor.Id && o.TrangThai == TrangThaiHoSoToChuc.DaDuyet), ct);

    internal async Task NeedOwner(TaiKhoan actor, int eventId, CancellationToken ct)
    {
        if (!await Owns(actor, eventId, ct)) throw Denied();
    }

    internal async Task NeedRead(TaiKhoan actor, int eventId, CancellationToken ct)
    {
        if (!await Owns(actor, eventId, ct) && !AccountAccess.Platform(actor, OperationsService.EventReviewPermission)) throw Denied();
    }

    public async Task<PageDto<EventWorkDto>> List(TaiKhoan actor, bool review, string? status, string? cursor, CancellationToken ct)
    {
        const int size = 30;
        var after = After(cursor);
        if (review)
        {
            AccountAccess.NeedPlatform(actor, OperationsService.EventReviewPermission);
            var versions = db.PhienBanSuKien.AsNoTracking().Where(v => v.TrangThaiDuyet == TrangThaiDuyetPhienBan.ChoDuyet);
            if (status != null)
            {
                var type = status switch {
                    "PENDING" => LoaiYeuCauSuKien.TaoMoi, "PENDING_UPDATE" => LoaiYeuCauSuKien.CapNhat,
                    "POSTPONEMENT_REQUESTED" => LoaiYeuCauSuKien.Hoan, "CANCEL_REQUESTED" => LoaiYeuCauSuKien.Huy,
                    _ => throw Invalid("Trạng thái hàng đợi không hợp lệ.")
                };
                versions = versions.Where(v => v.LoaiYeuCau == type);
            }
            var rows = await (from e in db.SuKien.AsNoTracking() join v in versions on e.Id equals v.SuKienId
                join owner in db.HoSoBTC on e.BTCId equals owner.Id where e.Id > after orderby e.Id
                select new { Event = e, Version = v, owner.TenToChuc }).Take(size + 1).ToListAsync(ct);
            return Page(rows.Select(r => Project(r.Event, r.TenToChuc, r.Version, null, true)).ToArray(), size, row => row.Id);
        }

        var owned = db.SuKien.AsNoTracking().Where(e => db.HoSoBTC.Any(h => h.Id == e.BTCId && h.TaiKhoanId == actor.Id &&
            h.TrangThai == TrangThaiHoSoToChuc.DaDuyet));
        var source = AccountAccess.Platform(actor, OperationsService.EventReviewPermission) ? db.SuKien.AsNoTracking() : owned;
        var query = source.Where(e => e.Id > after);
        if (status == "APPROVED_PRIVATE")
            query = query.Where(e => e.TrangThai == TrangThaiSuKien.DaDuyet && !e.PhienBanCongKhaiId.HasValue);
        else if (status != null)
        {
            var state = Enum.GetValues<TrangThaiSuKien>().SingleOrDefault(s => State(s) == status);
            query = query.Where(e => e.TrangThai == state);
        }
        var result = await (from e in query join o in db.HoSoBTC on e.BTCId equals o.Id orderby e.Id
            select new { Event = e, o.TenToChuc, Start = db.SuatDien.Where(s => s.SuKienId == e.Id && s.TrangThai != TrangThaiSuatDien.Nhap)
                .Min(s => (DateTime?)s.ThoiGianBatDau) }).Take(size + 1).ToListAsync(ct);
        return Page(result.Select(row => Project(row.Event, row.TenToChuc, null, row.Start, false)).ToArray(), size, row => row.Id);
    }

    public async Task<EventWorkDto> Detail(TaiKhoan actor, int id, CancellationToken ct)
    {
        await NeedRead(actor, id, ct);
        var item = await (from e in db.SuKien.AsNoTracking() join o in db.HoSoBTC on e.BTCId equals o.Id
                          where e.Id == id select new { Event = e, o.TenToChuc, o.TaiKhoanId }).SingleOrDefaultAsync(ct) ?? throw Missing();
        var reviewer = AccountAccess.Platform(actor, OperationsService.EventReviewPermission) && item.TaiKhoanId != actor.Id;
        var waiting = reviewer ? await db.PhienBanSuKien.AsNoTracking().Where(v => v.SuKienId == id && v.TrangThaiDuyet == TrangThaiDuyetPhienBan.ChoDuyet)
            .OrderByDescending(v => v.Id).FirstOrDefaultAsync(ct) : null;
        var actions = new List<string>();
        if (await CompletionReady(item.Event, DateTime.UtcNow, ct)) actions.Add("complete");
        if (waiting != null) actions.Add("decide");
        if (reviewer && item.Event.PhienBanCongKhaiId.HasValue &&
            item.Event.TrangThai is not (TrangThaiSuKien.DinhChi or TrangThaiSuKien.DaHuy or TrangThaiSuKien.HoanTat)) actions.Add("suspend");
        var versions = await db.PhienBanSuKien.AsNoTracking().Where(v => v.SuKienId == id)
            .OrderBy(v => v.SoPhienBan).Take(50).ToListAsync(ct);
        var dto = Project(item.Event, item.TenToChuc, waiting, null, waiting != null);
        var reviewedInitial = await InitialPublicationVersion(id, ct);
        if (reviewedInitial != null)
        {
            dto = dto with { VersionId = reviewedInitial.Id };
            if (actor.VaiTro == VaiTroTaiKhoan.BanToChuc && await Owns(actor, id, ct)) actions.AddRange(["publish", "reopen"]);
        }
        var fields = waiting == null ? null : new DetailFieldDto[] {
            new("Tên đang công khai", item.Event.PhienBanCongKhaiId.HasValue ? item.Event.TenSuKien : null),
            new("Nội dung đang công khai", item.Event.PhienBanCongKhaiId.HasValue ? item.Event.MoTa : null),
            new("Đề xuất", Content(waiting).Name), new("Lý do trình duyệt", waiting.LyDo)
        };
        return dto with { AllowedActions = actions, Fields = fields, History = versions.Select(v =>
            new HistoryDto(Stamp(v.NgayDuyet ?? v.NgayGui), VersionState(v), TypeLabel(v.LoaiYeuCau), null, v.KetQuaDuyet ?? v.LyDo)).ToArray() };
    }

    public async Task<int> Create(TaiKhoan actor, EventCreateRequest request, CancellationToken ct)
    {
        if (actor.VaiTro != VaiTroTaiKhoan.BanToChuc) throw Denied();
        var profile = await db.HoSoBTC.AsNoTracking().Where(h => h.TaiKhoanId == actor.Id && h.TrangThai == TrangThaiHoSoToChuc.DaDuyet)
            .OrderByDescending(h => h.Id).FirstOrDefaultAsync(ct) ?? throw Denied();
        if (!await db.DanhMucSuKien.AnyAsync(c => c.Id == request.CategoryId && c.TrangThai == TrangThaiHienThi.Hien, ct))
            throw Invalid("Danh mục chưa khả dụng.");
        var image = Optional(request.Image, 500, "ảnh bìa");
        if (image != null && CatalogService.SafeImage(image) == null) throw Invalid("Đường dẫn ảnh không hợp lệ.");
        var content = new EventContent(Required(request.Name, 200, "tên sự kiện"), request.CategoryId,
            Required(request.Description, 4000, "mô tả"), Optional(request.Program, 5000, "chương trình"),
            Optional(request.Participants, 1500, "nghệ sĩ và diễn giả"), Optional(request.Rules, 3500, "quy định tham dự"),
            Optional(request.Policy, 3500, "chính sách vé"), image, request.AllowRefund == "yes");
        var item = new SuKien {
            BTCId = profile.Id, DanhMucId = content.CategoryId, TenSuKien = content.Name, MoTa = content.Description!,
            AnhBia = image ?? "", TrangThai = TrangThaiSuKien.Nhap, NgayTao = DateTime.UtcNow, ChoPhepTuHuyVe = content.AllowRefund
        };
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.SuKien.Add(item);
        await db.SaveChangesAsync(ct);
        db.PhienBanSuKien.Add(new PhienBanSuKien {
            SuKienId = item.Id, SoPhienBan = 1, LoaiYeuCau = LoaiYeuCauSuKien.TaoMoi, NguoiGuiId = actor.Id,
            TrangThaiDuyet = TrangThaiDuyetPhienBan.Nhap, NoiDung = JsonSerializer.Serialize(content, JsonSerializerOptions.Web)
        });
        SqlOperations.Audit(db, actor, "Tạo sự kiện nháp", nameof(SuKien), item.Id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return item.Id;
    }

    public async Task<PageDto<VersionDto>> Versions(TaiKhoan actor, int id, string? status, string? cursor, CancellationToken ct)
    {
        await NeedRead(actor, id, ct);
        var after = After(cursor);
        const int size = 30;
        var isOwner = await Owns(actor, id, ct);
        var query = db.PhienBanSuKien.AsNoTracking().Where(v => v.SuKienId == id && v.Id > after);
        if (status != null)
        {
            var phase = status switch
            {
                "DRAFT" => TrangThaiDuyetPhienBan.Nhap, "APPROVED" => TrangThaiDuyetPhienBan.DaDuyet,
                "REJECTED" => TrangThaiDuyetPhienBan.TuChoi, _ => TrangThaiDuyetPhienBan.ChoDuyet
            };
            query = query.Where(v => v.TrangThaiDuyet == phase);
            if (phase == TrangThaiDuyetPhienBan.ChoDuyet)
            {
                var type = status switch
                {
                    "PENDING" => LoaiYeuCauSuKien.TaoMoi, "PENDING_UPDATE" => LoaiYeuCauSuKien.CapNhat,
                    "POSTPONEMENT_REQUESTED" => LoaiYeuCauSuKien.Hoan, "CANCEL_REQUESTED" => LoaiYeuCauSuKien.Huy,
                    _ => throw Invalid("Trạng thái phiên bản không hợp lệ.")
                };
                query = query.Where(v => v.LoaiYeuCau == type);
            }
        }
        var rows = await query.OrderBy(v => v.Id).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(v => VersionRow(v, isOwner)).ToArray(), size, v => v.Id);
    }

    public async Task<VersionDto> Version(TaiKhoan actor, int id, int versionId, CancellationToken ct)
    {
        await NeedRead(actor, id, ct);
        var v = await db.PhienBanSuKien.AsNoTracking().SingleOrDefaultAsync(x => x.SuKienId == id && x.Id == versionId, ct) ?? throw Missing();
        return VersionRow(v, await Owns(actor, id, ct));
    }

    public async Task<int> CreateVersion(TaiKhoan actor, int id, EventVersionRequest request, CancellationToken ct)
    {
        await NeedOwner(actor, id, ct);
        var type = request.ChangeType switch {
            "UPDATE" => LoaiYeuCauSuKien.CapNhat, "POSTPONE" => LoaiYeuCauSuKien.Hoan, "CANCEL" => LoaiYeuCauSuKien.Huy,
            _ => throw Invalid("Loại phiên bản không hợp lệ.")
        };
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        var evt = await db.SuKien.SingleOrDefaultAsync(e => e.Id == id, ct) ?? throw Missing();
        if (evt.TrangThai is TrangThaiSuKien.DaHuy or TrangThaiSuKien.HoanTat or TrangThaiSuKien.DinhChi)
            throw Conflict("Sự kiện không còn nhận bản sửa.");
        if (type != LoaiYeuCauSuKien.CapNhat && !evt.PhienBanCongKhaiId.HasValue)
            throw Conflict("Chỉ yêu cầu hoãn hoặc hủy sự kiện đã công khai.");
        var prior = await db.PhienBanSuKien.Where(v => v.SuKienId == id).OrderByDescending(v => v.SoPhienBan).FirstAsync(ct);
        var baseline = evt.PhienBanCongKhaiId.HasValue
            ? await db.PhienBanSuKien.SingleAsync(v => v.Id == evt.PhienBanCongKhaiId.Value, ct) : prior;
        if (!evt.PhienBanCongKhaiId.HasValue) type = LoaiYeuCauSuKien.TaoMoi;
        var content = Content(baseline);
        if (content.CategoryId < 1)
            content = content with { CategoryId = evt.DanhMucId, Name = evt.TenSuKien,
                Description = evt.MoTa, Image = evt.AnhBia, AllowRefund = evt.ChoPhepTuHuyVe };
        if (type is LoaiYeuCauSuKien.TaoMoi or LoaiYeuCauSuKien.CapNhat)
            content = content with { Name = Required(request.Name, 200, "tên phiên bản"), Description = Required(request.Description, 4000, "nội dung") };
        var row = new PhienBanSuKien { SuKienId = id, SoPhienBan = prior.SoPhienBan + 1, LoaiYeuCau = type,
            TrangThaiDuyet = TrangThaiDuyetPhienBan.Nhap, NguoiGuiId = actor.Id, LyDo = Required(request.Reason, 1000, "lý do"),
            NoiDung = JsonSerializer.Serialize(content, JsonSerializerOptions.Web) };
        db.PhienBanSuKien.Add(row);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return row.Id;
    }

    public async Task<VersionDto> Submit(TaiKhoan actor, int id, int versionId, DecisionRequest request, CancellationToken ct)
    {
        await NeedOwner(actor, id, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        var evt = await db.SuKien.SingleOrDefaultAsync(e => e.Id == id, ct) ?? throw Missing();
        var version = await Draft(id, versionId, ct);
        if (request.ExpectedState != "DRAFT") throw Conflict("Phiên bản đã thay đổi.");
        if (await db.PhienBanSuKien.AnyAsync(v => v.SuKienId == id && v.TrangThaiDuyet == TrangThaiDuyetPhienBan.ChoDuyet, ct))
            throw Conflict("Đợi kết quả của phiên bản đang xét duyệt.");
        if (!evt.PhienBanCongKhaiId.HasValue && (await DraftSessions(id, versionId, ct)).Count == 0)
            throw Invalid("Thêm tối thiểu một suất diễn vào bản nháp trước khi gửi duyệt.");
        version.NgayGui = DateTime.UtcNow;
        version.NguoiGuiId = actor.Id;
        version.LyDo = Optional(request.Reason, 1000, "lý do") ?? version.LyDo;
        version.TrangThaiDuyet = TrangThaiDuyetPhienBan.ChoDuyet;
        if (!evt.PhienBanCongKhaiId.HasValue) evt.TrangThai = TrangThaiSuKien.ChoDuyet;
        SqlOperations.Audit(db, actor, "Gửi bản xét duyệt", nameof(PhienBanSuKien), versionId);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return VersionRow(version, true);
    }

    internal async Task<PhienBanSuKien> Draft(int eventId, int versionId, CancellationToken ct)
    {
        if (await db.SuKien.AnyAsync(e => e.Id == eventId && (e.TrangThai == TrangThaiSuKien.HoanTat || e.TrangThai == TrangThaiSuKien.DaHuy || e.TrangThai == TrangThaiSuKien.DinhChi), ct))
            throw Conflict("Sự kiện đã kết thúc hoặc không còn nhận bản sửa.");
        var version = await db.PhienBanSuKien.SingleOrDefaultAsync(v => v.Id == versionId && v.SuKienId == eventId, ct) ?? throw Missing();
        if (version.TrangThaiDuyet != TrangThaiDuyetPhienBan.Nhap) throw Conflict("Phiên bản không còn trong trạng thái nháp.");
        return version;
    }

    public async Task<EventWorkDto> Decide(TaiKhoan actor, int id, int versionId, DecisionRequest request, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, OperationsService.EventReviewPermission);
        if (request.Action is not ("approve" or "reject")) throw Invalid("Quyết định phiên bản không hợp lệ.");
        var reason = Required(request.Reason, 1000, "lý do xét duyệt");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        var evt = await db.SuKien.SingleOrDefaultAsync(e => e.Id == id, ct) ?? throw Missing();
        var version = await db.PhienBanSuKien.SingleOrDefaultAsync(v => v.Id == versionId && v.SuKienId == id, ct) ?? throw Missing();
        var owner = await db.HoSoBTC.SingleAsync(h => h.Id == evt.BTCId, ct);
        if (owner.TaiKhoanId == actor.Id || version.NguoiGuiId == actor.Id) throw Denied();
        if (version.TrangThaiDuyet != TrangThaiDuyetPhienBan.ChoDuyet || VersionState(version) != request.ExpectedState)
            throw Conflict("Phiên bản đã được xử lý. Tải lại trước khi quyết định.");
        if (request.Action == "approve")
        {
            if (evt.TrangThai == TrangThaiSuKien.DinhChi) throw Conflict("Sự kiện đang bị đình chỉ.");
            if (version.LoaiYeuCau is LoaiYeuCauSuKien.Hoan or LoaiYeuCauSuKien.Huy)
                await ChangeLifecycle(evt, version, ct);
            else if (!evt.PhienBanCongKhaiId.HasValue)
            {
                // Review validates the proposed schedule without making it public or reserving a venue.
                await Publish(evt, version, ct, apply: false);
                evt.TrangThai = TrangThaiSuKien.DaDuyet;
            }
            else await Publish(evt, version, ct);
            version.TrangThaiDuyet = TrangThaiDuyetPhienBan.DaDuyet;
        }
        else
        {
            version.TrangThaiDuyet = TrangThaiDuyetPhienBan.TuChoi;
            if (!evt.PhienBanCongKhaiId.HasValue) evt.TrangThai = TrangThaiSuKien.TuChoi;
        }
        version.NgayDuyet = DateTime.UtcNow;
        version.NguoiDuyetId = actor.Id;
        version.KetQuaDuyet = reason;
        SqlOperations.Audit(db, actor, request.Action == "approve" ? "Duyệt phiên bản" : "Từ chối phiên bản",
            nameof(PhienBanSuKien), versionId, reason);
        SqlOperations.Notify(db, owner.TaiKhoanId, "Kết quả xét duyệt sự kiện", request.Action == "approve" ? "Đã duyệt" : "Từ chối", "event");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Detail(actor, id, ct);
    }

    public async Task<EventWorkDto> PublishInitial(TaiKhoan actor, int id, int versionId, string? note, CancellationToken ct)
    {
        if (actor.VaiTro != VaiTroTaiKhoan.BanToChuc) throw Denied();
        await NeedOwner(actor, id, ct);
        var reason = Required(note, 1000, "ghi chú công bố sự kiện");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        await NeedOwner(actor, id, ct);
        var evt = await db.SuKien.SingleOrDefaultAsync(e => e.Id == id, ct) ?? throw Missing();
        var version = await db.PhienBanSuKien.SingleOrDefaultAsync(v => v.Id == versionId && v.SuKienId == id, ct) ?? throw Missing();
        if (evt.PhienBanCongKhaiId == versionId && version.TrangThaiDuyet == TrangThaiDuyetPhienBan.DaDuyet &&
            evt.TrangThai is TrangThaiSuKien.DaDuyet or TrangThaiSuKien.DangMoBan)
        {
            await tx.CommitAsync(ct);
            return await Detail(actor, id, ct);
        }
        if ((await InitialPublicationVersion(id, ct))?.Id != versionId)
            throw Conflict("Chỉ công bố phiên bản tạo mới đã được quản lý duyệt và chưa có bản thay thế.");
        await Publish(evt, version, ct);
        SqlOperations.Audit(db, actor, "BTC công bố sự kiện", nameof(SuKien), id, reason);
        SqlOperations.Notify(db, actor.Id, "Sự kiện đã công khai", evt.TenSuKien, "event");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Detail(actor, id, ct);
    }

    internal async Task<PhienBanSuKien?> InitialPublicationVersion(int eventId, CancellationToken ct)
    {
        var evt = await db.SuKien.AsNoTracking().SingleOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null || evt.PhienBanCongKhaiId.HasValue || evt.TrangThai != TrangThaiSuKien.DaDuyet) return null;
        var version = await db.PhienBanSuKien.AsNoTracking().Where(v => v.SuKienId == eventId)
            .OrderByDescending(v => v.SoPhienBan).FirstOrDefaultAsync(ct);
        if (version == null || version.LoaiYeuCau != LoaiYeuCauSuKien.TaoMoi ||
            version.TrangThaiDuyet != TrangThaiDuyetPhienBan.DaDuyet || !version.NguoiDuyetId.HasValue) return null;
        var owner = await db.HoSoBTC.AsNoTracking().SingleAsync(h => h.Id == evt.BTCId, ct);
        return owner.TrangThai == TrangThaiHoSoToChuc.DaDuyet && owner.TaiKhoanId != version.NguoiDuyetId ? version : null;
    }

    public async Task<EventWorkDto> ReopenInitial(TaiKhoan actor, int id, int versionId, string? note, CancellationToken ct)
    {
        if (actor.VaiTro != VaiTroTaiKhoan.BanToChuc) throw Denied();
        await NeedOwner(actor, id, ct);
        var reason = Required(note, 1000, "lý do mở lại bản nháp");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        await NeedOwner(actor, id, ct);
        if ((await InitialPublicationVersion(id, ct))?.Id != versionId)
            throw Conflict("Chỉ mở lại bản tạo mới đã duyệt nhưng chưa công bố.");
        var evt = await db.SuKien.SingleAsync(e => e.Id == id, ct);
        var version = await db.PhienBanSuKien.SingleAsync(v => v.Id == versionId && v.SuKienId == id, ct);
        SqlOperations.Audit(db, actor, "BTC mở lại bản duyệt trước công bố", nameof(PhienBanSuKien), versionId,
            reason + "; Kết quả duyệt trước: " + version.KetQuaDuyet);
        version.TrangThaiDuyet = TrangThaiDuyetPhienBan.Nhap;
        version.NguoiDuyetId = null;
        version.NgayDuyet = null;
        version.KetQuaDuyet = null;
        evt.TrangThai = TrangThaiSuKien.Nhap;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Detail(actor, id, ct);
    }

    private async Task Publish(SuKien evt, PhienBanSuKien version, CancellationToken ct, bool apply = true)
    {
        var content = Content(version);
        if (string.IsNullOrWhiteSpace(content.Name) || content.CategoryId < 1)
            throw Conflict("Phiên bản cũ thiếu tên hoặc danh mục. Tạo bản nháp mới để bổ sung trước khi công bố.");
        if (!await db.DanhMucSuKien.AnyAsync(c => c.Id == content.CategoryId && c.TrangThai == TrangThaiHienThi.Hien, ct))
            throw Conflict("Danh mục của phiên bản không còn khả dụng.");
        var slots = await DraftSessions(evt.Id, version.Id, ct);
        var now = DateTime.UtcNow;
        if (!evt.PhienBanCongKhaiId.HasValue && slots.Count == 0) throw Conflict("Bản nháp thiếu suất diễn.");
        var ownerId = await db.HoSoBTC.Where(h => h.Id == evt.BTCId).Select(h => h.TaiKhoanId).SingleAsync(ct);
        foreach (var location in slots.Select(s => s.DiaDiemId).Distinct().Order())
            await SqlOperations.Lock(db, $"qlsk:venue:{location}", ct);
        foreach (var slot in slots)
        {
            var location = await db.DiaDiem.AsNoTracking().SingleOrDefaultAsync(v => v.Id == slot.DiaDiemId, ct);
            if (location == null || location.TrangThai != TrangThaiDiaDiem.HoatDong || slot.SucChua < 1 || slot.SucChua > location.SucChua ||
                slot.ThoiGianKetThuc <= slot.ThoiGianBatDau || slot.ThoiGianMoBan >= slot.ThoiGianDongBan || slot.ThoiGianDongBan > slot.ThoiGianBatDau)
                throw Conflict("Địa điểm, sức chứa hoặc thời gian bán của suất không còn hợp lệ.");
            var types = await (from ticket in db.LoaiVe.AsNoTracking() join zone in db.KhuVuc.AsNoTracking() on ticket.KhuVucId equals zone.Id
                               where ticket.SuatDienId == slot.Id select new { Ticket = ticket, Zone = zone }).ToListAsync(ct);
            if (types.Any(t => t.Zone.DiaDiemId != slot.DiaDiemId || t.Ticket.SoLuong < 1 || t.Ticket.GiaVe < 0 ||
                decimal.Truncate(t.Ticket.GiaVe) != t.Ticket.GiaVe || (t.Ticket.NhomVe == NhomVe.VeMoi && t.Ticket.GiaVe != 0) ||
                t.Ticket.BatDauBan >= t.Ticket.KetThucBan || t.Ticket.BatDauBan < slot.ThoiGianMoBan || t.Ticket.KetThucBan > slot.ThoiGianDongBan) ||
                types.Sum(t => (long)t.Ticket.SoLuong) > slot.SucChua ||
                types.GroupBy(t => t.Zone.Id).Any(g => g.Sum(t => (long)t.Ticket.SoLuong) > g.First().Zone.SucChua))
                throw Conflict("Giá vé, thời gian bán hoặc hạn mức khu vực của suất không còn hợp lệ.");
            var rentalId = ScheduleService.RentalOf(slot);
            var rental = await ScheduleService.ValidateRental(db, ownerId, rentalId, slot.DiaDiemId,
                slot.ThoiGianBatDau, slot.ThoiGianKetThuc, ct);
            if (rental != null && !await db.LichDatDiaDiem.AnyAsync(l => l.YeuCauThueId == rental.Id &&
                l.LoaiLich == LoaiLichDiaDiem.Thue && l.TrangThai == TrangThaiLichDatDiaDiem.DaXacNhan &&
                l.DiaDiemId == slot.DiaDiemId && l.BatDau <= slot.ThoiGianBatDau && l.KetThuc >= slot.ThoiGianKetThuc, ct))
                throw Conflict("Lịch thuê đã chọn không còn xác nhận hoặc không phủ đủ suất diễn.");
            if (slot.ThoiGianBatDau <= now ||
                slots.Any(s => s.Id != slot.Id && s.DiaDiemId == slot.DiaDiemId && s.ThoiGianBatDau < slot.ThoiGianKetThuc && s.ThoiGianKetThuc > slot.ThoiGianBatDau) ||
                await catalog.Busy(now).AnyAsync(l => l.DiaDiemId == slot.DiaDiemId && l.BatDau < slot.ThoiGianKetThuc && l.KetThuc > slot.ThoiGianBatDau &&
                    !(rentalId.HasValue && l.YeuCauThueId == rentalId && l.LoaiLich == LoaiLichDiaDiem.Thue &&
                      l.TrangThai == TrangThaiLichDatDiaDiem.DaXacNhan), ct))
                throw Conflict("Có suất đã qua hoặc trùng lịch địa điểm.");
            if (!apply) continue;
            slot.TrangThai = TrangThaiSuatDien.DaXacNhan;
            if (!await db.LichDatDiaDiem.AnyAsync(l => l.SuatDienId == slot.Id && l.TrangThai != TrangThaiLichDatDiaDiem.DaHuy, ct))
                db.LichDatDiaDiem.Add(new LichDatDiaDiem {
                DiaDiemId = slot.DiaDiemId, SuatDienId = slot.Id, BatDau = slot.ThoiGianBatDau, KetThuc = slot.ThoiGianKetThuc,
                LoaiLich = LoaiLichDiaDiem.SuatDien, TrangThai = TrangThaiLichDatDiaDiem.DaXacNhan
            });
        }
        if (!apply) return;
        evt.TenSuKien = content.Name;
        evt.MoTa = content.Description ?? "";
        evt.DanhMucId = content.CategoryId;
        evt.AnhBia = content.Image ?? "";
        evt.ChoPhepTuHuyVe = content.AllowRefund;
        evt.PhienBanCongKhaiId = version.Id;
        evt.TrangThai = TrangThaiSuKien.DaDuyet;
    }

    private async Task ChangeLifecycle(SuKien evt, PhienBanSuKien version, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        if (await db.DonHang.AnyAsync(d => db.SuatDien.Any(s => s.Id == d.SuatDienId && s.SuKienId == evt.Id) &&
            ((d.TrangThai == TrangThaiDonHang.ChoThanhToan && d.HanThanhToan > now) ||
             d.TrangThai == TrangThaiDonHang.DaThanhToan || d.TrangThai == TrangThaiDonHang.ChoHoanTien ||
             d.TrangThai == TrangThaiDonHang.DaThuTienHetCho), ct))
            throw Conflict("Sự kiện còn đơn cần xử lý. Hoàn tất phương án thông báo, đổi lịch hoặc hoàn tiền trước khi duyệt.");
        if (await db.Ve.AnyAsync(v => (v.TrangThai == TrangThaiVe.DaPhatHanh || v.TrangThai == TrangThaiVe.DaCheckIn) &&
            db.SuatDien.Any(s => s.Id == v.SuatDienId && s.SuKienId == evt.Id), ct))
            throw Conflict("Cần xử lý vé đã phát hành trước khi duyệt hoãn hoặc hủy.");
        var cancelled = version.LoaiYeuCau == LoaiYeuCauSuKien.Huy;
        var phase = cancelled ? TrangThaiSuatDien.DaHuy : TrangThaiSuatDien.Hoan;
        var slots = await db.SuatDien.Where(s => s.SuKienId == evt.Id && s.ThoiGianBatDau > now && s.TrangThai != TrangThaiSuatDien.Nhap).ToListAsync(ct);
        foreach (var venue in slots.Select(s => s.DiaDiemId).Distinct().Order())
            await SqlOperations.Lock(db, $"qlsk:venue:{venue}", ct);
        var ids = slots.Select(s => s.Id).ToArray();
        await db.LichDatDiaDiem.Where(l => l.SuatDienId.HasValue && ids.Contains(l.SuatDienId.Value))
            .ExecuteUpdateAsync(set => set.SetProperty(l => l.TrangThai, TrangThaiLichDatDiaDiem.DaHuy), ct);
        foreach (var slot in slots) slot.TrangThai = phase;
        evt.TrangThai = cancelled ? TrangThaiSuKien.DaHuy : TrangThaiSuKien.Hoan;
    }

    public async Task<EventWorkDto> Suspend(TaiKhoan actor, int id, DecisionRequest request, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, OperationsService.EventReviewPermission);
        var reason = Required(request.Reason, 1000, "lý do đình chỉ");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        var evt = await db.SuKien.SingleOrDefaultAsync(e => e.Id == id, ct) ?? throw Missing();
        if (await Owns(actor, id, ct)) throw Denied();
        if (!evt.PhienBanCongKhaiId.HasValue || evt.TrangThai is TrangThaiSuKien.DaHuy or TrangThaiSuKien.HoanTat or TrangThaiSuKien.DinhChi)
            throw Conflict("Sự kiện không ở trạng thái có thể đình chỉ.");
        evt.TrangThai = TrangThaiSuKien.DinhChi;
        SqlOperations.Audit(db, actor, "Đình chỉ sự kiện", nameof(SuKien), id, reason);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Detail(actor, id, ct);
    }

    private async Task<bool> CompletionReady(SuKien evt, DateTime now, CancellationToken ct)
    {
        if (!evt.PhienBanCongKhaiId.HasValue || evt.TrangThai is not (TrangThaiSuKien.DaDuyet or TrangThaiSuKien.DangMoBan)) return false;
        if (!await db.PhienBanSuKien.AnyAsync(v => v.Id == evt.PhienBanCongKhaiId && v.SuKienId == evt.Id && v.TrangThaiDuyet == TrangThaiDuyetPhienBan.DaDuyet, ct)) return false;
        if (await db.PhienBanSuKien.AnyAsync(v => v.SuKienId == evt.Id && (v.TrangThaiDuyet == TrangThaiDuyetPhienBan.Nhap || v.TrangThaiDuyet == TrangThaiDuyetPhienBan.ChoDuyet), ct)) return false;
        var sessions = db.SuatDien.Where(s => s.SuKienId == evt.Id && s.TrangThai != TrangThaiSuatDien.Nhap);
        if (!await sessions.AnyAsync(s => s.TrangThai != TrangThaiSuatDien.DaHuy, ct)) return false;
        if (await sessions.AnyAsync(s => s.TrangThai != TrangThaiSuatDien.DaHuy &&
            (s.ThoiGianKetThuc > now || s.TrangThai == TrangThaiSuatDien.Hoan), ct)) return false;
        if (await db.DonHang.AnyAsync(o => o.TrangThai == TrangThaiDonHang.ChoThanhToan && o.HanThanhToan > now &&
            db.SuatDien.Any(s => s.Id == o.SuatDienId && s.SuKienId == evt.Id), ct)) return false;
        return true;
    }

    public async Task<EventWorkDto> Complete(TaiKhoan actor, int id, DecisionRequest request, CancellationToken ct)
    {
        await NeedRead(actor, id, ct);
        var reason = Required(request.Reason, 1000, "ghi chú hoàn tất sự kiện");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        // Scheduling, version submission/approval, booking and reconciliation share this lock.
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        var evt = await db.SuKien.SingleOrDefaultAsync(e => e.Id == id, ct) ?? throw Missing();
        if (!await Owns(actor, id, ct) && !AccountAccess.Platform(actor, OperationsService.EventReviewPermission)) throw Denied();
        if (evt.TrangThai == TrangThaiSuKien.HoanTat) return await Detail(actor, id, ct);
        if (request.ExpectedState != State(evt.TrangThai)) throw Conflict("Trạng thái sự kiện đã thay đổi. Tải lại trước khi hoàn tất.");
        var slots = await db.SuatDien.Where(s => s.SuKienId == id).OrderBy(s => s.Id).ToListAsync(ct);
        foreach (var slot in slots) await SqlOperations.Lock(db, $"qlsk:session:{slot.Id}", ct);
        var now = DateTime.UtcNow;
        if (!await CompletionReady(evt, now, ct)) throw Conflict("Chỉ hoàn tất sự kiện công khai khi mọi suất đã kết thúc, không còn bản nháp/đang duyệt hoặc đơn giữ chỗ còn hiệu lực.");
        foreach (var slot in slots.Where(s => s.TrangThai is not (TrangThaiSuatDien.Nhap or TrangThaiSuatDien.DaHuy))) slot.TrangThai = TrangThaiSuatDien.HoanTat;
        await db.GiuGhe.Where(h => h.TrangThai == TrangThaiGiuGhe.DangGiu && h.HetHan <= now &&
            db.SuatDien.Any(s => s.Id == h.SuatDienId && s.SuKienId == id))
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.TrangThai, TrangThaiGiuGhe.HetHan), ct);
        await db.DonHang.Where(o => o.TrangThai == TrangThaiDonHang.ChoThanhToan && o.HanThanhToan <= now &&
            db.SuatDien.Any(s => s.Id == o.SuatDienId && s.SuKienId == id))
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.TrangThai, TrangThaiDonHang.HetHan), ct);
        evt.TrangThai = TrangThaiSuKien.HoanTat;
        SqlOperations.Audit(db, actor, "Hoàn tất sự kiện", nameof(SuKien), id, reason);
        var owner = await db.HoSoBTC.Where(o => o.Id == evt.BTCId).Select(o => o.TaiKhoanId).SingleAsync(ct);
        SqlOperations.Notify(db, owner, "Sự kiện đã hoàn tất", evt.TenSuKien + ". Kiểm tra các yêu cầu còn lại trước khi lập đối soát.", "event");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Detail(actor, id, ct);
    }

    private async Task<List<SuatDien>> DraftSessions(int eventId, int versionId, CancellationToken ct) =>
        (await db.SuatDien.Where(s => s.SuKienId == eventId && s.TrangThai == TrangThaiSuatDien.Nhap).ToListAsync(ct))
            .Where(s => ScheduleService.VersionOf(s) == versionId).ToList();

    internal static EventContent Content(PhienBanSuKien version) =>
        JsonSerializer.Deserialize<EventContent>(version.NoiDung, JsonSerializerOptions.Web) ?? throw Conflict("Thiếu nội dung phiên bản.");
    private static string TypeLabel(LoaiYeuCauSuKien type) => type switch {
        LoaiYeuCauSuKien.CapNhat => "Bản sửa", LoaiYeuCauSuKien.Hoan => "Hoãn", LoaiYeuCauSuKien.Huy => "Hủy", _ => "Tạo mới"
    };
    private static string State(TrangThaiSuKien state) => state switch {
        TrangThaiSuKien.Nhap => "DRAFT", TrangThaiSuKien.ChoDuyet => "PENDING", TrangThaiSuKien.DaDuyet => "APPROVED",
        TrangThaiSuKien.DangMoBan => "ACTIVE", TrangThaiSuKien.DinhChi => "SUSPENDED", TrangThaiSuKien.Hoan => "POSTPONED",
        TrangThaiSuKien.DaHuy => "CANCELLED", TrangThaiSuKien.HoanTat => "COMPLETED", _ => "REJECTED"
    };
    private static string VersionState(PhienBanSuKien v) => v.TrangThaiDuyet switch {
        TrangThaiDuyetPhienBan.Nhap => "DRAFT", TrangThaiDuyetPhienBan.DaDuyet => "APPROVED", TrangThaiDuyetPhienBan.TuChoi => "REJECTED",
        _ => v.LoaiYeuCau switch {
            LoaiYeuCauSuKien.Hoan => "POSTPONEMENT_REQUESTED", LoaiYeuCauSuKien.Huy => "CANCEL_REQUESTED",
            LoaiYeuCauSuKien.CapNhat => "PENDING_UPDATE", _ => "PENDING"
        }
    };
    private static string Label(string status) => status switch {
        "DRAFT" => "Bản nháp", "PENDING" => "Chờ duyệt", "APPROVED" => "Đã duyệt", "APPROVED_PRIVATE" => "Đã duyệt · Chưa công bố", "ACTIVE" => "Đang mở bán",
        "PENDING_UPDATE" => "Bản sửa chờ duyệt", "POSTPONEMENT_REQUESTED" => "Yêu cầu hoãn", "CANCEL_REQUESTED" => "Yêu cầu hủy",
        "SUSPENDED" => "Đã đình chỉ", "POSTPONED" => "Đã hoãn", "CANCELLED" => "Đã hủy", "COMPLETED" => "Đã kết thúc", _ => "Từ chối"
    };
    private static EventWorkDto Project(SuKien evt, string organizer, PhienBanSuKien? version, DateTime? next, bool review)
    {
        var code = review && version != null ? VersionState(version) :
            !evt.PhienBanCongKhaiId.HasValue && evt.TrangThai == TrangThaiSuKien.DaDuyet ? "APPROVED_PRIVATE" : State(evt.TrangThai);
        var content = version == null ? null : Content(version);
        return new(evt.Id, content?.Name ?? evt.TenSuKien, organizer, code, Label(code), Stamp(next), version?.Id,
            version == null ? null : TypeLabel(version.LoaiYeuCau), Stamp(version?.NgayGui), content?.Description ?? evt.MoTa);
    }
    private static VersionDto VersionRow(PhienBanSuKien v, bool owner)
    {
        var phase = VersionState(v);
        return new(v.Id, $"Phiên bản {v.SoPhienBan}", TypeLabel(v.LoaiYeuCau), phase, Label(phase), Stamp(v.NgayDuyet ?? v.NgayGui),
            v.KetQuaDuyet ?? v.LyDo, owner && phase == "DRAFT" ? ["submit"] : []);
    }
}
