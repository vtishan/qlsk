using Microsoft.EntityFrameworkCore;
using QLSK.Common.Models;

namespace QLSK.Common.Exceptions;

public class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            context.Response.StatusCode = ex.Status;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(ex.Code, ex.Message));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("CONCURRENCY_CONFLICT", "Dữ liệu đã thay đổi. Vui lòng tải lại và thử lại."));
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Database conflict");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("DATABASE_CONFLICT", "Dữ liệu bị xung đột hoặc vi phạm ràng buộc."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("SYSTEM_ERROR", "Có lỗi hệ thống xảy ra."));
        }
    }
}
