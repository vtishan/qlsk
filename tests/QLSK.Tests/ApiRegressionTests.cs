using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.TaiKhoan;
using QLSK.Features.ThanhToan;
using QLSK.Infrastructure.PayOS;
using Xunit;
namespace QLSK.Tests;

public class ApiRegressionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact(DisplayName = "PUT /api/accounts/{id}/role - token cũ mất quyền")]
    public async Task Put_api_accounts_id_role_revokes_old_token()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var oldManagerClient = factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien");

        var changeRole = await factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien").PutAsJsonAsync(
            $"/api/accounts/{data.ManagerId}/role",
            new DoiVaiTroDto { VaiTroId = 1 });
        Assert.Equal(HttpStatusCode.OK, changeRole.StatusCode);

        var response = await oldManagerClient.GetAsync("/api/events/manage");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /api/auth/change-password - token cũ mất hiệu lực")]
    public async Task Put_api_auth_change_password_revokes_old_token()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();
        var userId = await ApiTestData.AddPasswordUserAsync(db, hasher);
        var oldClient = factory.CreateAuthenticatedClient(userId, "NguoiDung");

        var changePassword = await oldClient.PutAsJsonAsync(
            "/api/auth/change-password",
            new DoiMatKhauDto { MatKhauCu = "Old@123456", MatKhauMoi = "New@123456" });
        Assert.Equal(HttpStatusCode.OK, changePassword.StatusCode);

        var response = await oldClient.GetAsync("/api/accounts/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/events/{id} - không lộ mật khẩu tài khoản")]
    public async Task Get_api_events_id_does_not_expose_password_hash()
    {
        var data = await ApiTestData.ScenarioAsync(factory);

        var response = await factory.CreateClient().GetAsync($"/api/events/{data.EventId}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("MatKhau", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "GET /api/events/{eventId}/ticket-types - chặn sự kiện riêng tư")]
    public async Task Get_api_events_eventId_ticket_types_hides_private_event()
    {
        var data = await ApiTestData.ScenarioAsync(factory, TrangThaiSuKien.ChoDuyet);

        var response = await factory.CreateClient().GetAsync($"/api/events/{data.EventId}/ticket-types");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/check-in - chặn ngoài thời gian sự kiện")]
    public async Task Post_api_check_in_rejects_event_not_in_progress()
    {
        var data = await ApiTestData.ScenarioAsync(factory, TrangThaiSuKien.DaDuyet);
        var code = await factory.WithDb(db => db.Ves
            .Where(x => x.Id == data.TicketId)
            .Select(x => x.MaQR)
            .SingleAsync());

        var response = await factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien")
            .PostAsJsonAsync("/api/check-in", new { ma = code });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/payments - đơn miễn phí phát vé không tạo payment")]
    public async Task Post_api_payments_completes_free_order()
    {
        var data = await ApiTestData.ScenarioAsync(
            factory,
            orderState: TrangThaiDon.ChoThanhToan,
            paymentState: TrangThaiThanhToan.DangXuLy);
        await factory.WithDb(async db =>
        {
            var order = await db.DonVes.Include(x => x.ChiTiets).Include(x => x.ThanhToans).Include(x => x.Ves)
                .SingleAsync(x => x.Id == data.OrderId);
            var type = await db.LoaiVes.SingleAsync(x => x.Id == data.TicketTypeId);
            db.Ves.RemoveRange(order.Ves);
            db.ThanhToans.RemoveRange(order.ThanhToans);
            order.TongTien = 0;
            foreach (var detail in order.ChiTiets)
            {
                detail.DonGia = 0;
                detail.ThanhTien = 0;
            }
            type.Gia = 0;
            type.DaBan = 0;
            type.DangGiu = 1;
            await db.SaveChangesAsync();
        });

        var response = await factory.CreateAuthenticatedClient(data.UserId, "NguoiDung")
            .PostAsJsonAsync("/api/payments", new TaoThanhToanDto { DonVeId = data.OrderId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await factory.WithDb(async db =>
        {
            var order = await db.DonVes.Include(x => x.ThanhToans).Include(x => x.Ves)
                .SingleAsync(x => x.Id == data.OrderId);
            Assert.Equal(TrangThaiDon.ThanhCong, order.TrangThai);
            Assert.Empty(order.ThanhToans);
            Assert.Single(order.Ves);
        });
    }

    [Fact(DisplayName = "POST /api/payments/webhook - không phát vé dở dang")]
    public async Task Post_api_payments_webhook_is_atomic_when_inventory_invalid()
    {
        var data = await ApiTestData.ScenarioAsync(
            factory,
            orderState: TrangThaiDon.ChoThanhToan,
            paymentState: TrangThaiThanhToan.DangXuLy);
        var secondTypeId = await factory.WithDb(async db =>
        {
            var order = await db.DonVes.Include(x => x.Ves).SingleAsync(x => x.Id == data.OrderId);
            db.Ves.RemoveRange(order.Ves);
            var secondType = new LoaiVe
            {
                SuKienId = data.EventId,
                Ten = $"Vé hết-{Guid.NewGuid():N}",
                Gia = 50_000,
                SoLuong = 1,
                DaBan = 1,
                DangGiu = 0,
                MoBan = DateTime.UtcNow.AddDays(-1),
                DongBan = DateTime.UtcNow.AddDays(1)
            };
            order.ChiTiets.Add(new ChiTietDonVe
            {
                LoaiVe = secondType,
                SoLuong = 1,
                DonGia = 50_000,
                ThanhTien = 50_000
            });
            order.SoLuong = 2;
            order.TongTien = 150_000;
            var payment = await db.ThanhToans.SingleAsync(x => x.Id == data.PaymentId);
            payment.SoTien = order.TongTien;
            await db.SaveChangesAsync();
            return secondType.Id;
        });
        var before = await factory.WithDb(db => db.LoaiVes.AsNoTracking()
            .Where(x => x.Id == data.TicketTypeId)
            .Select(x => new { x.DaBan, x.DangGiu })
            .SingleAsync());
        var webhook = Webhook(data.OrderId, 150_000, $"REF-{Guid.NewGuid():N}", $"LINK-{Guid.NewGuid():N}");

        var response = await factory.CreateClient().PostAsJsonAsync("/api/payments/webhook", webhook);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await factory.WithDb(async db =>
        {
            var first = await db.LoaiVes.AsNoTracking().SingleAsync(x => x.Id == data.TicketTypeId);
            var second = await db.LoaiVes.AsNoTracking().SingleAsync(x => x.Id == secondTypeId);
            Assert.Equal(before.DaBan, first.DaBan);
            Assert.Equal(before.DangGiu, first.DangGiu);
            Assert.Equal(1, second.DaBan);
            Assert.Equal(0, await db.Ves.CountAsync(x => x.DonVeId == data.OrderId));
            Assert.Equal(TrangThaiThanhToan.NghiVan,
                await db.ThanhToans.Where(x => x.Id == data.PaymentId).Select(x => x.TrangThai).SingleAsync());
        });
    }

    [Fact(DisplayName = "POST /api/payments/webhook - ghép đúng paymentLinkId")]
    public async Task Post_api_payments_webhook_matches_payment_link()
    {
        var data = await ApiTestData.ScenarioAsync(
            factory,
            orderState: TrangThaiDon.ChoThanhToan,
            paymentState: TrangThaiThanhToan.DangXuLy);
        const string firstLink = "payment-link-a";
        const string secondLink = "payment-link-b";
        var secondPaymentId = await factory.WithDb(async db =>
        {
            db.Ves.RemoveRange(await db.Ves.Where(x => x.DonVeId == data.OrderId).ToListAsync());
            var first = await db.ThanhToans.SingleAsync(x => x.Id == data.PaymentId);
            first.MaLienKet = firstLink;
            var second = new ThanhToan
            {
                DonVeId = data.OrderId,
                SoTien = 100_000,
                MaLienKet = secondLink,
                TrangThai = TrangThaiThanhToan.DangXuLy
            };
            db.ThanhToans.Add(second);
            await db.SaveChangesAsync();
            return second.Id;
        });
        var reference = $"REF-{Guid.NewGuid():N}";

        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/payments/webhook",
            Webhook(data.OrderId, 100_000, reference, firstLink));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await factory.WithDb(async db =>
        {
            var first = await db.ThanhToans.AsNoTracking().SingleAsync(x => x.Id == data.PaymentId);
            var second = await db.ThanhToans.AsNoTracking().SingleAsync(x => x.Id == secondPaymentId);
            Assert.Equal(reference, first.MaGiaoDich);
            Assert.Equal(TrangThaiThanhToan.ThanhCong, first.TrangThai);
            Assert.Equal(TrangThaiThanhToan.ThatBai, second.TrangThai);
        });
    }

    [Fact(DisplayName = "POST /api/payments/{id}/refund - hoàn tồn vé")]
    public async Task Post_api_payments_id_refund_restores_inventory()
    {
        var data = await ApiTestData.ScenarioAsync(factory, orderState: TrangThaiDon.ThanhCong);

        var response = await factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/payments/{data.PaymentId}/refund",
            new HoanTienDto { SoTien = 100_000, MaHoanTien = $"REFUND-{Guid.NewGuid():N}" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await factory.WithDb(async db =>
        {
            Assert.Equal(0, await db.LoaiVes.Where(x => x.Id == data.TicketTypeId).Select(x => x.DaBan).SingleAsync());
            Assert.Equal(TrangThaiVe.DaHoanTien,
                await db.Ves.Where(x => x.Id == data.TicketId).Select(x => x.TrangThai).SingleAsync());
        });
    }

    [Fact(DisplayName = "POST /api/payments/{id}/refund - chặn vé đã check-in")]
    public async Task Post_api_payments_id_refund_rejects_checked_in_ticket()
    {
        var data = await ApiTestData.ScenarioAsync(
            factory,
            orderState: TrangThaiDon.ThanhCong,
            ticketState: TrangThaiVe.DaCheckIn);

        var response = await factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").PostAsJsonAsync(
            $"/api/payments/{data.PaymentId}/refund",
            new HoanTienDto { SoTien = 100_000, MaHoanTien = $"REFUND-{Guid.NewGuid():N}" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private static PayOsWebhook Webhook(int orderId, decimal amount, string reference, string paymentLinkId)
    {
        var data = JsonSerializer.SerializeToElement(new
        {
            orderCode = orderId,
            amount,
            reference,
            paymentLinkId,
            code = "00",
            desc = "success"
        });
        return new PayOsWebhook
        {
            Code = "00",
            Desc = "success",
            Success = true,
            Data = data,
            Signature = Sign(data)
        };
    }

    private static string Sign(JsonElement data)
    {
        var raw = string.Join("&", data.EnumerateObject()
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => $"{x.Name}={x.Value.ValueKind switch
            {
                JsonValueKind.String => x.Value.GetString() ?? "",
                JsonValueKind.Null or JsonValueKind.Undefined => "",
                _ => x.Value.GetRawText()
            }}"));
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("test-checksum-key"));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
    }
}
