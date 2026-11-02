using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using QLSK.Common.Auth;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Features.DonVe;
using QLSK.Infrastructure.Email;
using QLSK.Infrastructure.PayOS;
using Xunit;

namespace QLSK.Tests;

public class NamingCompatibilityTests
{
    [Fact]
    public void Renamed_model_matches_existing_migration_snapshot()
    {
        using var nguCanhDuLieu = TaoNguCanhKiemTraModel();
        Assert.False(nguCanhDuLieu.Database.HasPendingModelChanges());
    }

    [Fact]
    public void Renamed_properties_keep_existing_sql_columns_and_primary_keys()
    {
        using var nguCanhDuLieu = TaoNguCanhKiemTraModel();
        foreach (var (kieuEntity, tenProperty, tenCotSql) in new[]
        {
            (typeof(TaiKhoan), nameof(TaiKhoan.TaiKhoanId), "Id"),
            (typeof(TaiKhoan), nameof(TaiKhoan.MatKhauBam), "MatKhau"),
            (typeof(LoaiVe), nameof(LoaiVe.GiaVe), "Gia"),
            (typeof(LoaiVe), nameof(LoaiVe.SoLuongDangGiu), "DangGiu"),
            (typeof(XuatVe), nameof(XuatVe.HinhThucXuat), "Kieu"),
            (typeof(XuatVe), nameof(XuatVe.EmailNhan), "DiaChi")
        })
        {
            var entityDuocKiemTra = nguCanhDuLieu.Model.FindEntityType(kieuEntity)!;
            var bangSql = StoreObjectIdentifier.Table(entityDuocKiemTra.GetTableName()!, entityDuocKiemTra.GetSchema());
            Assert.Equal(tenCotSql, entityDuocKiemTra.FindProperty(tenProperty)!.GetColumnName(bangSql));
        }
        foreach (var entityDuocKiemTra in nguCanhDuLieu.Model.GetEntityTypes())
            Assert.Equal(entityDuocKiemTra.ClrType.Name + "Id", Assert.Single(entityDuocKiemTra.FindPrimaryKey()!.Properties).Name);
    }

    [Fact]
    public void Renamed_account_hash_stays_private_and_wire_keys_stay_compatible()
    {
        var taiKhoanKiemThu = new TaiKhoan { TaiKhoanId = 17, MatKhauBam = "private-hash" };
        using var jsonTaiKhoan = JsonDocument.Parse(JsonSerializer.Serialize(taiKhoanKiemThu, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.Equal(17, jsonTaiKhoan.RootElement.GetProperty("id").GetInt32());
        Assert.False(jsonTaiKhoan.RootElement.TryGetProperty("matKhau", out _));
        Assert.False(jsonTaiKhoan.RootElement.TryGetProperty("matKhauBam", out _));
        Assert.DoesNotContain("private-hash", jsonTaiKhoan.RootElement.GetRawText());
    }

    [Fact]
    public void Existing_order_and_payment_json_bind_to_renamed_properties()
    {
        var tuyChonJson = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var duLieuTaoDon = JsonSerializer.Deserialize<TaoDonDto>("""{"suKienId":8,"items":[{"loaiVeId":3,"soLuong":2}]}""", tuyChonJson)!;
        Assert.Equal(3, Assert.Single(duLieuTaoDon.ChiTietDatVe).LoaiVeId);
        var webhookThanhToan = JsonSerializer.Deserialize<PayOsWebhook>("""{"code":"00","success":true,"signature":"signed","data":{"orderCode":25,"amount":120000,"paymentLinkId":"link-25"}}""", tuyChonJson)!;
        Assert.True(webhookThanhToan.ThanhCong);
        Assert.Equal("signed", webhookThanhToan.ChuKyWebhook);
        var duLieuWebhook = webhookThanhToan.DuLieuWebhook.Deserialize<PayOsWebhookData>(tuyChonJson)!;
        Assert.Equal(25, duLieuWebhook.MaDonVe);
        Assert.Equal("link-25", duLieuWebhook.MaLienKetThanhToan);
    }

    [Fact]
    public void Existing_configuration_keys_bind_to_renamed_options()
    {
        var cauHinhKiemThu = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-signing-key", ["Email:Host"] = "smtp.example.test",
            ["Email:Port"] = "2525", ["Email:From"] = "sender@example.test", ["Email:EnableSsl"] = "true"
        }).Build();
        Assert.Equal("test-signing-key", cauHinhKiemThu.GetSection("Jwt").Get<JwtOptions>()!.KhoaKyJwt);
        var tuyChonEmail = cauHinhKiemThu.GetSection("Email").Get<EmailOptions>()!;
        Assert.Equal("smtp.example.test", tuyChonEmail.MayChuSmtp);
        Assert.Equal(2525, tuyChonEmail.CongSmtp);
        Assert.Equal("sender@example.test", tuyChonEmail.EmailGui);
        Assert.True(tuyChonEmail.BatMaHoaSsl);
    }

    private static ApplicationDbContext TaoNguCanhKiemTraModel() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=QLSK_Model_Inspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options);
}
