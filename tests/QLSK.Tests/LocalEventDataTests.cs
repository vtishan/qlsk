using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QLSK.LocalData;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using Xunit;

namespace QLSK.Tests;

public class LocalEventDataTests
{
    [Fact]
    public void Local_population_rejects_the_business_database()
        => Assert.Throws<InvalidOperationException>(() => LocalSqlGuard.Validate(
            "Server=.\\SQL2022EXP;Database=QLSK;Integrated Security=True", requireTestDatabase: true));

    private const string LegacyDescription = "Đây là sự kiện mẫu phục vụ trải nghiệm hệ thống QLSK.";

    [Theory]
    [InlineData("Server=customer.example.com;Database=QLSK;Integrated Security=True")]
    [InlineData("Server=192.0.2.15;Database=QLSK;Integrated Security=True")]
    [InlineData("Server=localhost;Database=master;Integrated Security=True")]
    [InlineData("Server=localhost;Database=TEMPDB;Integrated Security=True")]
    [InlineData("Server=localhost;Integrated Security=True")]
    [InlineData("Server=localhost;Database=QLSK;AttachDbFilename=C:\\test.mdf;Integrated Security=True")]
    public void Local_data_tool_rejects_remote_system_or_attached_databases(string connection)
        => Assert.Throws<InvalidOperationException>(() => LocalSqlGuard.Validate(connection));

    [Theory]
    [InlineData("Server=.\\SQL2022EXP;Database=QLSK_Local;Integrated Security=True")]
    [InlineData("Server=tcp:127.0.0.1,1433;Database=QLSK_Local;Integrated Security=True")]
    public void Local_data_tool_accepts_explicit_local_application_databases(string connection)
        => LocalSqlGuard.Validate(connection);

    [Fact]
    public async Task Application_startup_has_no_business_data_or_local_data_dependency()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        await factory.WithDb(async db =>
        {
            Assert.Equal(4, await db.VaiTros.CountAsync());
            Assert.Empty(await db.TaiKhoans.ToListAsync());
            Assert.Empty(await db.BanToChucs.ToListAsync());
            Assert.Empty(await db.DiaDiems.ToListAsync());
            Assert.Empty(await db.SuKiens.ToListAsync());
            Assert.Empty(await db.LoaiVes.ToListAsync());
        });

        var application = typeof(Program).Assembly;
        Assert.NotEqual(application, typeof(LocalEventSeeder).Assembly);
        Assert.DoesNotContain(application.GetReferencedAssemblies(), x => x.Name == "QLSK.LocalData");
        Assert.DoesNotContain(application.GetTypes(), x => x.Namespace == "QLSK.Data.Seed");
        foreach (var path in new[] { "/Data/Seed/demo-events.json", "/tests/QLSK.LocalData/catalog/events.json" })
        {
            using var response = await client.GetAsync(path);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task Explicit_local_population_is_bookable_and_repeated_runs_preserve_edits()
    {
        using var factory = new ApiFactory();
        var root = factory.Services.GetRequiredService<IHostEnvironment>().ContentRootPath;
        var now = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc);
        var hasher = new PasswordHasher<TaiKhoan>();

        await factory.WithDb(async db =>
        {
            Assert.Equal(100, await LocalEventSeeder.SeedAsync(db, hasher, root, "LocalTest@123456", now));
            db.ChangeTracker.Clear();

            var events = await db.SuKiens.Include(x => x.LoaiVes)
                .Include(x => x.DatDiaDiem).ThenInclude(x => x!.DiaDiem)
                .OrderBy(x => x.Id).ToListAsync();
            Assert.Equal(100, events.Count);
            Assert.All(events, item => Assert.DoesNotContain(LegacyDescription, item.MoTa));
            Assert.Equal(100, events.Select(x => x.HinhAnh).Distinct().Count());
            Assert.Equal(10, await db.LoaiSuKiens.CountAsync());
            Assert.Equal(5, await db.BanToChucs.CountAsync());
            Assert.Equal(10, await db.DiaDiems.CountAsync());
            Assert.Equal(200, await db.LoaiVes.CountAsync());
            Assert.Equal(100, await db.DatDiaDiems.CountAsync());

            foreach (var item in events)
            {
                Assert.Equal(TrangThaiSuKien.DaDuyet, item.TrangThai);
                Assert.True(item.BatDau > now);
                Assert.True(item.KetThuc > item.BatDau);
                Assert.NotNull(item.HinhAnh);
                Assert.StartsWith("/assets/images/events/event-", item.HinhAnh);
                Assert.EndsWith(".jpg", item.HinhAnh);
                var imagePath = Path.Combine(root, "wwwroot", item.HinhAnh.TrimStart('/'));
                Assert.True(File.Exists(imagePath), $"Missing local image {item.HinhAnh}");
                using (var image = File.OpenRead(imagePath))
                {
                    Assert.Equal(0xff, image.ReadByte());
                    Assert.Equal(0xd8, image.ReadByte());
                }

                var booking = Assert.IsType<DatDiaDiem>(item.DatDiaDiem);
                Assert.Equal(TrangThaiDatCho.DaXacNhan, booking.TrangThai);
                Assert.Equal(item.BatDau, booking.BatDau);
                Assert.Equal(item.KetThuc, booking.KetThuc);
                Assert.True(booking.DiaDiem.SucChua >= item.SoNguoi);
                Assert.InRange(booking.TienCoc, 0, booking.TongTien);
                Assert.Equal(item.SoNguoi, item.LoaiVes.Sum(x => x.SoLuong));
                Assert.All(item.LoaiVes, ticket =>
                {
                    Assert.True(ticket.Gia >= 0);
                    Assert.True(ticket.SoLuong > 0);
                    Assert.Equal(0, ticket.DaBan);
                    Assert.Equal(0, ticket.DangGiu);
                    Assert.True(ticket.MoBan <= now && ticket.DongBan > now);
                    Assert.True(ticket.DongBan <= item.BatDau);
                });
            }

            foreach (var venueGroup in events.GroupBy(x => x.DatDiaDiem!.DiaDiemId))
            {
                var bookings = venueGroup.Select(x => x.DatDiaDiem!).OrderBy(x => x.BatDau).ToArray();
                for (var i = 1; i < bookings.Length; i++)
                    Assert.True(bookings[i - 1].KetThuc <= bookings[i].BatDau);
            }

            var edited = events[0];
            var originalStart = edited.BatDau;
            edited.Ten = "Tên sự kiện do người dùng chỉnh sửa";
            edited.MoTa = "Nội dung đã chỉnh sửa phải được giữ lại.";
            edited.LoaiVes.First().Gia = 123_000;
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            Assert.Equal(0, await LocalEventSeeder.SeedAsync(db, hasher, root, "NewPassword@123", now.AddDays(1)));
            db.ChangeTracker.Clear();
            var saved = await db.SuKiens.Include(x => x.LoaiVes).SingleAsync(x => x.Id == edited.Id);
            Assert.Equal(edited.Ten, saved.Ten);
            Assert.Equal(edited.MoTa, saved.MoTa);
            Assert.Equal(originalStart, saved.BatDau);
            Assert.Contains(saved.LoaiVes, x => x.Gia == 123_000);
            Assert.Equal(100, await db.SuKiens.CountAsync());
            Assert.Equal(200, await db.LoaiVes.CountAsync());
            Assert.Equal(100, await db.DatDiaDiems.CountAsync());
            Assert.Equal(5, await db.BanToChucs.CountAsync());
            Assert.Equal(10, await db.DiaDiems.CountAsync());
        });
    }

    [Fact]
    public async Task Normalizing_existing_data_preserves_identity_relations_edits_and_passwords()
    {
        using var factory = new ApiFactory();
        var root = factory.Services.GetRequiredService<IHostEnvironment>().ContentRootPath;
        var now = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc);
        await factory.WithDb(async db =>
        {
            await LocalEventSeeder.SeedAsync(db, new PasswordHasher<TaiKhoan>(), root, "LocalTest@123456", now);
            var account = await db.TaiKhoans.Include(x => x.BanToChuc)
                .SingleAsync(x => x.Email == "organizer01@qlsk.local");
            var organizer = account.BanToChuc!;
            var venue = await db.DiaDiems.SingleAsync(x => x.Ten == "Trung tâm sự kiện Bến Sông");
            var events = await db.SuKiens.Include(x => x.LoaiVes).Include(x => x.DatDiaDiem)
                .Where(x => x.BanToChucId == organizer.Id).OrderBy(x => x.Id).ToListAsync();
            var first = events[0];
            var edited = events[1];
            account.Email = "demo.btc01@qlsk.local";
            account.HoTen = "Ban tổ chức mẫu 01";
            organizer.Ten = "SựKiện Hub Demo 01";
            organizer.MoTa = "Ban tổ chức mẫu phục vụ trải nghiệm hệ thống.";
            venue.Ten = "Không gian sự kiện Demo 01 - TP. Hồ Chí Minh";
            venue.DiaChi = "Khu trải nghiệm mẫu 01, TP. Hồ Chí Minh (địa điểm giả lập)";
            first.MoTa = "Nội dung đã được biên tập riêng. " + LegacyDescription;
            edited.Ten = "Tên chương trình đã được khách sửa";
            edited.MoTa = "Nội dung đã chỉnh sửa cần được giữ nguyên.";
            edited.LoaiVes.First().Gia = 123_000;
            await db.SaveChangesAsync();
            var accountId = account.Id;
            var organizerId = organizer.Id;
            var venueId = venue.Id;
            var firstId = first.Id;
            var editedId = edited.Id;
            var passwordHash = account.MatKhau;
            var firstStart = first.BatDau;
            var image = first.HinhAnh;
            var bookingId = first.DatDiaDiem!.Id;
            var editedTitle = edited.Ten;
            var editedDescription = edited.MoTa;
            db.ChangeTracker.Clear();

            var preview = await LocalEventSeeder.NormalizeExistingAsync(db, dryRun: true);
            Assert.True(preview.Accounts > 0 && preview.Organizers > 0 && preview.Venues > 0 && preview.Events > 0);
            db.ChangeTracker.Clear();
            Assert.Equal("demo.btc01@qlsk.local", (await db.TaiKhoans.SingleAsync(x => x.Id == accountId)).Email);
            Assert.Contains(LegacyDescription, (await db.SuKiens.SingleAsync(x => x.Id == firstId)).MoTa);
            db.ChangeTracker.Clear();

            await LocalEventSeeder.NormalizeExistingAsync(db);
            db.ChangeTracker.Clear();
            var savedAccount = await db.TaiKhoans.SingleAsync(x => x.Id == accountId);
            var savedOrganizer = await db.BanToChucs.SingleAsync(x => x.Id == organizerId);
            var savedVenue = await db.DiaDiems.SingleAsync(x => x.Id == venueId);
            var saved = await db.SuKiens.Include(x => x.DatDiaDiem).SingleAsync(x => x.Id == firstId);
            var savedEdited = await db.SuKiens.Include(x => x.LoaiVes).SingleAsync(x => x.Id == editedId);
            Assert.Equal("organizer01@qlsk.local", savedAccount.Email);
            Assert.Equal(passwordHash, savedAccount.MatKhau);
            Assert.Equal("Kết Nối Văn Hóa", savedOrganizer.Ten);
            Assert.Equal(accountId, savedOrganizer.TaiKhoanId);
            Assert.Equal("Trung tâm sự kiện Bến Sông", savedVenue.Ten);
            Assert.Equal("Khu trung tâm, TP. Hồ Chí Minh", savedVenue.DiaChi);
            Assert.Equal(600, savedVenue.SucChua);
            Assert.Equal(2_000_000, savedVenue.GiaThue);
            Assert.Equal("Nội dung đã được biên tập riêng.", saved.MoTa);
            Assert.Equal(firstStart, saved.BatDau);
            Assert.Equal(image, saved.HinhAnh);
            Assert.Equal(organizerId, saved.BanToChucId);
            Assert.Equal(bookingId, saved.DatDiaDiem!.Id);
            Assert.Equal(venueId, saved.DatDiaDiem.DiaDiemId);
            Assert.Equal(editedTitle, savedEdited.Ten);
            Assert.Equal(editedDescription, savedEdited.MoTa);
            Assert.Contains(savedEdited.LoaiVes, x => x.Gia == 123_000);
            Assert.Equal(100, await db.SuKiens.CountAsync());
            Assert.Equal(200, await db.LoaiVes.CountAsync());
            Assert.Equal(100, await db.DatDiaDiems.CountAsync());
            var again = await LocalEventSeeder.NormalizeExistingAsync(db);
            Assert.Equal(0, again.Accounts + again.Organizers + again.Venues + again.Events);
        });
    }

}
