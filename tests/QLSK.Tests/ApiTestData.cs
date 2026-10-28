using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.Tests;

public sealed record TestScenario(
    int UserId,
    int OrganizerUserId,
    int ManagerId,
    int AdminId,
    int CategoryId,
    int EventId,
    int VenueId,
    int TicketTypeId,
    int OrderId,
    int PaymentId,
    int TicketId,
    int BookingId);

public static class ApiTestData
{
    public static async Task<TestScenario> SeedAsync(
        ApplicationDbContext db,
        TrangThaiSuKien eventState = TrangThaiSuKien.DaDuyet,
        TrangThaiDon orderState = TrangThaiDon.ChoThanhToan,
        TrangThaiThanhToan paymentState = TrangThaiThanhToan.ThanhCong,
        TrangThaiVe ticketState = TrangThaiVe.ChuaSuDung,
        TrangThaiDatCho bookingState = TrangThaiDatCho.DaXacNhan)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var user = User(1, $"user-{suffix}@test.local");
        var organizerUser = User(2, $"organizer-{suffix}@test.local");
        var manager = User(3, $"manager-{suffix}@test.local");
        var admin = User(4, $"admin-{suffix}@test.local");
        db.TaiKhoans.AddRange(user, organizerUser, manager, admin);

        var organizer = new BanToChuc
        {
            TaiKhoan = organizerUser,
            Ten = $"Ban tổ chức {suffix}"
        };
        var category = new LoaiSuKien
        {
            Ten = $"Loại {suffix}",
            MoTa = "Dữ liệu integration test",
            HoatDong = true
        };
        db.AddRange(organizer, category);

        var start = eventState switch
        {
            TrangThaiSuKien.DangDienRa => DateTime.UtcNow.AddMinutes(-30),
            TrangThaiSuKien.DaKetThuc => DateTime.UtcNow.AddDays(-1),
            _ => DateTime.UtcNow.AddDays(10)
        };
        var @event = new SuKien
        {
            BanToChuc = organizer,
            LoaiSuKien = category,
            Ten = $"Sự kiện {suffix}",
            MoTa = "Dữ liệu integration test",
            BatDau = start,
            KetThuc = start.AddHours(3),
            SoNguoi = 500,
            TrangThai = eventState
        };
        var venue = new DiaDiem
        {
            Ten = $"Địa điểm {suffix}",
            DiaChi = $"Địa chỉ {suffix}",
            SucChua = 1000,
            GiaThue = 1_000_000,
            HoatDong = true
        };
        db.AddRange(@event, venue);

        var ticketType = new LoaiVe
        {
            SuKien = @event,
            Ten = $"Vé {suffix}",
            Gia = 100_000,
            SoLuong = 100,
            DaBan = orderState == TrangThaiDon.ThanhCong ? 1 : 0,
            DangGiu = orderState == TrangThaiDon.ChoThanhToan ? 1 : 0,
            MoBan = DateTime.UtcNow.AddDays(-1),
            DongBan = DateTime.UtcNow.AddDays(5)
        };
        var order = new DonVe
        {
            TaiKhoan = user,
            SuKien = @event,
            SoLuong = 1,
            TongTien = 100_000,
            TrangThai = orderState,
            HetHan = DateTime.UtcNow.AddMinutes(15)
        };
        order.ChiTiets.Add(new ChiTietDonVe
        {
            LoaiVe = ticketType,
            SoLuong = 1,
            DonGia = 100_000,
            ThanhTien = 100_000
        });
        var payment = new ThanhToan
        {
            DonVe = order,
            SoTien = 100_000,
            TrangThai = paymentState,
            MaGiaoDich = $"PAY-{suffix}",
            NgayThanhToan = paymentState == TrangThaiThanhToan.ThanhCong ? DateTime.UtcNow : null
        };
        var ticket = new Ve
        {
            DonVe = order,
            LoaiVe = ticketType,
            MaVe = $"VE-{suffix}",
            MaQR = $"QR-{suffix}",
            TrangThai = ticketState
        };
        var booking = new DatDiaDiem
        {
            DiaDiem = venue,
            SuKien = @event,
            BatDau = start,
            KetThuc = start.AddHours(3),
            TienCoc = 100_000,
            TongTien = venue.GiaThue,
            TrangThai = bookingState
        };
        db.AddRange(ticketType, order, payment, ticket, booking);
        await db.SaveChangesAsync();

        return new TestScenario(
            user.Id,
            organizerUser.Id,
            manager.Id,
            admin.Id,
            category.Id,
            @event.Id,
            venue.Id,
            ticketType.Id,
            order.Id,
            payment.Id,
            ticket.Id,
            booking.Id);
    }

    public static async Task<int> AddUserAsync(
        ApplicationDbContext db,
        int roleId,
        bool active = true)
    {
        var user = User(roleId, $"user-{Guid.NewGuid():N}@test.local", active);
        db.TaiKhoans.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    public static async Task<int> AddPendingVerificationAsync(ApplicationDbContext db)
    {
        var user = User(1, $"verification-{Guid.NewGuid():N}@test.local");
        var request = new YeuCauXacThucToChuc
        {
            TaiKhoan = user,
            TenToChuc = $"Tổ chức {Guid.NewGuid():N}",
            GiayTo = "https://test.local/document.pdf"
        };
        db.Add(request);
        await db.SaveChangesAsync();
        return request.Id;
    }

    public static Task<TestScenario> ScenarioAsync(ApiFactory factory,
        TrangThaiSuKien eventState = TrangThaiSuKien.DaDuyet,
        TrangThaiDon orderState = TrangThaiDon.ChoThanhToan,
        TrangThaiThanhToan paymentState = TrangThaiThanhToan.ThanhCong,
        TrangThaiVe ticketState = TrangThaiVe.ChuaSuDung,
        TrangThaiDatCho bookingState = TrangThaiDatCho.DaXacNhan)
        => factory.WithDb(db => SeedAsync(db, eventState, orderState, paymentState, ticketState, bookingState));

    public static async Task<int> AddPasswordUserAsync(ApplicationDbContext db, IPasswordHasher<TaiKhoan> hasher)
    {
        var user = User(1, $"password-{Guid.NewGuid():N}@test.local");
        user.MatKhau = hasher.HashPassword(user, "Old@123456");
        db.TaiKhoans.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    private static TaiKhoan User(int roleId, string email, bool active = true) => new()
    {
        HoTen = $"Test role {roleId}",
        Email = email,
        MatKhau = "not-used-by-token-tests",
        VaiTroId = roleId,
        HoatDong = active
    };
}
