using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using QLSK.Common.Models;
using QLSK.Common.Security;

namespace QLSK.Common.Exceptions;

public class ApiExceptionMiddleware(RequestDelegate xuLyMiddlewareTiepTheo, ILogger<ApiExceptionMiddleware> boGhiLog)
{
    public async Task Invoke(HttpContext nguCanhHttp)
    {
        try
        {
            await xuLyMiddlewareTiepTheo(nguCanhHttp);
        }
        catch (OperationCanceledException) when (nguCanhHttp.RequestAborted.IsCancellationRequested)
        {
            // The timeout middleware handles its own cancellation; disconnected clients need no response.
        }
        catch (ApiException ngoaiLe) when (!nguCanhHttp.Response.HasStarted)
        {
            nguCanhHttp.Response.StatusCode = ngoaiLe.MaTrangThaiHttp;
            if (ngoaiLe.MaTrangThaiHttp is 429 or 503)
                nguCanhHttp.Response.Headers.RetryAfter = ngoaiLe.MaLoi == "LOGIN_RATE_LIMITED" ? "300" : "5";
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai(ngoaiLe.MaLoi, ngoaiLe.Message));
        }
        catch (OperationCanceledException) when (!nguCanhHttp.Response.HasStarted)
        {
            await HttpProtection.WriteBusy(nguCanhHttp, "UPSTREAM_TIMEOUT", 5);
        }
        catch (SqlException ngoaiLe) when (!nguCanhHttp.Response.HasStarted && ngoaiLe.Number == 1205)
        {
            nguCanhHttp.Response.StatusCode = StatusCodes.Status409Conflict;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("CONCURRENCY_CONFLICT", "Dữ liệu đang thay đổi. Vui lòng thử lại."));
        }
        catch (SqlException ngoaiLe) when (!nguCanhHttp.Response.HasStarted)
        {
            boGhiLog.LogWarning("Database unavailable ({Number}), trace {TraceId}", ngoaiLe.Number, nguCanhHttp.TraceIdentifier);
            await HttpProtection.WriteBusy(nguCanhHttp, "SERVICE_UNAVAILABLE", 5);
        }
        catch (DbUpdateConcurrencyException) when (!nguCanhHttp.Response.HasStarted)
        {
            boGhiLog.LogWarning("Concurrency conflict, trace {TraceId}", nguCanhHttp.TraceIdentifier);
            nguCanhHttp.Response.StatusCode = StatusCodes.Status409Conflict;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("CONCURRENCY_CONFLICT", "Dữ liệu đã thay đổi. Vui lòng tải lại và thử lại."));
        }
        catch (DbUpdateException ngoaiLe) when (!nguCanhHttp.Response.HasStarted)
        {
            boGhiLog.LogWarning("Database conflict ({Type}), trace {TraceId}", ngoaiLe.InnerException?.GetType().Name, nguCanhHttp.TraceIdentifier);
            nguCanhHttp.Response.StatusCode = StatusCodes.Status409Conflict;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("DATABASE_CONFLICT", "Dữ liệu bị xung đột hoặc vi phạm ràng buộc."));
        }
        catch (BadHttpRequestException ngoaiLe) when (!nguCanhHttp.Response.HasStarted)
        {
            nguCanhHttp.Response.StatusCode = ngoaiLe.StatusCode;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("INVALID_REQUEST", "Yêu cầu không hợp lệ hoặc vượt giới hạn cho phép."));
        }
        catch (HttpRequestException) when (!nguCanhHttp.Response.HasStarted)
        {
            await HttpProtection.WriteBusy(nguCanhHttp, "UPSTREAM_UNAVAILABLE", 5);
        }
        catch (Exception ngoaiLe) when (!nguCanhHttp.Response.HasStarted)
        {
            boGhiLog.LogError("Unhandled error ({Type}), trace {TraceId}", ngoaiLe.GetType().Name, nguCanhHttp.TraceIdentifier);
            nguCanhHttp.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("SYSTEM_ERROR", "Có lỗi hệ thống xảy ra."));
        }
    }
}
