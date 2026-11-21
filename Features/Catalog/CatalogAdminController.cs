using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;

namespace QLSK.Features.Catalog;

[ApiController]
[Authorize]
[Route("api/admin/catalog")]
public sealed class CatalogAdminController(CatalogAdminService service) : ControllerBase
{
    [HttpGet("{resource}")]
    public Task<PageDto<CatalogAdminDto>> List(string resource, string? status, string? cursor, CancellationToken ct) =>
        service.List(AccountAccess.Actor(HttpContext), resource, status, cursor, ct);

    [HttpGet("{resource}/{id:int}")]
    public Task<CatalogAdminDto> Detail(string resource, int id, CancellationToken ct) =>
        service.Detail(AccountAccess.Actor(HttpContext), resource, id, ct);

    [HttpPost("{resource}")]
    public async Task<IActionResult> Create(string resource, CatalogChangeRequest request, CancellationToken ct) =>
        StatusCode(201, new CreatedDto(await service.Create(AccountAccess.Actor(HttpContext), resource, request, ct)));

    [HttpPut("{resource}/{id:int}")]
    public Task<CatalogAdminDto> Update(string resource, int id, CatalogChangeRequest request, CancellationToken ct) =>
        service.Update(AccountAccess.Actor(HttpContext), resource, id, request, ct);
}
