using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.LocalData;
using Xunit;

namespace QLSK.Tests;

public class LocalUserDataTests
{
    private const string Password = "LocalUserTest!2026";
    private static readonly DateTime Now = new(2026, 11, 2, 3, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Creates_100_unique_users_with_consistent_orders_payments_tickets_and_inventory()
    {
        using var factory = new ApiFactory();
        await factory.WithDb(async db =>
        {
            await AddEvent(db);
            var hasher = new PasswordHasher<TaiKhoan>();
            var result = await LocalUserSeeder.SeedAsync(db, hasher, Password, Now);
            Assert.Equal(new UserSeedResult(100, 100, 100, 118, 0), result);
            db.ChangeTracker.Clear();

            var users = await db.TaiKhoans.Where(x => x.VaiTroId == 1).OrderBy(x => x.Email).ToListAsync();
            Assert.Equal(100, users.Count);
            Assert.Equal(100, users.Select(x => x.HoTen).Distinct().Count());
            Assert.Equal(100, users.Select(x => x.Email).Distinct().Count());
            Assert.Equal(100, users.Select(x => x.MatKhau).Distinct().Count());
            Assert.Equal(91, users.Count(x => x.HoatDong));
            Assert.Equal(9, users.Count(x => !x.HoatDong));
            Assert.All(users, x => Assert.EndsWith("@sample.qlsk.test", x.Email));
            Assert.NotEqual(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(users[0], users[0].MatKhau, Password));
            Assert.Empty(await db.TaiKhoans.Where(x => x.VaiTroId == 4).ToListAsync());

            var orders = await db.DonVes.Include(x => x.TaiKhoan).Include(x => x.ChiTiets).ThenInclude(x => x.LoaiVe)
                .Include(x => x.ThanhToans).Include(x => x.Ves).ToListAsync();
            Assert.Equal(100, orders.Count);
            Assert.Equal(50, orders.Count(x => x.TrangThai == TrangThaiDon.ThanhCong));
            Assert.Equal(20, orders.Count(x => x.TrangThai == TrangThaiDon.ChoThanhToan));
            Assert.Equal(10, orders.Count(x => x.TrangThai == TrangThaiDon.DaHuy));
            Assert.Equal(10, orders.Count(x => x.TrangThai == TrangThaiDon.HetHan));
            Assert.Equal(5, orders.Count(x => x.TrangThai == TrangThaiDon.YeuCauHoanTien));
            Assert.Equal(5, orders.Count(x => x.TrangThai == TrangThaiDon.DaHoanTien));
            foreach (var order in orders)
            {
                var detail = Assert.Single(order.ChiTiets);
                var payment = Assert.Single(order.ThanhToans);
                Assert.Equal(order.SoLuong, detail.SoLuong);
                Assert.Equal(order.TongTien, detail.DonGia * detail.SoLuong);
                Assert.Equal(order.TongTien, detail.ThanhTien);
                Assert.Equal(order.TongTien, payment.SoTien);
                Assert.Equal(order.SuKienId, detail.LoaiVe.SuKienId);
                Assert.True(order.TaiKhoan.NgayTao <= order.NgayTao && order.NgayTao >= detail.LoaiVe.MoBan);
                Assert.InRange(payment.SoTienHoan, 0, payment.SoTien);
                var paid = payment.TrangThai == TrangThaiThanhToan.ThanhCong;
                Assert.Equal(paid ? order.SoLuong : 0, order.Ves.Count);
                if (paid) Assert.True(payment.NgayThanhToan < order.HetHan);
                if (order.TrangThai == TrangThaiDon.ChoThanhToan) Assert.True(order.HetHan > Now);
                if (order.TrangThai == TrangThaiDon.HetHan) Assert.True(order.HetHan < Now);
                if (order.TrangThai == TrangThaiDon.DaHoanTien)
                {
                    Assert.Equal(order.TongTien, payment.SoTienHoan);
                    Assert.All(order.Ves, x => Assert.Equal(TrangThaiVe.DaHoanTien, x.TrangThai));
                }
            }

            var tickets = await db.Ves.ToListAsync();
            Assert.Equal(118, tickets.Count);
            Assert.Equal(118, tickets.Select(x => x.MaVe).Distinct().Count());
            Assert.Equal(118, tickets.Select(x => x.MaQR).Distinct().Count());
            Assert.All(tickets, x => Assert.Null(x.NgayCheckIn));
            foreach (var type in await db.LoaiVes.ToListAsync())
            {
                Assert.Equal(tickets.Count(x => x.LoaiVeId == type.Id && x.TrangThai == TrangThaiVe.ChuaSuDung), type.DaBan);
                Assert.Equal(orders.Where(x => x.TrangThai == TrangThaiDon.ChoThanhToan)
                    .SelectMany(x => x.ChiTiets).Where(x => x.LoaiVeId == type.Id).Sum(x => x.SoLuong), type.DangGiu);
                Assert.InRange(type.DaBan + type.DangGiu, 0, type.SoLuong);
            }
        });
    }

    [Fact]
    public async Task Repeated_population_preserves_user_edits_passwords_orders_and_inventory()
    {
        using var factory = new ApiFactory();
        await factory.WithDb(async db =>
        {
            await AddEvent(db);
            var hasher = new PasswordHasher<TaiKhoan>();
            await LocalUserSeeder.SeedAsync(db, hasher, Password, Now);
            var user = await db.TaiKhoans.SingleAsync(x => x.Email == LocalUserSeeder.EmailFor(1));
            user.HoTen = "Tên người dùng đã chỉnh sửa";
            user.HoatDong = false;
            user.MatKhau = hasher.HashPassword(user, "ChangedUserPassword!2026");
            await db.SaveChangesAsync();
            var hash = user.MatKhau;
            var inventory = await db.LoaiVes.OrderBy(x => x.Id).Select(x => new { x.Id, x.DaBan, x.DangGiu }).ToListAsync();
            db.ChangeTracker.Clear();

            var result = await LocalUserSeeder.SeedAsync(db, hasher, "DoNotResetPassword!2026", Now.AddDays(1));
            Assert.Equal(new UserSeedResult(0, 0, 0, 0, 100), result);
            user = await db.TaiKhoans.SingleAsync(x => x.Id == user.Id);
            Assert.Equal("Tên người dùng đã chỉnh sửa", user.HoTen);
            Assert.False(user.HoatDong);
            Assert.Equal(hash, user.MatKhau);
            Assert.Equal(100, await db.DonVes.CountAsync());
            Assert.Equal(100, await db.ThanhToans.CountAsync());
            Assert.Equal(118, await db.Ves.CountAsync());
            Assert.Equal(inventory, await db.LoaiVes.OrderBy(x => x.Id).Select(x => new { x.Id, x.DaBan, x.DangGiu }).ToListAsync());
        });
    }

    [Fact]
    public async Task Insufficient_inventory_does_not_leave_a_partial_batch_or_change_capacity()
    {
        using var factory = new ApiFactory();
        await factory.WithDb(async db =>
        {
            await AddEvent(db, capacityPerType: 1);
            db.ChangeTracker.Clear();
            await Assert.ThrowsAsync<InvalidOperationException>(() => LocalUserSeeder.SeedAsync(db,
                new PasswordHasher<TaiKhoan>(), Password, Now));
            Assert.DoesNotContain(db.ChangeTracker.Entries(), x => x.State == EntityState.Added || x.State == EntityState.Modified);
            Assert.Empty(await db.TaiKhoans.Where(x => x.VaiTroId == 1).ToListAsync());
            Assert.Empty(await db.DonVes.ToListAsync());
            Assert.All(await db.LoaiVes.ToListAsync(), x => Assert.Equal(0, x.DaBan + x.DangGiu));
        });
    }

    private static async Task AddEvent(QLSK.Data.ApplicationDbContext db, int capacityPerType = 200)
    {
        var item = new SuKien
        {
            Ten = "Sự kiện cho dữ liệu người dùng", MoTa = "Integration test", TrangThai = TrangThaiSuKien.DaDuyet,
            BatDau = Now.AddDays(7), KetThuc = Now.AddDays(7).AddHours(3), SoNguoi = capacityPerType * 2,
            BanToChuc = new BanToChuc
            {
                Ten = "Đơn vị kiểm thử", TaiKhoan = new TaiKhoan { Email = "owner@sample.test", HoTen = "Chủ sự kiện", VaiTroId = 2 }
            },
            LoaiSuKien = new LoaiSuKien { Ten = "Loại kiểm thử" }
        };
        foreach (var name in new[] { "Tiêu chuẩn", "Đồng hành" })
            item.LoaiVes.Add(new LoaiVe
            {
                Ten = name, Gia = name == "Tiêu chuẩn" ? 120_000 : 220_000, SoLuong = capacityPerType,
                MoBan = Now.AddDays(-3), DongBan = item.BatDau
            });
        db.SuKiens.Add(item);
        await db.SaveChangesAsync();
    }
}
