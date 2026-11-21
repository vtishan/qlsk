using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;
using System.Text.Json;

namespace QLSK.Common.Data;

internal static class SqlOperations
{
    public static void Audit(AppDbContext db, TaiKhoan actor, string action, string subject, int? id, string? change = null)
    {
        db.NhatKyHeThong.Add(new NhatKyHeThong {
            TaiKhoanId = actor.Id, HanhDong = action, DoiTuong = subject, DoiTuongId = id,
            ThoiGian = DateTime.UtcNow, DuLieuThayDoi = change == null ? null : JsonSerializer.Serialize(new { note = change })
        });
    }

    public static string? AuditNote(string? value)
    {
        if (value == null) return null;
        using var data = JsonDocument.Parse(value);
        return data.RootElement.TryGetProperty("note", out var note) && note.ValueKind == JsonValueKind.String
            ? note.GetString() : null;
    }

    public static void Notify(AppDbContext db, int accountId, string title, string message, string type)
    {
        db.ThongBao.Add(new ThongBao {
            TaiKhoanId = accountId, TieuDe = title, NoiDung = message, LoaiThongBao = type,
            KenhGui = KenhGuiThongBao.TrongUngDung, TrangThaiGui = TrangThaiGuiThongBao.DaGui,
            NgayTao = DateTime.UtcNow, NgayGui = DateTime.UtcNow
        });
    }

    public static Task Lock(AppDbContext db, string resource, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive',
                @LockOwner = 'Transaction', @LockTimeout = 5000;
            IF @result < 0 THROW 50001, 'QLSK transaction lock not acquired', 1;
            """, ct);
}
