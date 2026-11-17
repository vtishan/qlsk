using Microsoft.EntityFrameworkCore;
using QLSK.Common.Exceptions;
using SuKienEntity = QLSK.Domain.Entities.SuKien;

namespace QLSK.Features.SuKien;

internal static class EventListQuery
{
    internal static IOrderedQueryable<SuKienEntity> Apply(IQueryable<SuKienEntity> query, HttpContext? context)
    {
        string Value(string name)
        {
            var value = context?.Request.Query[name].ToString().Trim() ?? "";
            if (value.Length > 200) throw new ApiException(400, "VALIDATION_ERROR", "Bộ lọc quá dài.");
            return value;
        }
        var q = Value("q");
        if (q.Length != 0) query = query.Where(e =>
            EF.Functions.Collate(e.TenSuKien, "Vietnamese_CI_AI").Contains(q) ||
            EF.Functions.Collate(e.MoTaSuKien, "Vietnamese_CI_AI").Contains(q) ||
            EF.Functions.Collate(e.LoaiSuKien.TenLoaiSuKien, "Vietnamese_CI_AI").Contains(q) ||
            EF.Functions.Collate(e.BanToChuc.TenBanToChuc, "Vietnamese_CI_AI").Contains(q));
        var category = Value("category");
        if (category.Length != 0) query = query.Where(e => e.LoaiSuKien.TenLoaiSuKien == category);
        var location = Value("location");
        if (location.Length != 0) query = query.Where(e => e.DatDiaDiem != null &&
            (EF.Functions.Collate(e.DatDiaDiem.DiaDiem.TenDiaDiem, "Vietnamese_CI_AI").Contains(location) ||
             EF.Functions.Collate(e.DatDiaDiem.DiaDiem.DiaChi, "Vietnamese_CI_AI").Contains(location)));
        var price = Value("price");
        if (price == "free") query = query.Where(e => e.DanhSachLoaiVe.Min(t => (decimal?)t.GiaVe) == 0);
        if (price == "under500") query = query.Where(e => e.DanhSachLoaiVe.Min(t => (decimal?)t.GiaVe) < 500000);
        if (price == "over500") query = query.Where(e => e.DanhSachLoaiVe.Min(t => (decimal?)t.GiaVe) >= 500000);
        var date = Value("date");
        var now = DateTime.UtcNow;
        if (date == "today")
        {
            var start = now.AddHours(7).Date.AddHours(-7);
            var end = start.AddDays(1);
            query = query.Where(e => e.ThoiGianBatDau >= start && e.ThoiGianBatDau < end);
        }
        if (date == "month") query = query.Where(e => e.ThoiGianBatDau >= now && e.ThoiGianBatDau <= now.AddDays(30));
        var localNow = now.AddHours(7);
        var nextMonday = localNow.Date.AddDays(7 - (((int)localNow.DayOfWeek + 6) % 7)).AddHours(-7);
        if (date == "weekend") query = query.Where(e => e.ThoiGianBatDau >= now && e.ThoiGianBatDau < nextMonday &&
            (EF.Functions.DateDiffDay(new DateTime(2000, 1, 2), e.ThoiGianBatDau.AddHours(7)) % 7 == 0 ||
             EF.Functions.DateDiffDay(new DateTime(2000, 1, 2), e.ThoiGianBatDau.AddHours(7)) % 7 == 6));
        return Value("sort") switch
        {
            "name" => query.OrderBy(e => e.TenSuKien).ThenBy(e => e.SuKienId),
            "newest" => query.OrderByDescending(e => e.SuKienId),
            _ => query.OrderBy(e => e.ThoiGianBatDau).ThenBy(e => e.SuKienId)
        };
    }
}
