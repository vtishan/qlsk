using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using QLSK.Common.Data;
using QLSK.Common.Http;
using QLSK.Common.Security;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.Accounts;
using static QLSK.Common.Http.ApiRequest;

namespace QLSK.Features.Organizers;

public sealed class OrganizerService(AppDbContext db, IDataProtectionProvider dataProtection,
    IWebHostEnvironment environment, IConfiguration configuration)
{
    internal const string ReviewPermission = "organizer.review";
    internal static readonly string[] UserOperations = ["organizer.applications", "organizer.application", "organizer.submit"];
    internal static readonly string[] ReviewOperations = [
        "work.applications.list", "work.applications.detail", "work.applications.decide", "work.applications.document"
    ];

    public async Task<PageDto<ApplicationDto>> List(TaiKhoan actor, bool review, string? status, bool overdue, string? cursor, CancellationToken ct)
    {
        if (review) AccountAccess.NeedPlatform(actor, ReviewPermission);
        var after = After(cursor);
        const int size = 30;
        var query = db.HoSoBTC.AsNoTracking().Where(h => h.Id > after);
        if (!review) query = query.Where(h => h.TaiKhoanId == actor.Id);
        if (status != null)
        {
            var phase = Enum.GetValues<TrangThaiHoSoToChuc>().SingleOrDefault(phase => Status(phase).Code == status);
            if (phase == default) throw Invalid("Trạng thái hồ sơ không hợp lệ.");
            query = query.Where(h => h.TrangThai == phase);
        }
        if (overdue)
        {
            var now = DateTime.UtcNow;
            query = query.Where(h => h.TrangThai == TrangThaiHoSoToChuc.ChoDuyet && h.HanDuyet < now);
        }
        var rows = await query.OrderBy(h => h.Id).Take(size + 1).ToListAsync(ct);
        return Page(rows.Select(Summary).ToArray(), size, h => h.Id);
    }

    public async Task<int> Apply(TaiKhoan actor, OrganizerSubmission request, CancellationToken ct)
    {
        if (actor.VaiTro is not (VaiTroTaiKhoan.NguoiDung or VaiTroTaiKhoan.BanToChuc)) throw Denied();
        var holder = ReadHolder(request);
        var protectedData = dataProtection.CreateProtector("QLSK.Organizer.Holder.v1")
            .Protect(JsonSerializer.Serialize(holder.Private));
        var folder = Storage();
        Directory.CreateDirectory(folder);
        var stored = new List<string>();
        var protector = dataProtection.CreateProtector("QLSK.Organizer.Document.v1");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlOperations.Lock(db, $"qlsk:organizer:{actor.Id}", ct);
        if (await db.HoSoBTC.AnyAsync(h => h.TaiKhoanId == actor.Id &&
            (h.TrangThai == TrangThaiHoSoToChuc.ChoDuyet || h.TrangThai == TrangThaiHoSoToChuc.DaDuyet), ct))
            throw Conflict("Tài khoản đã có hồ sơ đang xét duyệt hoặc đã được chấp thuận.");

        var now = DateTime.UtcNow;
        var profile = new HoSoBTC {
            TaiKhoanId = actor.Id, TenToChuc = holder.Name, NguoiDaiDien = holder.Representative, MaSoThue = holder.Tax,
            DiaChi = holder.Address, ThongTinLienHe = holder.Contact, ThongTinNhanTienMaHoa = protectedData,
            TrangThai = TrangThaiHoSoToChuc.ChoDuyet, NgayNop = now, HanDuyet = await Deadline(now, ct)
        };
        try
        {
            db.HoSoBTC.Add(profile);
            await db.SaveChangesAsync(ct);
            foreach (var (slot, file) in holder.Files)
            {
                var (name, mime, content) = await ReadDocument(file, ct);
                var key = Guid.NewGuid().ToString("N");
                var path = Path.Combine(folder, key + ".enc");
                stored.Add(path);
                await File.WriteAllBytesAsync(path, protector.Protect(content), ct);
                db.GiayToBTC.Add(new GiayToBTC {
                    HoSoBTCId = profile.Id, LoaiGiayTo = slot, KhoaLuuTru = key,
                    TenTep = name, LoaiNoiDung = mime, NgayTaiLen = now
                });
            }
            SqlOperations.Audit(db, actor, "NopHoSo", nameof(HoSoBTC), profile.Id);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return profile.Id;
        }
        catch
        {
            foreach (var path in stored)
            {
                try { File.Delete(path); } catch (IOException) { }
            }
            throw;
        }
    }

    public async Task<ApplicationDto> Detail(TaiKhoan actor, int id, CancellationToken ct)
    {
        var profile = await Accessible(actor, id, ct);
        var rows = await (from audit in db.NhatKyHeThong.AsNoTracking() join staff in db.TaiKhoan on audit.TaiKhoanId equals staff.Id
                          where audit.DoiTuong == nameof(HoSoBTC) && audit.DoiTuongId == id
                          orderby audit.ThoiGian, audit.Id
                          select new { audit.HanhDong, audit.ThoiGian, audit.DuLieuThayDoi, staff.HoTen }).ToListAsync(ct);
        var files = await db.GiayToBTC.AsNoTracking().Where(f => f.HoSoBTCId == id).OrderBy(f => f.Id)
            .Select(f => new ApplicationDocumentDto(f.Id, f.LoaiGiayTo, f.TenTep)).ToListAsync(ct);
        var allowed = AccountAccess.Platform(actor, ReviewPermission) && profile.TrangThai == TrangThaiHoSoToChuc.ChoDuyet &&
            actor.Id != profile.TaiKhoanId;
        return Summary(profile) with {
            History = rows.Select(h => new HistoryDto(Stamp(h.ThoiGian), null, h.HanhDong switch {
                "NopHoSo" => "Nộp hồ sơ", "approve" => "Đã duyệt", "reject" => "Từ chối", "needs_info" => "Yêu cầu bổ sung",
                _ => h.HanhDong
            }, h.HoTen, SqlOperations.AuditNote(h.DuLieuThayDoi))).ToArray(),
            Documents = files,
            AllowedActions = allowed ? ["decide"] : [],
            Fields = [new("Người đại diện", profile.NguoiDaiDien), new("Địa chỉ", profile.DiaChi), new("Mã số thuế", profile.MaSoThue),
                new("Thông tin liên hệ", profile.ThongTinLienHe), new("Lý do xử lý", profile.LyDoTuChoi)]
        };
    }

    public async Task<PrivateDocumentDto> Document(TaiKhoan actor, int id, int documentId, CancellationToken ct)
    {
        await Accessible(actor, id, ct);
        var doc = await db.GiayToBTC.AsNoTracking().SingleOrDefaultAsync(f => f.Id == documentId && f.HoSoBTCId == id, ct) ?? throw Missing();
        if (!Guid.TryParseExact(doc.KhoaLuuTru, "N", out var key)) throw Missing();
        var path = Path.Combine(Storage(), key.ToString("N") + ".enc");
        if (!File.Exists(path)) throw Missing();
        var cipher = await File.ReadAllBytesAsync(path, ct);
        var content = dataProtection.CreateProtector("QLSK.Organizer.Document.v1").Unprotect(cipher);
        return new(content, doc.LoaiNoiDung, doc.TenTep);
    }

    public async Task<ApplicationDto> Decide(TaiKhoan actor, int id, DecisionRequest request, CancellationToken ct)
    {
        AccountAccess.NeedPlatform(actor, ReviewPermission);
        var next = request.Action switch
        {
            "approve" => TrangThaiHoSoToChuc.DaDuyet,
            "reject" => TrangThaiHoSoToChuc.TuChoi,
            "needs_info" => TrangThaiHoSoToChuc.CanBoSung,
            _ => throw Invalid("Quyết định hồ sơ không hợp lệ.")
        };
        var reason = Required(request.Reason, 1000, "lý do xét duyệt");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var owner = await db.HoSoBTC.Where(h => h.Id == id).Select(h => (int?)h.TaiKhoanId).SingleOrDefaultAsync(ct) ?? throw Missing();
        if (actor.Id == owner) throw Denied();
        await SqlOperations.Lock(db, $"qlsk:organizer:{owner}", ct);
        var profile = await db.HoSoBTC.SingleAsync(h => h.Id == id, ct);
        if (profile.TrangThai != TrangThaiHoSoToChuc.ChoDuyet || Status(profile.TrangThai).Code != request.ExpectedState)
            throw Conflict("Hồ sơ đã được xử lý. Mở lại chi tiết trước khi xác nhận.");
        if (next == TrangThaiHoSoToChuc.DaDuyet)
        {
            await SqlOperations.Lock(db, $"qlsk:account:{owner}", ct);
            var applicant = await db.TaiKhoan.SingleAsync(a => a.Id == owner, ct);
            if (applicant.VaiTro is not (VaiTroTaiKhoan.NguoiDung or VaiTroTaiKhoan.BanToChuc) || applicant.TrangThai == TrangThaiTaiKhoan.DaKhoa)
                throw Conflict("Tài khoản người nộp không đủ điều kiện nhận quyền Ban tổ chức.");
            applicant.VaiTro = VaiTroTaiKhoan.BanToChuc;
        }
        profile.TrangThai = next;
        profile.NgayDuyet = DateTime.UtcNow;
        profile.NguoiDuyetId = actor.Id;
        profile.LyDoTuChoi = next == TrangThaiHoSoToChuc.DaDuyet ? null : reason;
        SqlOperations.Audit(db, actor, request.Action!, nameof(HoSoBTC), id, reason);
        SqlOperations.Notify(db, owner, "Cập nhật hồ sơ Ban tổ chức", Status(next).Label, "organizer");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Summary(profile);
    }

    private async Task<HoSoBTC> Accessible(TaiKhoan actor, int id, CancellationToken ct)
    {
        var review = AccountAccess.Platform(actor, ReviewPermission);
        return await db.HoSoBTC.AsNoTracking().SingleOrDefaultAsync(h => h.Id == id && (review || h.TaiKhoanId == actor.Id), ct) ?? throw Missing();
    }

    internal static (string Code, string Label) Status(TrangThaiHoSoToChuc phase) => phase switch
    {
        TrangThaiHoSoToChuc.ChoDuyet => ("PENDING", "Chờ duyệt"),
        TrangThaiHoSoToChuc.DaDuyet => ("APPROVED", "Đã duyệt"),
        TrangThaiHoSoToChuc.CanBoSung => ("NEEDS_INFO", "Yêu cầu bổ sung"),
        TrangThaiHoSoToChuc.TuChoi => ("REJECTED", "Từ chối"),
        _ => ("DRAFT", "Bản nháp")
    };

    private static ApplicationDto Summary(HoSoBTC profile)
    {
        var status = Status(profile.TrangThai);
        return new(profile.Id, profile.TenToChuc, Stamp(profile.NgayNop), Stamp(profile.HanDuyet), status.Code, status.Label, profile.LyDoTuChoi);
    }

    private async Task<DateTime> Deadline(DateTime now, CancellationToken ct)
    {
        var configured = await db.CauHinhHeThong.Where(c => c.Khoa == "review.holidays").Select(c => c.GiaTri).FirstOrDefaultAsync(ct);
        var holidays = configured == null ? [] : JsonSerializer.Deserialize<DateOnly[]>(configured) ?? [];
        var next = now.AddHours(7);
        for (int day = 0; day < 7;)
        {
            next = next.AddDays(1);
            if (next.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && !holidays.Contains(DateOnly.FromDateTime(next))) day++;
        }
        return next.AddHours(-7);
    }

    internal string Storage()
    {
        var folder = Path.GetFullPath(configuration["QLSK:PrivateFiles"] ?? Path.Combine(environment.ContentRootPath, "App_Data", "organizer"),
            Path.GetFullPath(environment.ContentRootPath));
        var relation = Path.GetRelativePath(environment.WebRootPath, folder);
        if (relation != ".." && !relation.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) && !Path.IsPathRooted(relation))
            throw new InvalidOperationException("Kho giấy tờ cần nằm ngoài thư mục tĩnh.");
        return folder;
    }

    private sealed record Holder(string Name, string Representative, string? Tax, string Address, string Contact,
        object Private, List<(string Slot, IFormFile File)> Files);

    private static Holder ReadHolder(OrganizerSubmission r)
    {
        if (r.Agreement is not ("on" or "yes" or "true")) throw Invalid("Cần xác nhận cam kết trước khi gửi hồ sơ.");
        var address = Required(r.ContactAddress, 300, "địa chỉ liên hệ");
        var email = AccountService.Email(r.Email) ?? throw Invalid("Cần email liên hệ.");
        var phone = AccountService.Phone(r.Phone) ?? throw Invalid("Cần số điện thoại liên hệ.");
        if (email.Length + phone.Length > 296) throw Invalid("Thông tin liên hệ quá dài.");
        var website = Optional(r.Website, 300, "website");
        if (website != null && (!Uri.TryCreate(website, UriKind.Absolute, out var site) || (site.Scheme is not ("https" or "http"))))
            throw Invalid("Website phải có địa chỉ HTTP hoặc HTTPS.");
        var bankName = Required(r.BankName, 150, "ngân hàng");
        var bankAccount = Required(r.BankAccount, 34, "số tài khoản ngân hàng");
        if (!bankAccount.All(char.IsAsciiDigit)) throw Invalid("Số tài khoản ngân hàng chỉ gồm chữ số.");
        var bankOwner = Required(r.BankOwner, 180, "chủ tài khoản ngân hàng");
        string name, representative;
        string? tax;
        object legal;
        var files = new List<(string, IFormFile)>();
        void Add(string slot, IFormFile? file) => files.Add((slot, file ?? throw Invalid($"Thiếu giấy tờ {slot}.")));

        if (r.RegistrationType == "individual")
        {
            name = representative = Required(r.FullName, 120, "họ tên pháp lý");
            tax = Optional(r.PersonalTax, 20, "mã số thuế");
            var number = Required(r.IdentityNumber, 12, "số giấy tờ");
            var place = Required(r.IssuedPlace, 180, "nơi cấp");
            if (!r.IssuedDate.HasValue || r.IssuedDate > VietnamDate(DateTime.UtcNow))
                throw Invalid("Ngày cấp giấy tờ không hợp lệ.");
            if (r.IdentityType == "card" && Regex.IsMatch(number, @"^\d{12}$"))
            {
                Add("CCCD mặt trước", r.IdentityFront);
                Add("CCCD mặt sau", r.IdentityBack);
            }
            else if (r.IdentityType == "passport" && Regex.IsMatch(number, @"^(?=.*[0-9])[A-Za-z0-9]{6,12}$"))
                Add("Hộ chiếu", r.Passport);
            else throw Invalid("Loại và số giấy tờ định danh không hợp lệ.");
            legal = new { r.RegistrationType, r.IdentityType, number, r.IssuedDate, place };
        }
        else if (r.RegistrationType == "organization")
        {
            name = Required(r.OrganizationName, 200, "tên tổ chức");
            representative = Required(r.Representative, 120, "người đại diện");
            tax = Required(r.OrganizationTax, 20, "mã số thuế");
            if (!Regex.IsMatch(tax, @"^[0-9]{10}(-?[0-9]{3})?$")) throw Invalid("Mã số thuế tổ chức không hợp lệ.");
            legal = new { r.RegistrationType, Position = Required(r.Position, 100, "chức vụ"),
                HeadOffice = Required(r.HeadOffice, 300, "địa chỉ trụ sở") };
            Add("Đăng ký doanh nghiệp", r.BusinessLicense);
            if (r.Authorization != null) Add("Giấy ủy quyền", r.Authorization);
        }
        else throw Invalid("Chọn loại hình đăng ký.");
        if (Normalize(bankOwner) != Normalize(name)) throw Invalid("Tên chủ tài khoản phải khớp tên đăng ký pháp lý.");
        return new(name, representative, tax, address, email + " · " + phone, new {
            bankName, bankAccount, bankOwner, Branch = Optional(r.BankBranch, 150, "chi nhánh"), legal, website
        }, files);
    }

    private static string Normalize(string value) => Regex.Replace(new string(value.Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray())
        .Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant(), @"\s+", " ");

    private static async Task<(string Name, string Mime, byte[] Content)> ReadDocument(IFormFile file, CancellationToken ct)
    {
        var name = Path.GetFileName(file.FileName.Replace('\\', '/'));
        if (file.Length is < 1 or > 5 * 1024 * 1024 || name.Length is < 1 or > 255 || name.Any(char.IsControl))
            throw Invalid("Giấy tờ cần có nội dung và tối đa 5 MB mỗi tệp.");
        using var buffer = new MemoryStream((int)file.Length);
        await file.CopyToAsync(buffer, ct);
        var bytes = buffer.ToArray();
        var mime = Path.GetExtension(name).ToLowerInvariant() switch
        {
            ".pdf" when bytes.AsSpan().StartsWith("%PDF-"u8) => "application/pdf",
            ".png" when bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) => "image/png",
            ".jpg" or ".jpeg" when bytes.AsSpan().StartsWith(new byte[] { 255, 216, 255 }) => "image/jpeg",
            _ => throw Invalid("Nội dung giấy tờ không khớp định dạng PDF, PNG hoặc JPG.")
        };
        return (name, mime, bytes);
    }
}
