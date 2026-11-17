using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OutputCaching;

namespace QLSK.Common.Security;

public sealed class CatalogCacheFilter(IOutputCacheStore store) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();
        var path = context.HttpContext.Request.Path;
        var changesCatalog = path.StartsWithSegments("/api/venues") || path.StartsWithSegments("/api/event-types") ||
            path.StartsWithSegments("/api/admin/catalog/venues") || path.StartsWithSegments("/api/admin/catalog/event-types");
        if (changesCatalog && !HttpMethods.IsGet(context.HttpContext.Request.Method) && result.Exception is null && !result.Canceled)
            await store.EvictByTagAsync("catalog", context.HttpContext.RequestAborted);
    }
}
