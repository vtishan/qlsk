using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Accounts;
using QLSK.Features.Events;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Admissions;

public sealed class StaffGrantService(AppDbContext db, EventService events)
{
    internal static readonly string[] Operations = [
        "work.staff.list", "work.staff.detail", "work.staff.create", "work.staff.revoke"
    ];

    internal static readonly Dictionary<string, string> Permissions = new()
    {
        [AdmissionService.Scan] = "Quét QR",
        [AdmissionService.Manual] = "Check-in thủ công",
        [AdmissionService.View] = "Xem người tham dự"
    };

    public async Task<int> Give(TaiKhoan actor, int id, StaffGrantRequest request, CancellationToken ct)
    {
        await events.NeedOwner(actor, id, ct);
        var email = AccountService.Email(request.Email) ?? throw Invalid("Nhập email nhân sự đã xác minh.");
        var permission = Required(request.Permission, 64, "quyền");
        if (!Permissions.ContainsKey(permission)) throw Invalid("Chọn quyền check-in hoặc tra cứu đã hỗ trợ.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        if (!await db.SuKien.AnyAsync(e => e.Id == id && e.PhienBanCongKhaiId.HasValue &&
            (e.TrangThai == TrangThaiSuKien.DaDuyet || e.TrangThai == TrangThaiSuKien.DangMoBan), ct))
            throw Conflict("Chỉ cấp quyền vận hành cho sự kiện đã được duyệt.");
        if (request.SessionId.HasValue && !await db.SuatDien.AnyAsync(s => s.Id == request.SessionId &&
            s.SuKienId == id && s.TrangThai != TrangThaiSuatDien.Nhap, ct)) throw Invalid("Suất không thuộc phạm vi sự kiện.");
        var accountId = await db.TaiKhoan.Where(x => x.Email == email && x.EmailXacMinhLuc != null && x.TrangThai != TrangThaiTaiKhoan.DaKhoa)
            .Select(x => (int?)x.Id).SingleOrDefaultAsync(ct) ?? throw Invalid("Chỉ cấp quyền cho tài khoản có email đã xác minh.");
        if (actor.Id == accountId) throw Denied();
        await SqlOperations.Lock(db, $"qlsk:account:{accountId}", ct);
        var target = await db.TaiKhoan.SingleAsync(x => x.Id == accountId, ct);
        if (target.VaiTro is not (VaiTroTaiKhoan.NguoiDung or VaiTroTaiKhoan.QuanLySuKien) ||
            target.Email != email || target.EmailXacMinhLuc == null || target.TrangThai == TrangThaiTaiKhoan.DaKhoa) throw Denied();
        var active = await db.PhanCongSuKien.SingleOrDefaultAsync(g => g.TaiKhoanId == target.Id && g.SuKienId == id &&
            g.SuatDienId == request.SessionId && g.LoaiQuyen == permission && g.TrangThai == TrangThaiPhanCongSuKien.DangHieuLuc, ct);
        if (active != null) return active.Id;
        var grant = new PhanCongSuKien { TaiKhoanId = target.Id, SuKienId = id, SuatDienId = request.SessionId,
            LoaiQuyen = permission, NguoiPhanCongId = actor.Id, TrangThai = TrangThaiPhanCongSuKien.DangHieuLuc, NgayPhanCong = DateTime.UtcNow };
        target.VaiTro = VaiTroTaiKhoan.QuanLySuKien;
        db.PhanCongSuKien.Add(grant);
        await db.SaveChangesAsync(ct);
        SqlOperations.Audit(db, actor, "Cấp quyền sự kiện", nameof(PhanCongSuKien), grant.Id, permission);
        SqlOperations.Notify(db, target.Id, "Đã cấp quyền sự kiện", Permissions[permission], "staff");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return grant.Id;
    }

    public async Task Revoke(TaiKhoan actor, int id, int grantId, CancellationToken ct)
    {
        await events.NeedOwner(actor, id, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:event:{id}", ct);
        var grant = await db.PhanCongSuKien.SingleOrDefaultAsync(g => g.Id == grantId && g.SuKienId == id, ct) ?? throw Missing();
        if (grant.TrangThai == TrangThaiPhanCongSuKien.DaThuHoi) return;
        grant.TrangThai = TrangThaiPhanCongSuKien.DaThuHoi;
        grant.NgayThuHoi = DateTime.UtcNow;
        SqlOperations.Audit(db, actor, "Thu hồi quyền sự kiện", nameof(PhanCongSuKien), grantId);
        await db.SaveChangesAsync(ct);
        if (grant.TaiKhoanId is { } staffId)
        {
            await SqlOperations.Lock(db, $"qlsk:account:{staffId}", ct);
            if (!await db.PhanCongSuKien.AnyAsync(g => g.TaiKhoanId == staffId && g.TrangThai == TrangThaiPhanCongSuKien.DangHieuLuc, ct))
                await db.TaiKhoan.Where(a => a.Id == staffId && a.VaiTro == VaiTroTaiKhoan.QuanLySuKien)
                    .ExecuteUpdateAsync(set => set.SetProperty(a => a.VaiTro, VaiTroTaiKhoan.NguoiDung), ct);
        }
        await tx.CommitAsync(ct);
    }

    private sealed class GrantRow
    {
        public required PhanCongSuKien Grant { get; init; }
        public string? Name { get; init; }
        public SuatDien? Slot { get; init; }
    }
    private IQueryable<GrantRow> Rows(int id) => from grant in db.PhanCongSuKien.AsNoTracking()
        where grant.SuKienId == id
        select new GrantRow { Grant = grant, Name = db.TaiKhoan.Where(a => a.Id == grant.TaiKhoanId).Select(a => a.HoTen).FirstOrDefault(),
            Slot = db.SuatDien.FirstOrDefault(s => s.Id == grant.SuatDienId) };

    public async Task<PageDto<StaffGrantDto>> List(TaiKhoan actor, int id, string? status, string? cursor, CancellationToken ct)
    {
        await events.NeedOwner(actor, id, ct);
        var after = After(cursor);
        const int size = 30;
        var query = Rows(id).Where(row => row.Grant.Id > after);
        if (status != null)
        {
            var phase = status switch {
                "ACTIVE" => TrangThaiPhanCongSuKien.DangHieuLuc, "PENDING" => TrangThaiPhanCongSuKien.ChoNhan,
                "REVOKED" => TrangThaiPhanCongSuKien.DaThuHoi, _ => throw Invalid("Trạng thái phân công không hợp lệ.")
            };
            query = query.Where(g => g.Grant.TrangThai == phase);
        }
        var rows = await query.OrderBy(r => r.Grant.Id).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(Map).ToArray(), size, r => r.Id);
    }

    public async Task<StaffGrantDto> Detail(TaiKhoan actor, int id, int grantId, CancellationToken ct)
    {
        await events.NeedOwner(actor, id, ct);
        return Map(await Rows(id).SingleOrDefaultAsync(g => g.Grant.Id == grantId, ct) ?? throw Missing());
    }

    private static StaffGrantDto Map(GrantRow row)
    {
        var grant = row.Grant;
        var status = grant.TrangThai switch {
            TrangThaiPhanCongSuKien.DangHieuLuc => ("ACTIVE", "Đang cấp"),
            TrangThaiPhanCongSuKien.ChoNhan => ("PENDING", "Chờ nhận"),
            TrangThaiPhanCongSuKien.DaThuHoi => ("REVOKED", "Đã thu hồi"),
            TrangThaiPhanCongSuKien.HetHan => ("EXPIRED", "Hết hạn"), _ => ("REJECTED", "Từ chối")
        };
        return new(grant.Id, row.Name ?? grant.EmailMoi ?? "Nhân sự", row.Slot == null ? "Toàn sự kiện" : ScheduleService.NameOf(row.Slot),
            Permissions.GetValueOrDefault(grant.LoaiQuyen, grant.LoaiQuyen), Stamp(grant.NgayPhanCong), status.Item1, status.Item2,
            status.Item1 is "ACTIVE" or "PENDING" ? ["revoke"] : []);
    }
}
