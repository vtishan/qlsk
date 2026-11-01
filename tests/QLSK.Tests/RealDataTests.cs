using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using QLSK.Features.QuanTri;
using QLSK.Features.LoaiVe;
using QLSK.Setup;
using Xunit;

namespace QLSK.Tests;

public class RealDataTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Administrator_edits_approved_imported_event_and_public_reads_database_changes()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var dto = await EventDto(data.EventId);
        dto.Ten = "Nội dung quản trị vừa cập nhật"; dto.HinhAnh = "/assets/images/events/event-002.jpg";
        var response = await Admin(data).PutAsJsonAsync($"/api/admin/catalog/events/{data.EventId}", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await factory.CreateClient().GetStringAsync($"/api/events/{data.EventId}"));
        Assert.Equal(dto.Ten, body.RootElement.GetProperty("data").GetProperty("ten").GetString());
        Assert.Equal(dto.HinhAnh, body.RootElement.GetProperty("data").GetProperty("hinhAnh").GetString());
        await factory.WithDb(async db => Assert.Equal(TrangThaiSuKien.DaDuyet, (await db.SuKiens.FindAsync(data.EventId))!.TrangThai));
    }

    [Fact]
    public async Task Booked_schedule_and_sold_capacity_cannot_be_overwritten()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var dto = await EventDto(data.EventId); dto.BatDau = dto.BatDau.AddDays(1); dto.KetThuc = dto.KetThuc.AddDays(1);
        var response = await Admin(data).PutAsJsonAsync($"/api/admin/catalog/events/{data.EventId}", dto);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("events")]
    [InlineData("venues")]
    [InlineData("event-types")]
    [InlineData("ticket-types")]
    [InlineData("organizers")]
    public async Task Linked_records_cannot_be_deleted(string kind)
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var id = kind switch { "events" => data.EventId, "venues" => data.VenueId, "event-types" => data.CategoryId, "ticket-types" => data.TicketTypeId,
            _ => await factory.WithDb(db => db.BanToChucs.Where(x => x.TaiKhoanId == data.OrganizerUserId).Select(x => x.Id).SingleAsync()) };
        Assert.Equal(HttpStatusCode.Conflict, (await Admin(data).DeleteAsync($"/api/admin/catalog/{kind}/{id}")).StatusCode);
    }

    [Theory]
    [InlineData("NguoiDung", 1)]
    [InlineData("BanToChuc", 2)]
    [InlineData("QuanLySuKien", 3)]
    public async Task Catalog_mutations_are_admin_only(string role, int roleId)
    {
        var userId = await factory.WithDb(db => ApiTestData.AddUserAsync(db, roleId));
        var client = factory.CreateAuthenticatedClient(userId, role);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/catalog")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync("/api/admin/catalog/events/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/admin/catalog/events", new QuanTriSuKienDto())).StatusCode);
    }

    [Fact]
    public async Task Unused_event_can_be_created_and_deleted_without_seeding_other_data()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var dto = await EventDto(data.EventId);
        var response = await Admin(data).PostAsJsonAsync("/api/admin/catalog/events", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var id = body.RootElement.GetProperty("data").GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK, (await Admin(data).DeleteAsync($"/api/admin/catalog/events/{id}")).StatusCode);
        await factory.WithDb(async db => Assert.False(await db.SuKiens.AnyAsync(x => x.Id == id)));
    }

    [Fact]
    public async Task Administrator_can_manage_existing_ticket_types_but_cannot_reduce_reserved_inventory()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var dto = new LoaiVeDto { Ten = "Vé đã chỉnh sửa", Gia = 220000, SoLuong = 120, MoBan = DateTime.UtcNow.AddDays(-1), DongBan = DateTime.UtcNow.AddDays(4) };
        Assert.Equal(HttpStatusCode.OK, (await Admin(data).PutAsJsonAsync($"/api/ticket-types/{data.TicketTypeId}", dto)).StatusCode);
        await factory.WithDb(async db => Assert.Equal(dto.Gia, (await db.LoaiVes.FindAsync(data.TicketTypeId))!.Gia));
        await factory.WithDb(async db => { (await db.LoaiVes.FindAsync(data.TicketTypeId))!.DangGiu = 10; await db.SaveChangesAsync(); });
        dto.SoLuong = 1;
        Assert.Equal(HttpStatusCode.Conflict, (await Admin(data).PutAsJsonAsync($"/api/ticket-types/{data.TicketTypeId}", dto)).StatusCode);
    }

    [Fact]
    public async Task Activity_filters_by_account_foreign_key_and_export_history_uses_real_records()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        using var body = JsonDocument.Parse(await Admin(data).GetStringAsync($"/api/admin/catalog/accounts/{data.OrganizerUserId}/activity"));
        Assert.Contains(body.RootElement.GetProperty("data").GetProperty("suKien").EnumerateArray(), x => x.GetProperty("id").GetInt32() == data.EventId);
        using var orders = JsonDocument.Parse(await Admin(data).GetStringAsync($"/api/admin/catalog/accounts/{data.UserId}/activity"));
        Assert.Contains(orders.RootElement.GetProperty("data").GetProperty("donVe").EnumerateArray(), x => x.GetProperty("id").GetInt32() == data.OrderId);
        await factory.WithDb(async db => { db.XuatVes.Add(new XuatVe { VeId = data.TicketId, Kieu = "Email", ThanhCong = false }); await db.SaveChangesAsync(); });
        var client = factory.CreateAuthenticatedClient(data.UserId, "NguoiDung");
        using var history = JsonDocument.Parse(await client.GetStringAsync($"/api/tickets/{data.TicketId}/exports"));
        var item = Assert.Single(history.RootElement.GetProperty("data").EnumerateArray());
        Assert.Equal("Email", item.GetProperty("kieu").GetString()); Assert.False(item.GetProperty("thanhCong").GetBoolean());
        Assert.Equal(HttpStatusCode.Forbidden, (await factory.CreateAuthenticatedClient(data.ManagerId, "QuanLySuKien").GetAsync($"/api/tickets/{data.TicketId}/exports")).StatusCode);
    }

    [Fact]
    public async Task Terminal_admin_update_hashes_password_and_revokes_existing_session()
    {
        var data = await ApiTestData.ScenarioAsync(factory);
        var old = Admin(data);
        await factory.WithDb(db => AdminProvisioner.UpdateAsync(db, new PasswordHasher<TaiKhoan>(), data.AdminId, $"changed-{Guid.NewGuid():N}@customer.test", "lowercase123!password"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await old.GetAsync("/api/admin/catalog")).StatusCode);
    }

    [Fact]
    public async Task Adoption_preserves_business_keys_and_revokes_old_credentials_idempotently()
    {
        using var separateFactory = new ApiFactory();
        var data = await ApiTestData.ScenarioAsync(separateFactory);
        var hasher = new PasswordHasher<TaiKhoan>();
        await separateFactory.WithDb(async db =>
        {
            (await db.TaiKhoans.FindAsync(data.AdminId))!.Email = "admin@qlsk.local";
            (await db.TaiKhoans.FindAsync(data.OrganizerUserId))!.Email = "demo.btc01@qlsk.local";
            await db.SaveChangesAsync();
        });
        await separateFactory.WithDb(async db => Assert.Equal(1, await LegacyDataMigrator.AdoptAsync(db, hasher, "owner@customer.test", "lowercase123!password")));
        await separateFactory.WithDb(async db =>
        {
            Assert.Equal(0, await LegacyDataMigrator.AdoptAsync(db, hasher, "owner@customer.test", "lowercase123!password"));
            Assert.Equal(1, await db.SuKiens.CountAsync()); Assert.Equal(1, await db.DonVes.CountAsync());
            var owner = (await db.TaiKhoans.FindAsync(data.OrganizerUserId))!;
            Assert.False(owner.HoatDong); Assert.EndsWith("@disabled.invalid", owner.Email);
            Assert.Equal(data.EventId, (await db.SuKiens.SingleAsync()).Id);
            var admin = (await db.TaiKhoans.FindAsync(data.AdminId))!;
            Assert.NotEqual(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(admin, admin.MatKhau, "lowercase123!password"));
        });
    }

    private HttpClient Admin(TestScenario data) => factory.CreateAuthenticatedClient(data.AdminId, "QuanTriVien");
    private Task<QuanTriSuKienDto> EventDto(int id) => factory.WithDb(async db =>
    {
        var item = (await db.SuKiens.AsNoTracking().SingleAsync(x => x.Id == id));
        return new QuanTriSuKienDto { BanToChucId = item.BanToChucId, LoaiSuKienId = item.LoaiSuKienId,
            Ten = item.Ten, MoTa = item.MoTa, HinhAnh = item.HinhAnh, BatDau = item.BatDau, KetThuc = item.KetThuc, SoNguoi = item.SoNguoi };
    });
}
