using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QLSK.Common.Exceptions;
using QLSK.Data;

namespace QLSK.Common.Models;

public static class Pagination
{
    public static int Size(int size) => Math.Clamp(size, 1, 100);
    public static int Offset(int page, int size) => (Math.Clamp(page, 1, 10000) - 1) * Size(size);

    public static int Read(HttpContext? context, string key, int fallback, int max)
    {
        var raw = context?.Request.Query[key];
        if (raw is null || raw.Value.Count == 0) return fallback;
        if (raw.Value.Count != 1 || !int.TryParse(raw.Value[0], out var value) || value < 1)
            throw new ApiException(400, "VALIDATION_ERROR", "Tham số phân trang không hợp lệ.");
        return Math.Min(value, max);
    }

    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, ApplicationDbContext db, int defaultSize = 100)
    {
        var context = db.HttpContext;
        var page = Read(context, "page", 1, 10000);
        var size = Read(context, "size", defaultSize, 100);
        if (context is not null) context.Items["QLSK.PageSize"] = size;
        return query.Skip(Offset(page, size)).Take(size + (context is null ? 0 : 1));
    }
}

// Keep data as the existing JSON array. A lookahead row supplies continuation without COUNT(*).
public sealed class PaginationResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!context.HttpContext.Items.TryGetValue("QLSK.PageSize", out var value) || value is not int size ||
            context.Result is not ObjectResult result || result.Value is null) return;
        var data = result.Value.GetType().GetProperty("DuLieuPhanHoi")?.GetValue(result.Value);
        if (data is null) return;
        var more = false;
        void Trim(IList list)
        {
            more |= list.Count > size;
            while (list.Count > size) list.RemoveAt(list.Count - 1);
        }
        if (data is IList list) Trim(list);
        else foreach (var property in data.GetType().GetProperties())
            if (property.GetValue(data) is IList children) Trim(children);
        context.HttpContext.Response.Headers["X-Has-More"] = more ? "true" : "false";
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
