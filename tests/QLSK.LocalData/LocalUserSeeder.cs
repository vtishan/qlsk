using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.LocalData;

public sealed record UserSeedResult(int AddedUsers, int AddedOrders, int AddedPayments, int AddedTickets, int SkippedUsers);

public static class LocalUserSeeder
{
    public const int UserCount = 100;
    public const string EmailDomain = "sample.qlsk.test";

    private static readonly string[] Surnames =
        ["Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng"];
    private static readonly string[] GivenNames =
        ["Minh Anh", "Thanh Bình", "Ngọc Chi", "Quốc Dũng", "Thu Hà", "Gia Huy", "Khánh Linh", "Bảo Ngọc", "Đức Phúc", "Phương Thảo"];

    public static async Task<UserSeedResult> SeedAsync(ApplicationDbContext db, IPasswordHasher<TaiKhoan> hasher,
        string password, DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
            throw new InvalidOperationException("The sample user password must contain at least 12 characters.");
        var now = utcNow ?? DateTime.UtcNow;
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : null;

        var emails = Enumerable.Range(1, UserCount).Select(EmailFor).ToArray();
        var existing = (await db.TaiKhoans.AsNoTracking().Where(x => emails.Contains(x.Email))
            .Select(x => x.Email).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = Enumerable.Range(1, UserCount).Where(number => !existing.Contains(EmailFor(number))).ToArray();
        if (missing.Length == 0) return new UserSeedResult(0, 0, 0, 0, UserCount);
        if (!await db.VaiTros.AnyAsync(x => x.Id == 1))
            throw new InvalidOperationException("Run the schema migrations before populating sample users.");

        // Use existing events only, and keep all inventory changes in the same transaction as the orders.
        var ticketTypes = await db.LoaiVes.Include(x => x.SuKien)
            .Where(x => x.SuKien.TrangThai == TrangThaiSuKien.DaDuyet &&
                x.SuKien.BatDau > now.AddMinutes(15) && x.Gia >= 2 && x.Gia == decimal.Truncate(x.Gia) &&
                x.MoBan <= now.AddMinutes(-30) && x.DongBan > now.AddMinutes(15))
            .OrderBy(x => x.Id).ToListAsync();
        if (ticketTypes.Count == 0)
            throw new InvalidOperationException("Sample users need an approved upcoming event with paid tickets already on sale for at least 30 minutes.");

        // Plan the entire batch before adding records, so insufficient inventory cannot leave a partial batch.
        var plannedInventory = ticketTypes.ToDictionary(x => x.Id, x => x.SoLuong - x.DaBan - x.DangGiu);
        var plans = new List<(int Number, LoaiVe Type, int Quantity, TrangThaiDon State)>();
        foreach (var number in missing)
        {
            var index = number - 1;
            var quantity = 1 + index % 3;
            var type = Enumerable.Range(0, ticketTypes.Count).Select(offset => ticketTypes[(index + offset) % ticketTypes.Count])
                .FirstOrDefault(x => plannedInventory[x.Id] >= quantity)
                ?? throw new InvalidOperationException("There are not enough available tickets for the sample user orders.");
            var state = StateFor(index);
            if (state is TrangThaiDon.ChoThanhToan or TrangThaiDon.ThanhCong or TrangThaiDon.YeuCauHoanTien)
                plannedInventory[type.Id] -= quantity;
            plans.Add((number, type, quantity, state));
        }

        var addedTickets = 0;
        foreach (var (number, type, quantity, state) in plans)
        {
            var index = number - 1;
            var user = new TaiKhoan
            {
                HoTen = $"{Surnames[index / GivenNames.Length]} {GivenNames[index % GivenNames.Length]}",
                Email = EmailFor(number), VaiTroId = 1, HoatDong = number % 11 != 0,
                NgayTao = now.AddDays(-60 - number).AddMinutes(-number)
            };
            user.MatKhau = hasher.HashPassword(user, password);
            var saleMinutes = (int)Math.Min(14 * 24 * 60, (now - type.MoBan).TotalMinutes);
            var created = state == TrangThaiDon.ChoThanhToan
                ? now.AddMinutes(-1 - index % 5)
                : now.AddMinutes(-30 - index * 17 % Math.Max(1, saleMinutes - 29));
            var total = type.Gia * quantity;
            var order = new DonVe
            {
                TaiKhoan = user, SuKienId = type.SuKienId, SoLuong = quantity, TongTien = total,
                TrangThai = state, NgayTao = created, HetHan = created.AddMinutes(15)
            };
            order.ChiTiets.Add(new ChiTietDonVe
            {
                LoaiVe = type, SoLuong = quantity, DonGia = type.Gia, ThanhTien = total
            });

            var paid = state is TrangThaiDon.ThanhCong or TrangThaiDon.YeuCauHoanTien or TrangThaiDon.DaHoanTien;
            var payment = new ThanhToan
            {
                SoTien = total, NgayTao = created.AddSeconds(10),
                TrangThai = paid ? TrangThaiThanhToan.ThanhCong : state == TrangThaiDon.ChoThanhToan
                    ? TrangThaiThanhToan.DangXuLy : TrangThaiThanhToan.ThatBai,
                NgayThanhToan = paid ? created.AddMinutes(1) : null,
                MaLienKet = $"SAMPLE-U{number:000}-LINK",
                MaGiaoDich = paid ? $"SAMPLE-U{number:000}-PAY" : null,
                SoTienHoan = state == TrangThaiDon.DaHoanTien ? total
                    : state == TrangThaiDon.YeuCauHoanTien ? decimal.Truncate(total / 2) : 0,
                MaHoanTien = state is TrangThaiDon.DaHoanTien or TrangThaiDon.YeuCauHoanTien
                    ? $"SAMPLE-U{number:000}-REFUND" : null
            };
            order.ThanhToans.Add(payment);
            if (state == TrangThaiDon.ChoThanhToan) type.DangGiu += quantity;
            if (paid)
            {
                if (state != TrangThaiDon.DaHoanTien) type.DaBan += quantity;
                for (var ticket = 1; ticket <= quantity; ticket++)
                {
                    order.Ves.Add(new Ve
                    {
                        LoaiVe = type, MaVe = $"SAMPLE-U{number:000}-T{ticket:00}",
                        MaQR = $"SAMPLE-QR-U{number:000}-T{ticket:00}", NgayTao = payment.NgayThanhToan!.Value,
                        TrangThai = state == TrangThaiDon.DaHoanTien ? TrangThaiVe.DaHoanTien : TrangThaiVe.ChuaSuDung
                    });
                    addedTickets++;
                }
            }
            db.DonVes.Add(order);
        }

        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return new UserSeedResult(missing.Length, missing.Length, missing.Length, addedTickets, existing.Count);
    }

    public static string EmailFor(int number) => $"nguoidung{number:000}@{EmailDomain}";

    private static TrangThaiDon StateFor(int index) => (index % 10) switch
    {
        < 5 => TrangThaiDon.ThanhCong,
        5 or 6 => TrangThaiDon.ChoThanhToan,
        7 => TrangThaiDon.DaHuy,
        8 => TrangThaiDon.HetHan,
        _ => index / 10 % 2 == 0 ? TrangThaiDon.YeuCauHoanTien : TrangThaiDon.DaHoanTien
    };
}
