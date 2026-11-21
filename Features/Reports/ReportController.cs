using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Security;

namespace QLSK.Features.Reports;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportController(ReportService reports) : ControllerBase
{
    [HttpGet]
    public Task<ReportDto> Preview([FromQuery] ReportFilter filter, CancellationToken ct) =>
        reports.Preview(AccountAccess.Actor(HttpContext), filter, ct);

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ReportFilter filter, CancellationToken ct)
    {
        var file = await reports.Export(AccountAccess.Actor(HttpContext), filter, ct);
        return File(file.Content, file.ContentType, file.Filename);
    }
}
