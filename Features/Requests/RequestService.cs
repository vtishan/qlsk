using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Accounts;
using QLSK.Features.Catalog;
using QLSK.Features.Finance;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Requests;

public sealed class RequestService(AppDbContext db, CatalogService catalog, IDataProtectionProvider protection, RentalPayOs payos)
{
    internal const string RentalPermission = "rentals.review";
    internal const string SupportPermission = "support.review";
    internal static readonly string[] UserOperations = [
        "requests.rentals.list", "requests.rentals.detail", "requests.rentals.create", "requests.rentals.cancel",
        "requests.support.list", "requests.support.detail", "requests.support.create"];
    internal static readonly string[] RentalOperations = ["work.rentals.list", "work.rentals.detail", "work.rentals.decide"];
    internal static readonly string[] SupportOperations = ["work.support.list", "work.support.detail", "work.support.resolve"];
    private IDataProtector RentalProtector => protection.CreateProtector("QLSK.Rental.Content.v1");

    public async Task<PageDto<RentalDto>> Rentals(TaiKhoan actor, bool review, string? status, string? cursor, CancellationToken ct)
    {
        if (review) AccountAccess.NeedPlatform(actor, RentalPermission);
        var after = After(cursor);
        var now = DateTime.UtcNow;
        const int size = 30;
        var rows = db.YeuCauThue.AsNoTracking().Where(r => r.Id > after);
        if (!review) rows = rows.Where(r => r.TaiKhoanId == actor.Id);
        if (status == "EXPIRED")
            rows = rows.Where(r => r.TrangThai == TrangThaiYeuCauThue.HetHan ||
                r.TrangThai == TrangThaiYeuCauThue.ChoCoc && r.HanDatCoc <= now);
        else if (status != null)
        {
            var phase = Enum.GetValues<TrangThaiYeuCauThue>().SingleOrDefault(p => RentalState(p).Code == status);
            if (phase == default) throw Invalid("Trạng thái yêu cầu thuê không hợp lệ.");
            rows = rows.Where(r => r.TrangThai == phase);
            if (phase == TrangThaiYeuCauThue.ChoCoc) rows = rows.Where(r => r.HanDatCoc > now);
        }
        var page = await (from r in rows join v in db.DiaDiem on r.DiaDiemId equals v.Id orderby r.Id
                          select new { Rental = r, v.TenDiaDiem }).Take(size + 1).ToListAsync(ct);
        return Page(page.Select(r => RentalSummary(r.Rental, r.TenDiaDiem, now)).ToArray(), size, r => r.Id);
    }

    public async Task<RentalDto> Rental(TaiKhoan actor, int id, CancellationToken ct)
    {
        var canReview = AccountAccess.Platform(actor, RentalPermission);
        var review = canReview || AccountAccess.Platform(actor, FinanceService.Permission);
        var row = await (from r in db.YeuCauThue.AsNoTracking() join v in db.DiaDiem on r.DiaDiemId equals v.Id
                         where r.Id == id && (review || r.TaiKhoanId == actor.Id)
                         select new { Rental = r, v.TenDiaDiem }).SingleOrDefaultAsync(ct) ?? throw Missing();
        var data = RentalContent(row.Rental);
        var now = DateTime.UtcNow;
        var status = Effective(row.Rental, now);
        var actions = new List<string>();
        if (payos.Configured && row.Rental.TaiKhoanId == actor.Id && status == TrangThaiYeuCauThue.ChoCoc) actions.Add("deposit");
        if (payos.Configured && (row.Rental.TaiKhoanId == actor.Id || AccountAccess.Platform(actor, FinanceService.Permission)) &&
            await db.GiaoDichThanhToan.AnyAsync(p => p.YeuCauThueId == id && p.PhuongThuc == PhuongThucThanhToan.PayOS, ct)) actions.Add("reconcileDeposit");
        if (canReview && row.Rental.TaiKhoanId != actor.Id && status == TrangThaiYeuCauThue.ChoDuyet) actions.Add("decide");
        if (row.Rental.TaiKhoanId == actor.Id && status is TrangThaiYeuCauThue.ChoDuyet or TrangThaiYeuCauThue.ChoCoc)
        {
            if (!await HasPayment(id, ct)) actions.Add("cancel");
        }
        return RentalSummary(row.Rental, row.TenDiaDiem, now) with {
            Description = data?.Reason, Resolution = row.Rental.LyDoXuLy, AllowedActions = actions,
            History = await History(nameof(YeuCauThue), id, ct),
            Fields = data == null ? [] : [
                new("Sự kiện dự kiến", data.EventName), new("Loại hình", data.EventType), new("Số người", data.Capacity.ToString()),
                new("Kịch bản", data.Program), new("Thiết bị thêm", data.Equipment), new("Lưu ý", data.SpecialNotes),
                new("Người phụ trách", data.Representative), new("Số liên hệ", data.Phone), new("Mục đích", data.Reason)]
        };
    }

    public async Task<int> CreateRental(TaiKhoan actor, RentalCreateRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var prefix = request.RequestKey?.ToString("N") + ":";
        if (request.RequestKey.HasValue)
        {
            await SqlOperations.Lock(db, $"qlsk:rental:{actor.Id}:{request.RequestKey:N}", ct);
            var previous = await db.YeuCauThue.AsNoTracking().FirstOrDefaultAsync(r =>
                r.TaiKhoanId == actor.Id && r.NoiDungYeuCau != null && r.NoiDungYeuCau.StartsWith(prefix), ct);
            if (previous != null)
            {
                if (RentalContent(previous) != NormalizeRental(request))
                    throw Conflict("Mã gửi lại thuộc một hồ sơ thuê khác.");
                return previous.Id;
            }
        }
        await SqlOperations.Lock(db, $"qlsk:venue:{request.VenueId}", ct);
        var now = DateTime.UtcNow;
        var from = request.From.UtcDateTime;
        var to = request.To.UtcDateTime;
        if (from <= now || to <= from || to - from > TimeSpan.FromDays(31))
            throw Invalid("Chọn khung giờ thuê hợp lệ trong tương lai.");
        var place = await db.DiaDiem.AsNoTracking().SingleOrDefaultAsync(v => v.Id == request.VenueId && v.TrangThai == TrangThaiDiaDiem.HoatDong, ct)
            ?? throw Missing();
        if (request.Capacity < 1 || request.Capacity > place.SucChua) throw Invalid("Số người dự kiến vượt sức chứa địa điểm.");
        var policy = await catalog.PublishedPolicies("venue-rental").OrderByDescending(p => p.NgayHieuLuc).ThenByDescending(p => p.PhienBan)
            .Select(p => (int?)p.Id).FirstOrDefaultAsync(ct);
        if (policy != request.PolicyId) throw Conflict("Điều khoản thuê đã thay đổi. Mở lại biểu mẫu.");
        if (await catalog.Busy(now).AnyAsync(l => l.DiaDiemId == place.Id && l.BatDau < to && l.KetThuc > from, ct))
            throw Conflict("Địa điểm đã có lịch trong khung giờ này.");
        var content = NormalizeRental(request);
        var protectedContent = RentalProtector.Protect(JsonSerializer.Serialize(content));
        var rental = new YeuCauThue {
            TaiKhoanId = actor.Id, DiaDiemId = place.Id, ThoiGianBatDau = from, ThoiGianKetThuc = to, ChinhSachId = policy.Value,
            TongTien = place.GiaThue, TienCoc = decimal.Round(place.GiaThue * 0.3m, 0, MidpointRounding.AwayFromZero),
            TrangThai = TrangThaiYeuCauThue.ChoDuyet, NgayTao = now,
            NoiDungYeuCau = request.RequestKey.HasValue ? prefix + protectedContent : protectedContent
        };
        db.YeuCauThue.Add(rental);
        await db.SaveChangesAsync(ct);
        SqlOperations.Audit(db, actor, "Đã gửi", nameof(YeuCauThue), rental.Id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return rental.Id;
    }

    private static RentalCreateRequest NormalizeRental(RentalCreateRequest request) => request with {
            EventName = Required(request.EventName, 180, "tên sự kiện"), EventType = Required(request.EventType, 60, "loại hình"),
            Reason = Required(request.Reason, 500, "mục đích thuê"), Program = Required(request.Program, 1500, "kịch bản"),
            Equipment = Optional(request.Equipment, 700, "thiết bị"), SpecialNotes = Optional(request.SpecialNotes, 700, "lưu ý"),
            Representative = Required(request.Representative, 120, "người phụ trách"),
            Phone = AccountService.Phone(request.Phone) ?? throw Invalid("Nhập số liên hệ.")
        };

    public async Task<RentalDto> DecideRental(TaiKhoan actor, int id, DecisionRequest request, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, RentalPermission);
        if (request.Action is not ("approve" or "reject")) throw Invalid("Quyết định thuê không hợp lệ.");
        var reason = Required(request.Reason, 1000, "lý do quyết định");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var placeId = await db.YeuCauThue.Where(r => r.Id == id).Select(r => (int?)r.DiaDiemId).SingleOrDefaultAsync(ct) ?? throw Missing();
        await SqlOperations.Lock(db, $"qlsk:venue:{placeId}", ct);
        var rental = await db.YeuCauThue.SingleAsync(r => r.Id == id, ct);
        var now = DateTime.UtcNow;
        if (rental.TaiKhoanId == actor.Id) throw Denied();
        if (rental.TrangThai != TrangThaiYeuCauThue.ChoDuyet || request.ExpectedState != "REQUESTED")
            throw Conflict("Yêu cầu thuê đã thay đổi. Mở lại chi tiết.");
        if (request.Action == "approve")
        {
            if (rental.ThoiGianBatDau <= now ||
                !await db.DiaDiem.AnyAsync(v => v.Id == placeId && v.TrangThai == TrangThaiDiaDiem.HoatDong, ct) ||
                await catalog.Busy(now).AnyAsync(l => l.DiaDiemId == placeId && l.BatDau < rental.ThoiGianKetThuc && l.KetThuc > rental.ThoiGianBatDau, ct))
                throw Conflict("Địa điểm hoặc khung giờ thuê không còn khả dụng.");
            var confirmed = rental.TienCoc == 0;
            rental.HanDatCoc = confirmed ? null : now.AddHours(24);
            rental.TrangThai = confirmed ? TrangThaiYeuCauThue.DaXacNhan : TrangThaiYeuCauThue.ChoCoc;
            db.LichDatDiaDiem.Add(new LichDatDiaDiem {
                DiaDiemId = placeId, YeuCauThueId = id, BatDau = rental.ThoiGianBatDau, KetThuc = rental.ThoiGianKetThuc,
                LoaiLich = LoaiLichDiaDiem.Thue, TrangThai = confirmed ? TrangThaiLichDatDiaDiem.DaXacNhan : TrangThaiLichDatDiaDiem.DangGiu
            });
        }
        else rental.TrangThai = TrangThaiYeuCauThue.TuChoi;
        rental.NguoiDuyetId = actor.Id;
        rental.LyDoXuLy = reason;
        var label = RentalState(rental.TrangThai).Label;
        SqlOperations.Audit(db, actor, label, nameof(YeuCauThue), id, reason);
        SqlOperations.Notify(db, rental.TaiKhoanId, "Cập nhật yêu cầu thuê", label, "rental");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Rental(actor, id, ct);
    }

    public async Task<RentalDto> CancelRental(TaiKhoan actor, int id, CancelRentalRequest request, CancellationToken ct)
    {
        var reason = Required(request.Reason, 500, "lý do hủy");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var place = await db.YeuCauThue.Where(r => r.Id == id && r.TaiKhoanId == actor.Id)
            .Select(r => (int?)r.DiaDiemId).SingleOrDefaultAsync(ct) ?? throw Missing();
        await SqlOperations.Lock(db, $"qlsk:venue:{place}", ct);
        var rental = await db.YeuCauThue.SingleAsync(r => r.Id == id, ct);
        if (Effective(rental, DateTime.UtcNow) is not (TrangThaiYeuCauThue.ChoDuyet or TrangThaiYeuCauThue.ChoCoc) || await HasPayment(id, ct))
            throw Conflict("Yêu cầu này cần bộ phận hỗ trợ kiểm tra trước khi hủy.");
        rental.NgayHuy = DateTime.UtcNow;
        rental.TrangThai = TrangThaiYeuCauThue.DaHuy;
        rental.LyDoXuLy = reason;
        await db.LichDatDiaDiem.Where(l => l.YeuCauThueId == id && l.TrangThai == TrangThaiLichDatDiaDiem.DangGiu)
            .ExecuteUpdateAsync(set => set.SetProperty(l => l.TrangThai, TrangThaiLichDatDiaDiem.DaHuy), ct);
        SqlOperations.Audit(db, actor, "Đã hủy", nameof(YeuCauThue), id, reason);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Rental(actor, id, ct);
    }

    private Task<bool> HasPayment(int id, CancellationToken ct) =>
        db.GiaoDichThanhToan.AnyAsync(p => p.YeuCauThueId == id &&
            (p.TrangThai == TrangThaiGiaoDichThanhToan.ThanhCong || p.TrangThai == TrangThaiGiaoDichThanhToan.ChoThanhToan), ct);

    public async Task<RentalCheckoutDto> Deposit(TaiKhoan actor, int id, CancellationToken ct)
    {
        if (!payos.Configured) throw new ApiException(503, "Chưa cấu hình PayOS cho tiền cọc.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var venue = await db.YeuCauThue.Where(r => r.Id == id && r.TaiKhoanId == actor.Id)
            .Select(r => (int?)r.DiaDiemId).SingleOrDefaultAsync(ct) ?? throw Missing();
        await SqlOperations.Lock(db, $"qlsk:venue:{venue}", ct);
        var rental = await db.YeuCauThue.SingleAsync(r => r.Id == id, ct);
        if (Effective(rental, DateTime.UtcNow) != TrangThaiYeuCauThue.ChoCoc || rental.HanDatCoc == null || rental.TienCoc <= 0)
            throw Conflict("Yêu cầu không còn trong thời hạn đặt cọc.");
        if (!await db.LichDatDiaDiem.AnyAsync(l => l.YeuCauThueId == id && l.TrangThai == TrangThaiLichDatDiaDiem.DangGiu, ct))
            throw Conflict("Lịch giữ không còn khả dụng.");
        var payment = await db.GiaoDichThanhToan.Where(p => p.YeuCauThueId == id && p.PhuongThuc == PhuongThucThanhToan.PayOS)
            .OrderByDescending(p => p.Id).FirstOrDefaultAsync(ct);
        var existing = payment != null;
        if (payment?.TrangThai == TrangThaiGiaoDichThanhToan.ThanhCong) throw Conflict("Tiền cọc đã được xác nhận.");
        if (payment == null)
        {
            payment = new GiaoDichThanhToan { YeuCauThueId = id, SoTienYeuCau = rental.TienCoc,
                MaDonNhaCungCap = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000)).ToString(System.Globalization.CultureInfo.InvariantCulture),
                PhuongThuc = PhuongThucThanhToan.PayOS, TrangThai = TrangThaiGiaoDichThanhToan.ChoThanhToan };
            db.GiaoDichThanhToan.Add(payment);
            await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);
        // Persist the provider order code before sending; retries use the same payment intent.
        var checkout = payment.ChungTu;
        if (checkout == null)
        {
            var code = long.Parse(payment.MaDonNhaCungCap!);
            checkout = existing ? await payos.ExistingCheckout(code, payment.SoTienYeuCau, ct) : null;
            checkout ??= await payos.Checkout(code, payment.SoTienYeuCau, rental.HanDatCoc.Value, ct);
            payment.ChungTu = Required(checkout, 500, "liên kết thanh toán");
            await db.SaveChangesAsync(ct);
        }
        return new(payment.Id, checkout);
    }

    public Task<RentalDto> ReconcileDeposit(TaiKhoan actor, int id, CancellationToken ct) => ReconcileDepositCore(actor, id, null, ct);

    public async Task DepositWebhook(JsonElement payload, CancellationToken ct)
    {
        var verified = payos.VerifyWebhook(payload);
        var code = verified.Code.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var id = await db.GiaoDichThanhToan.Where(p => p.MaDonNhaCungCap == code && p.PhuongThuc == PhuongThucThanhToan.PayOS)
            .Select(p => p.YeuCauThueId).SingleOrDefaultAsync(ct) ?? throw Missing();
        await ReconcileDepositCore(null, id, verified.EventId, ct);
    }

    private async Task<RentalDto> ReconcileDepositCore(TaiKhoan? actor, int id, string? webhookId, CancellationToken ct)
    {
        if (actor != null)
        {
            var authorized = await db.YeuCauThue.AnyAsync(r => r.Id == id && r.TaiKhoanId == actor.Id, ct);
            if (!authorized && !AccountAccess.Platform(actor, FinanceService.Permission)) throw Missing();
        }
        else if (webhookId == null) throw Denied();
        var intent = await db.GiaoDichThanhToan.AsNoTracking().Where(p => p.YeuCauThueId == id && p.PhuongThuc == PhuongThucThanhToan.PayOS)
            .OrderByDescending(p => p.Id).FirstOrDefaultAsync(ct) ?? throw Missing();
        var actual = await payos.Status(long.Parse(intent.MaDonNhaCungCap!), ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var venue = await db.YeuCauThue.Where(r => r.Id == id).Select(r => r.DiaDiemId).SingleAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:venue:{venue}", ct);
        var rental = await db.YeuCauThue.SingleAsync(r => r.Id == id, ct);
        var payment = await db.GiaoDichThanhToan.SingleAsync(p => p.Id == intent.Id, ct);
        if (webhookId != null && !await db.WebhookThanhToan.AnyAsync(w => w.MaSuKienWebhook == webhookId, ct))
            db.WebhookThanhToan.Add(new WebhookThanhToan { GiaoDichId = payment.Id, MaSuKienWebhook = webhookId,
                NoiDungDaLoc = JsonSerializer.Serialize(new { orderCode = intent.MaDonNhaCungCap, paymentId = payment.Id }),
                ThoiGianNhan = DateTime.UtcNow, TrangThaiXuLy = TrangThaiXuLyWebhook.DaXuLy });
        if (payment.TrangThai == TrangThaiGiaoDichThanhToan.ThanhCong ||
            payment.TrangThai == TrangThaiGiaoDichThanhToan.NghiVan && payment.MaGiaoDich == actual.Reference)
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return actor == null ? RentalSummary(rental, "", DateTime.UtcNow) : await Rental(actor, id, ct);
        }
        if (actual.Status != "PAID") throw Conflict("PayOS chưa xác nhận đã thu đủ tiền cọc. Vui lòng kiểm tra lại sau.");
        if (actual.Paid <= 0 || string.IsNullOrWhiteSpace(actual.Reference) || actual.Reference.Length > 128 || actual.PaidAt == null)
            throw Conflict("Khoản thu PayOS cần bộ phận tài chính đối soát số tiền và chứng từ.");
        payment.SoTienThucThu = actual.Paid;
        payment.MaGiaoDich = actual.Reference;
        payment.NgayThanhToan = actual.PaidAt;
        var now = DateTime.UtcNow;
        var valid = actual.Amount == payment.SoTienYeuCau && actual.Paid == payment.SoTienYeuCau &&
            Effective(rental, now) == TrangThaiYeuCauThue.ChoCoc && actual.PaidAt <= now.AddMinutes(5) && actual.PaidAt <= rental.HanDatCoc && rental.ThoiGianBatDau > now;
        var calendar = await db.LichDatDiaDiem.SingleOrDefaultAsync(l => l.YeuCauThueId == id && l.TrangThai == TrangThaiLichDatDiaDiem.DangGiu, ct);
        valid = valid && calendar != null;
        if (valid)
        {
            payment.TrangThai = TrangThaiGiaoDichThanhToan.ThanhCong;
            rental.TrangThai = TrangThaiYeuCauThue.DaXacNhan;
            calendar!.TrangThai = TrangThaiLichDatDiaDiem.DaXacNhan;
            SqlOperations.Notify(db, rental.TaiKhoanId, "Đã xác nhận thuê địa điểm", "Tiền cọc đã được PayOS đối soát.", "rental");
        }
        else
        {
            payment.TrangThai = TrangThaiGiaoDichThanhToan.NghiVan;
            SqlOperations.Notify(db, rental.TaiKhoanId, "Cọc thuê cần hỗ trợ", "Khoản thu đến sau thời hạn hoặc lịch giữ không còn. Liên hệ hỗ trợ để xử lý khoản tiền đã thu.", "rental");
        }
        db.NhatKyHeThong.Add(new NhatKyHeThong { TaiKhoanId = actor?.Id, HanhDong = valid ? "Đối soát cọc PayOS" : "Cọc PayOS cần xử lý",
            DoiTuong = nameof(YeuCauThue), DoiTuongId = id, ThoiGian = now,
            DuLieuThayDoi = JsonSerializer.Serialize(new { note = "Đối soát PayOS", paymentId = payment.Id, amount = actual.Paid, reference = actual.Reference }) });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return actor == null ? RentalSummary(rental, "", now) : await Rental(actor, id, ct);
    }

    private RentalCreateRequest? RentalContent(YeuCauThue row) => row.NoiDungYeuCau == null ? null :
        JsonSerializer.Deserialize<RentalCreateRequest>(RentalProtector.Unprotect(row.NoiDungYeuCau[(row.NoiDungYeuCau.IndexOf(':') + 1)..]));

    private RentalDto RentalSummary(YeuCauThue row, string name, DateTime now)
    {
        var status = RentalState(Effective(row, now));
        return new(row.Id, "Thuê " + name, name, Stamp(row.NgayTao), Stamp(row.ThoiGianBatDau), Stamp(row.ThoiGianKetThuc),
            row.TongTien, row.TienCoc, Stamp(row.HanDatCoc), status.Code, status.Label, row.LyDoXuLy);
    }

    private static TrangThaiYeuCauThue Effective(YeuCauThue row, DateTime now) =>
        Effective(row.TrangThai, row.HanDatCoc, now);

    internal static TrangThaiYeuCauThue Effective(TrangThaiYeuCauThue status, DateTime? depositDue, DateTime now) =>
        status == TrangThaiYeuCauThue.ChoCoc && depositDue <= now ? TrangThaiYeuCauThue.HetHan : status;

    internal static (string Code, string Label) RentalState(TrangThaiYeuCauThue status) => status switch
    {
        TrangThaiYeuCauThue.ChoCoc => ("AWAITING_DEPOSIT", "Chờ đặt cọc"),
        TrangThaiYeuCauThue.DaXacNhan => ("CONFIRMED", "Đã xác nhận"),
        TrangThaiYeuCauThue.TuChoi => ("REJECTED", "Từ chối"),
        TrangThaiYeuCauThue.HetHan => ("EXPIRED", "Quá hạn cọc"),
        TrangThaiYeuCauThue.DaHuy => ("CANCELLED", "Đã hủy"),
        _ => ("REQUESTED", "Chờ duyệt")
    };

    public async Task<PageDto<SupportDto>> Cases(TaiKhoan actor, bool review, string? status, string? cursor, CancellationToken ct)
    {
        if (review) AccountAccess.NeedPlatform(actor, SupportPermission);
        var after = After(cursor);
        const int size = 30;
        var rows = db.KhieuNai.AsNoTracking().Where(r => r.Id > after);
        if (!review) rows = rows.Where(r => r.TaiKhoanId == actor.Id);
        if (status != null)
        {
            var phase = Enum.GetValues<TrangThaiKhieuNai>().SingleOrDefault(p => SupportState(p).Code == status);
            if (phase == default) throw Invalid("Trạng thái hỗ trợ không hợp lệ.");
            rows = rows.Where(r => r.TrangThai == phase);
        }
        var page = await rows.OrderBy(r => r.Id).Take(size + 1).ToListAsync(ct);
        return Page(page.Select(CaseSummary).ToArray(), size, r => r.Id);
    }

    public async Task<SupportDto> Case(TaiKhoan actor, int id, CancellationToken ct)
    {
        var review = AccountAccess.Platform(actor, SupportPermission);
        var item = await db.KhieuNai.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id && (r.TaiKhoanId == actor.Id || review), ct) ?? throw Missing();
        var canRespond = review && actor.Id != item.TaiKhoanId && item.TrangThai is TrangThaiKhieuNai.ChoXuLy or TrangThaiKhieuNai.DangXuLy;
        return CaseSummary(item) with { Description = item.NoiDung, Resolution = item.KetQuaXuLy, Response = item.KetQuaXuLy,
            AllowedActions = canRespond ? ["resolve"] : [], History = await History(nameof(KhieuNai), id, ct) };
    }

    public async Task<int> CreateCase(TaiKhoan actor, SupportCreateRequest request, CancellationToken ct)
    {
        int? eventId = request.EventId;
        if (request.TicketId is { } ticket)
        {
            var ownedEvent = await (from v in db.Ve join s in db.SuatDien on v.SuatDienId equals s.Id
                where v.Id == ticket && (db.VeMoi.Any(m => m.VeId == v.Id && m.TaiKhoanNhanId == actor.Id) ||
                    db.ChiTietDonHang.Any(l => l.Id == v.ChiTietDonHangId && db.DonHang.Any(d => d.Id == l.DonHangId && d.TaiKhoanId == actor.Id)))
                select (int?)s.SuKienId).SingleOrDefaultAsync(ct) ?? throw Missing();
            if (eventId.HasValue && eventId.Value != ownedEvent) throw Invalid("Vé không thuộc sự kiện đã chọn.");
            eventId = ownedEvent;
        }
        else if (eventId.HasValue && !await catalog.Published().AnyAsync(e => e.Id == eventId.Value, ct))
            throw Missing();
        if (request.RentalId.HasValue && !await db.YeuCauThue.AnyAsync(r => r.Id == request.RentalId.Value && r.TaiKhoanId == actor.Id, ct))
            throw Missing();
        var item = new KhieuNai { TaiKhoanId = actor.Id, VeId = request.TicketId, SuKienId = eventId, YeuCauThueId = request.RentalId,
            TieuDe = Required(request.Subject, 160, "chủ đề"), NoiDung = Required(request.Description, 1200, "nội dung"),
            TrangThai = TrangThaiKhieuNai.ChoXuLy, NgayTao = DateTime.UtcNow };
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.KhieuNai.Add(item);
        await db.SaveChangesAsync(ct);
        SqlOperations.Audit(db, actor, "Đã mở", nameof(KhieuNai), item.Id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return item.Id;
    }

    public async Task<SupportDto> UpdateCase(TaiKhoan actor, int id, DecisionRequest request, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, SupportPermission);
        var next = request.Action switch {
            "respond" => TrangThaiKhieuNai.DangXuLy, "resolve" => TrangThaiKhieuNai.DaGiaiQuyet,
            "close" => TrangThaiKhieuNai.TuChoi, _ => throw Invalid("Chọn quyết định hỗ trợ hợp lệ.")
        };
        var note = Required(request.Reason, 1000, "nội dung xử lý");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:support:{id}", ct);
        var item = await db.KhieuNai.SingleOrDefaultAsync(r => r.Id == id, ct) ?? throw Missing();
        if (item.TaiKhoanId == actor.Id) throw Denied();
        if (item.TrangThai is not (TrangThaiKhieuNai.ChoXuLy or TrangThaiKhieuNai.DangXuLy) || SupportState(item.TrangThai).Code != request.ExpectedState)
            throw Conflict("Yêu cầu hỗ trợ đã thay đổi. Mở lại chi tiết.");
        item.TrangThai = next;
        item.NguoiXuLyId = actor.Id;
        item.KetQuaXuLy = note;
        var label = SupportState(next).Label;
        SqlOperations.Audit(db, actor, label, nameof(KhieuNai), id, note);
        SqlOperations.Notify(db, item.TaiKhoanId, "Cập nhật hỗ trợ", label, "support");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Case(actor, id, ct);
    }

    private static (string Code, string Label) SupportState(TrangThaiKhieuNai status) => status switch {
        TrangThaiKhieuNai.DangXuLy => ("IN_PROGRESS", "Đang xử lý"), TrangThaiKhieuNai.DaGiaiQuyet => ("RESOLVED", "Đã giải quyết"),
        TrangThaiKhieuNai.TuChoi => ("CLOSED", "Đã đóng"), _ => ("OPEN", "Đã mở")
    };

    private static SupportDto CaseSummary(KhieuNai item)
    {
        var status = SupportState(item.TrangThai);
        var context = item.VeId.HasValue ? $"Vé #{item.VeId}" : item.SuKienId.HasValue ? $"Sự kiện #{item.SuKienId}" :
            item.YeuCauThueId.HasValue ? $"Yêu cầu thuê #{item.YeuCauThueId}" : "Hỗ trợ chung";
        return new(item.Id, item.TieuDe, Stamp(item.NgayTao), status.Code, status.Label, item.KetQuaXuLy, ContextLabel: context);
    }

    internal async Task<IReadOnlyList<HistoryDto>> History(string kind, int id, CancellationToken ct)
    {
        var rows = await db.NhatKyHeThong.AsNoTracking().Where(a => a.DoiTuong == kind && a.DoiTuongId == id)
            .OrderBy(a => a.ThoiGian).ThenBy(a => a.Id).Take(200).ToListAsync(ct);
        return rows.Select(a => new HistoryDto(Stamp(a.ThoiGian), null, a.HanhDong, null, SqlOperations.AuditNote(a.DuLieuThayDoi))).ToArray();
    }
}
