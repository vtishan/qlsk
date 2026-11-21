using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Catalog;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Accounts;

public sealed class AccountService(AppDbContext db, CatalogService catalog, IConfiguration config, ILogger<AccountService> logger)
{
    internal static readonly string[] AccountOperations = [
        "auth.logout", "account.profile", "account.updateProfile", "account.changePassword",
        "account.notifications", "account.readNotification", "catalog.review",
        "account.favorites", "account.addFavorite", "account.removeFavorite",
        "account.viewHistory", "account.recordView"
    ];
    private static readonly PasswordHasher<TaiKhoan> Hasher = new();
    private static readonly TaiKhoan Unknown = new();
    private static readonly string UnknownHash = Hasher.HashPassword(Unknown, Convert.ToHexString(RandomNumberGenerator.GetBytes(24)));

    internal bool MailConfigured => !string.IsNullOrWhiteSpace(config["QLSK:Smtp:Host"]) && Email(config["QLSK:Smtp:From"], false) is not null;

    public async Task<int> Register(RegisterRequest request, CancellationToken ct)
    {
        var latest = await catalog.PublishedPolicies("registration").OrderByDescending(p => p.NgayHieuLuc)
            .ThenByDescending(p => p.PhienBan).Select(p => (int?)p.Id).FirstOrDefaultAsync(ct);
        if (!latest.HasValue || latest.Value != request.AcceptedPolicyId)
            throw Conflict("Điều khoản sử dụng đã thay đổi. Mở lại biểu mẫu đăng ký.");
        var account = NewAccount(request.Name, request.UserName, request.Email, request.Phone, request.Password);
        db.TaiKhoan.Add(account);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.SaveChangesAsync(ct);
        SqlOperations.Audit(db, account, "ChapNhanDieuKhoan", nameof(ChinhSach), latest,
            System.Text.Json.JsonSerializer.Serialize(new { policyId = latest }));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return account.Id;
    }

    internal static TaiKhoan NewAccount(string? name, string? username, string? email, string? phone, string? password)
    {
        var emailAddress = Email(email);
        var phoneNumber = Phone(phone);
        if (emailAddress is null && phoneNumber is null) throw Invalid("Nhập email hoặc số điện thoại.");
        var account = new TaiKhoan
        {
            HoTen = Required(name, 100, "họ tên"), TenDangNhap = Required(username, 50, "tên đăng nhập").ToLowerInvariant(),
            Email = emailAddress, SoDienThoai = phoneNumber, TrangThai = TrangThaiTaiKhoan.HoatDong,
            VaiTro = VaiTroTaiKhoan.NguoiDung, NgayTao = DateTime.UtcNow
        };
        if (!account.TenDangNhap.All(c => char.IsLetterOrDigit(c) || c is '_' or '.' or '-'))
            throw Invalid("Tên đăng nhập chỉ gồm chữ, số, dấu chấm, gạch ngang và gạch dưới.");
        account.MatKhauBam = Hasher.HashPassword(account, Password(password));
        return account;
    }

    public async Task<TaiKhoan> Login(LoginRequest request, CancellationToken ct)
    {
        var login = Required(request.Identifier, 255, "địa chỉ đăng nhập");
        var email = Email(login, false);
        var phone = Phone(login, false);
        var password = request.Password;
        if (string.IsNullOrEmpty(password) || password.Length > 128) throw SignIn();
        var account = await db.TaiKhoan.AsNoTracking().FirstOrDefaultAsync(t =>
            (email != null && t.Email == email) || (phone != null && t.SoDienThoai == phone), ct);
        var result = Hasher.VerifyHashedPassword(account ?? Unknown, account?.MatKhauBam ?? UnknownHash, password);
        if (account is null || account.TrangThai == TrangThaiTaiKhoan.DaKhoa || result == PasswordVerificationResult.Failed)
            throw new ApiException(401, "Thông tin đăng nhập không đúng.");
        return account;
    }

    public async Task RevokeSessions(int id, CancellationToken ct) =>
        await db.TaiKhoan.Where(t => t.Id == id).ExecuteUpdateAsync(set => set
            .SetProperty(t => t.PhienBanDangNhap, t => t.PhienBanDangNhap + 1), ct);

    public static ProfileDto Profile(TaiKhoan account) =>
        new(account.Id, account.HoTen, account.TenDangNhap, account.Email, account.SoDienThoai, AccountAccess.RoleLabel(account.VaiTro),
            account.EmailXacMinhLuc.HasValue || account.SoDienThoaiXacMinhLuc.HasValue ? "Đã xác minh liên hệ" : "Chưa xác minh liên hệ");

    public async Task<ProfileDto> Update(int id, ProfileRequest request, CancellationToken ct)
    {
        var email = Email(request.Email);
        var phone = Phone(request.Phone);
        if (email is null && phone is null) throw Invalid("Giữ ít nhất một kênh liên hệ.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:account:{id}", ct);
        var account = await db.TaiKhoan.SingleAsync(t => t.Id == id, ct);
        account.HoTen = Required(request.Name, 100, "họ tên");
        if (account.Email != email || account.SoDienThoai != phone)
            ClearToken(account);
        if (account.Email != email) account.EmailXacMinhLuc = null;
        if (account.SoDienThoai != phone) account.SoDienThoaiXacMinhLuc = null;
        account.Email = email;
        account.SoDienThoai = phone;
        SqlOperations.Audit(db, account, "CapNhatHoSo", nameof(TaiKhoan), id);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Profile(account);
    }

    public async Task<TaiKhoan> ChangePassword(int id, ChangePasswordRequest request, CancellationToken ct)
    {
        var password = Password(request.NewPassword);
        var current = request.CurrentPassword;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:account:{id}", ct);
        var account = await db.TaiKhoan.SingleAsync(t => t.Id == id, ct);
        if (string.IsNullOrEmpty(current) || current.Length > 128 ||
            Hasher.VerifyHashedPassword(account, account.MatKhauBam, current) == PasswordVerificationResult.Failed)
            throw Invalid("Mật khẩu hiện tại không đúng.");
        SavePassword(account, password);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return account;
    }

    public async Task Reset(ResetRequest request, CancellationToken ct)
    {
        if (!MailConfigured) throw new ApiException(503, "Chưa cấu hình gửi email khôi phục.");
        var login = Required(request.Identifier, 255, "địa chỉ liên hệ");
        var email = Email(login, false);
        var phone = Phone(login, false);
        var accountId = await db.TaiKhoan.AsNoTracking().Where(t => t.TrangThai != TrangThaiTaiKhoan.DaKhoa && t.EmailXacMinhLuc != null &&
            ((email != null && t.Email == email) || (phone != null && t.SoDienThoai == phone && t.SoDienThoaiXacMinhLuc != null)))
            .Select(t => (int?)t.Id).FirstOrDefaultAsync(ct);
        if (!accountId.HasValue) return;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var to = await IssueToken(accountId.Value, "reset", token, true, ct);
        if (to != null)
            await Mail(to, "QLSK · Khôi phục mật khẩu", "Nhập mã sau trong màn hình khôi phục QLSK. Mã dùng một lần trong 30 phút.\n\n" + token, ct);
    }

    public async Task ConfirmReset(ConfirmResetRequest request, CancellationToken ct)
    {
        var password = Password(request.NewPassword);
        var hash = TokenHash("reset", Required(request.Token, 128, "mã khôi phục").ToUpperInvariant());
        var now = DateTime.UtcNow;
        var id = await db.TaiKhoan.Where(t => t.MaBamKhoiPhuc == hash && t.HanKhoiPhuc > now)
            .Select(t => (int?)t.Id).FirstOrDefaultAsync(ct);
        if (!id.HasValue) throw Invalid("Mã khôi phục không hợp lệ hoặc đã hết hạn.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:account:{id.Value}", ct);
        var account = await db.TaiKhoan.SingleAsync(t => t.Id == id.Value, ct);
        if (account.MaBamKhoiPhuc != hash || account.HanKhoiPhuc <= DateTime.UtcNow || account.TrangThai == TrangThaiTaiKhoan.DaKhoa)
            throw Invalid("Mã khôi phục không hợp lệ hoặc đã hết hạn.");
        SavePassword(account, password);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task RequestVerification(int id, VerificationRequest request, CancellationToken ct)
    {
        if (!MailConfigured || request.Channel != "email") throw Invalid("Kênh xác minh email chưa khả dụng.");
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        var address = await IssueToken(id, "verify", token, false, ct) ?? throw Invalid("Thêm email vào hồ sơ trước khi xác minh.");
        await Mail(address, "QLSK · Xác minh email", "Mã xác minh email của bạn có hiệu lực 30 phút.\n\n" + token, ct);
    }

    public async Task ConfirmVerification(int id, ConfirmVerificationRequest request, CancellationToken ct)
    {
        var hash = TokenHash("verify", Required(request.Code, 16, "mã xác minh").ToUpperInvariant());
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:account:{id}", ct);
        var account = await db.TaiKhoan.SingleAsync(t => t.Id == id, ct);
        if (account.Email == null || account.MaBamKhoiPhuc != hash || account.HanKhoiPhuc <= DateTime.UtcNow)
            throw Invalid("Mã xác minh không hợp lệ hoặc đã hết hạn.");
        account.EmailXacMinhLuc = DateTime.UtcNow;
        if (account.TrangThai == TrangThaiTaiKhoan.ChoXacMinh) account.TrangThai = TrangThaiTaiKhoan.HoatDong;
        ClearToken(account);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task<string?> IssueToken(int id, string purpose, string token, bool verified, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:account:{id}", ct);
        var account = await db.TaiKhoan.SingleAsync(t => t.Id == id, ct);
        if (account.Email == null || (verified && !account.EmailXacMinhLuc.HasValue) || account.TrangThai == TrangThaiTaiKhoan.DaKhoa)
            return null;
        account.MaBamKhoiPhuc = TokenHash(purpose, token);
        account.HanKhoiPhuc = DateTime.UtcNow.AddMinutes(30);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return account.Email;
    }

    private async Task Mail(string to, string subject, string body, CancellationToken ct)
    {
        var options = config.GetSection("QLSK:Smtp");
        try
        {
            using var client = new SmtpClient(options["Host"]!, options.GetValue("Port", 587)) { EnableSsl = true, Timeout = 10000 };
            if (!string.IsNullOrWhiteSpace(options["Username"]))
                client.Credentials = new NetworkCredential(options["Username"], options["Password"]);
            using var message = new MailMessage(options["From"]!, to, subject, body);
            await client.SendMailAsync(message, ct);
        }
        catch (SmtpException error) { logger.LogWarning("Gửi thư QLSK thất bại với mã {StatusCode}.", error.StatusCode); }
    }

    public async Task<PageDto<NotificationDto>> Notifications(int id, string? cursor, CancellationToken ct)
    {
        const int size = 40;
        var after = After(cursor);
        var rows = await db.ThongBao.AsNoTracking().Where(n => n.Id > after && n.TaiKhoanId == id && n.KenhGui == KenhGuiThongBao.TrongUngDung)
            .OrderBy(n => n.Id).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(n => new NotificationDto(n.Id, n.TieuDe, n.NoiDung, Stamp(n.NgayTao), n.DaDoc)).ToArray(), size, n => n.Id);
    }

    public async Task ReadNotification(int accountId, int id, CancellationToken ct)
    {
        var count = await db.ThongBao.Where(n => n.Id == id && n.TaiKhoanId == accountId && n.KenhGui == KenhGuiThongBao.TrongUngDung)
            .ExecuteUpdateAsync(set => set.SetProperty(n => n.DaDoc, true), ct);
        if (count == 0) throw Missing();
    }

    public async Task<PageDto<EventCardDto>> Favorites(int id, string? cursor, CancellationToken ct)
    {
        const int size = 40;
        var after = After(cursor);
        var now = DateTime.UtcNow;
        var saved = await db.TaiKhoan.Where(t => t.Id == id).Select(t => t.SuKienYeuThich).SingleAsync(ct) ?? [];
        var rows = await (from evt in catalog.Published()
                          join category in db.DanhMucSuKien on evt.DanhMucId equals category.Id
                          where saved.Contains(evt.Id) && evt.Id > after orderby evt.Id
                          select new { evt.Id, evt.TenSuKien, evt.AnhBia, category.TenDanhMuc,
                              Next = db.SuatDien.Where(s => s.SuKienId == evt.Id && s.TrangThai != TrangThaiSuatDien.Nhap &&
                                  s.ThoiGianBatDau >= now).Min(s => (DateTime?)s.ThoiGianBatDau) }).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(e => new EventCardDto(e.Id, e.TenSuKien, e.TenDanhMuc, CatalogService.SafeImage(e.AnhBia),
            Stamp(e.Next), null, null)).ToArray(), size, e => e.Id);
    }

    public async Task SetFavorite(int id, int eventId, bool add, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:favorites:{id}", ct);
        if (add && !await catalog.Published().AnyAsync(e => e.Id == eventId, ct)) throw Missing();
        var account = await db.TaiKhoan.SingleAsync(t => t.Id == id, ct);
        var saved = (account.SuKienYeuThich ?? []).ToHashSet();
        if (add ? saved.Add(eventId) : saved.Remove(eventId))
        {
            account.SuKienYeuThich = saved.Order().ToArray();
            await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);
    }

    public async Task<PageDto<EventCardDto>> ViewHistory(int id, CancellationToken ct)
    {
        var saved = await db.TaiKhoan.AsNoTracking().Where(t => t.Id == id)
            .Select(t => t.SuKienDaXem).SingleAsync(ct) ?? [];
        var ordered = saved.Distinct().Take(20).ToArray();
        var now = DateTime.UtcNow;
        var rows = await (from evt in catalog.Published()
                          join category in db.DanhMucSuKien on evt.DanhMucId equals category.Id
                          where ordered.Contains(evt.Id)
                          select new { evt.Id, evt.TenSuKien, evt.AnhBia, category.TenDanhMuc,
                              Next = db.SuatDien.Where(s => s.SuKienId == evt.Id && s.TrangThai != TrangThaiSuatDien.Nhap &&
                                  s.ThoiGianBatDau >= now).Min(s => (DateTime?)s.ThoiGianBatDau) }).ToListAsync(ct);
        var byId = rows.ToDictionary(e => e.Id);
        return new PageDto<EventCardDto>(ordered.Where(byId.ContainsKey).Select(eventId =>
        {
            var e = byId[eventId];
            return new EventCardDto(e.Id, e.TenSuKien, e.TenDanhMuc, CatalogService.SafeImage(e.AnhBia),
                Stamp(e.Next), null, null);
        }).ToArray());
    }

    public async Task RecordView(int id, int eventId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:view-history:{id}", ct);
        if (!await catalog.Published().AnyAsync(e => e.Id == eventId, ct)) throw Missing();
        // Authentication can already have tracked this account. Project the current database
        // value after acquiring the lock and update only this column, never the actor snapshot.
        var saved = await db.TaiKhoan.AsNoTracking().Where(t => t.Id == id)
            .Select(t => t.SuKienDaXem).SingleAsync(ct) ?? [];
        var recent = new[] { eventId }.Concat(saved).Distinct().Take(20).ToArray();
        if (!saved.SequenceEqual(recent))
            await db.TaiKhoan.Where(t => t.Id == id)
                .ExecuteUpdateAsync(set => set.SetProperty(t => t.SuKienDaXem, recent), ct);
        await tx.CommitAsync(ct);
    }

    private static string TokenHash(string type, string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(type + ":" + token)));

    private static void ClearToken(TaiKhoan account)
    {
        account.MaBamKhoiPhuc = null;
        account.HanKhoiPhuc = null;
    }

    private static void SavePassword(TaiKhoan account, string password)
    {
        account.MatKhauBam = Hasher.HashPassword(account, password);
        account.PhienBanDangNhap++;
        ClearToken(account);
    }

    private static string Password(string? value) => value is { Length: >= 8 and <= 128 } && !string.IsNullOrWhiteSpace(value)
        ? value : throw Invalid("Mật khẩu phải có từ 8 đến 128 ký tự.");

    internal static string? Email(string? value, bool strict = true)
    {
        var email = value?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(email)) return null;
        if (email.Length <= 255 && MailAddress.TryCreate(email, out var address) && address.Address == email && address.Host.Contains('.'))
            return email;
        return strict ? throw Invalid("Địa chỉ email không hợp lệ.") : null;
    }

    internal static string? Phone(string? value, bool strict = true)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var source = value.Trim();
        var text = new string(source.Where(c => char.IsAsciiDigit(c) || c == '+').ToArray());
        if (text.StartsWith("+84")) text = "0" + text[3..];
        if (text.StartsWith('+')) text = text[1..];
        if (source.All(c => char.IsAsciiDigit(c) || " +().-".Contains(c)) && source.Count(c => c == '+') <= 1 &&
            (!source.Contains('+') || source.StartsWith('+')) && text.Length is >= 8 and <= 15 && text.All(char.IsAsciiDigit))
            return text;
        return strict ? throw Invalid("Số điện thoại không hợp lệ.") : null;
    }
}
