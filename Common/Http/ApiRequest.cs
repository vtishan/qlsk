using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace QLSK.Common.Http;

internal sealed class ApiException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}

internal static class ApiRequest
{
    public static ApiException Invalid(string message) => new(422, message);
    public static ApiException Conflict(string message) => new(409, message);
    public static ApiException Missing() => new(404, "Không tìm thấy bản ghi trong phạm vi được phép.");
    public static ApiException Denied() => new(403, "Tài khoản chưa có quyền thực hiện thao tác này.");
    public static ApiException SignIn() => new(401, "Vui lòng đăng nhập để tiếp tục.");

    public static async Task Handle(HttpContext context, Func<Task> next)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next();
            return;
        }

        context.Response.Headers.CacheControl = "no-store";
        try { await next(); }
        catch (ApiException error) when (!context.Response.HasStarted)
        {
            await Error(context, error.Status, error.Message);
        }
        catch (AntiforgeryValidationException) when (!context.Response.HasStarted)
        {
            await Error(context, 403, "Phiên biểu mẫu không hợp lệ. Tải lại trang trước khi gửi.");
        }
        catch (DbUpdateException error) when (!context.Response.HasStarted && error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            await Error(context, 409, "Dữ liệu bị trùng hoặc vừa thay đổi. Kiểm tra lại rồi thử lại.");
        }
        catch (SqlException error) when (!context.Response.HasStarted && error.Number is 1205 or 50001)
        {
            await Error(context, 409, "Có yêu cầu khác đang xử lý cùng dữ liệu. Vui lòng thử lại.");
        }
        catch (InvalidDataException) when (!context.Response.HasStarted)
        {
            await Error(context, 422, "Nội dung biểu mẫu hoặc tệp đính kèm không hợp lệ.");
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception error) when (!context.Response.HasStarted)
        {
            context.RequestServices.GetRequiredService<ILogger<ApiException>>()
                .LogError("QLSK API thất bại với loại {ErrorType}, mã {ErrorCode}.", error.GetType().Name,
                    error is SqlException sql ? sql.Number : (int?)null);
            await Error(context, 503, "Dịch vụ QLSK hoặc cơ sở dữ liệu chưa khả dụng.");
        }
    }

    public static async Task ValidateMutation(HttpContext context, Func<Task> next)
    {
        var method = context.Request.Method;
        if (context.Request.Path.StartsWithSegments("/api") &&
            context.GetEndpoint()?.Metadata.GetMetadata<QLSK.Features.Requests.RentalPayOsWebhookAttribute>() is null &&
            (HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method)))
        {
            await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
        }
        await next();
    }

    private static async Task Error(HttpContext context, int code, string message)
    {
        context.Response.StatusCode = code;
        await context.Response.WriteAsJsonAsync(new { message }, context.RequestAborted);
    }

    public static string Required(string? value, int max, string label)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized) || normalized.Length > max)
            throw Invalid($"Nhập {label} trong giới hạn {max} ký tự.");
        return normalized;
    }

    public static string? Optional(string? value, int max, string label) =>
        string.IsNullOrWhiteSpace(value) ? null : Required(value, max, label);

    public static DateTimeOffset? Stamp(DateTime? value) => value.HasValue
        ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)) : null;

    public static DateOnly VietnamDate(DateTime time) => DateOnly.FromDateTime(time.AddHours(7));

    public static int After(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return 0;
        if (!int.TryParse(cursor, out var id) || id < 1) throw Invalid("Vị trí phân trang không hợp lệ.");
        return id;
    }

    public static PageDto<T> Page<T>(IReadOnlyList<T> rows, int size, Func<T, int> key) =>
        new(rows.Take(size).ToArray(), rows.Count > size
            ? key(rows[size - 1]).ToString(System.Globalization.CultureInfo.InvariantCulture) : null);
}

public sealed record PageDto<T>(IReadOnlyList<T> Items, string? NextCursor = null);
public sealed record CreatedDto(int Id);
public sealed record HistoryDto(DateTimeOffset? At, string? Status, string? StatusLabel, string? ActorName, string? Note);
public sealed record DetailFieldDto(string Label, string? Value);
public sealed record DecisionRequest(string? Action, string? Reason, string? ExpectedState);
