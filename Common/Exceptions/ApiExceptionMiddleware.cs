using Microsoft.EntityFrameworkCore;
using QLSK.Common.Models;

namespace QLSK.Common.Exceptions;

public class ApiExceptionMiddleware(RequestDelegate xuLyMiddlewareTiepTheo, ILogger<ApiExceptionMiddleware> boGhiLog)
{
    public async Task Invoke(HttpContext nguCanhHttp)
    {
        try
        {
            await xuLyMiddlewareTiepTheo(nguCanhHttp);
        }
        catch (ApiException ngoaiLe)
        {
            nguCanhHttp.Response.StatusCode = ngoaiLe.MaTrangThaiHttp;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai(ngoaiLe.MaLoi, ngoaiLe.Message));
        }
        catch (DbUpdateConcurrencyException ngoaiLe)
        {
            boGhiLog.LogWarning(ngoaiLe, "Concurrency conflict");
            nguCanhHttp.Response.StatusCode = StatusCodes.Status409Conflict;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("CONCURRENCY_CONFLICT", "Dữ liệu đã thay đổi. Vui lòng tải lại và thử lại."));
        }
        catch (DbUpdateException ngoaiLe)
        {
            boGhiLog.LogWarning(ngoaiLe, "Database conflict");
            nguCanhHttp.Response.StatusCode = StatusCodes.Status409Conflict;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("DATABASE_CONFLICT", "Dữ liệu bị xung đột hoặc vi phạm ràng buộc."));
        }
        catch (Exception ngoaiLe)
        {
            boGhiLog.LogError(ngoaiLe, "Unhandled error");
            nguCanhHttp.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await nguCanhHttp.Response.WriteAsJsonAsync(ApiResponse.TaoPhanHoiThatBai("SYSTEM_ERROR", "Có lỗi hệ thống xảy ra."));
        }
    }
}
