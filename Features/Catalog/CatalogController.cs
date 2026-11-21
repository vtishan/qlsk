using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLSK.Common.Http;
using QLSK.Common.Security;

namespace QLSK.Features.Catalog;

[ApiController]
[Route("api")]
public sealed class CatalogController(CatalogService service) : ControllerBase
{
    [HttpGet("banners")]
    public Task<PageDto<BannerDto>> Banners(CancellationToken ct) => service.Banners(ct);

    [HttpGet("event-categories")]
    public Task<PageDto<CategoryDto>> Categories(CancellationToken ct) => service.Categories(ct);

    [HttpGet("event-filter-options")]
    public Task<EventFilterOptionsDto> EventFilterOptions(CancellationToken ct) => service.EventFilterOptions(ct);

    [HttpGet("events")]
    public Task<PageDto<EventCardDto>> Events([FromQuery] EventFilter filter, CancellationToken ct) => service.Events(filter, AccountAccess.Current(HttpContext), ct);

    [HttpGet("events/{id:int}")]
    public Task<EventDetailDto> Event(int id, CancellationToken ct) => service.Event(id, AccountAccess.Current(HttpContext), ct);

    [HttpGet("sessions")]
    public Task<PageDto<SessionDto>> Sessions([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] string? city, [FromQuery] string? category, [FromQuery] string? cursor, CancellationToken ct) =>
        service.Sessions(from, to, city, category, cursor, ct);

    [HttpGet("venues")]
    public Task<PageDto<VenueDto>> Venues([FromQuery] VenueFilter filter, CancellationToken ct) => service.Venues(filter, ct);

    [HttpGet("venues/{id:int}")]
    public Task<VenueDto> Venue(int id, CancellationToken ct) => service.Venue(id, ct);

    [HttpGet("venues/{id:int}/availability")]
    public Task<PageDto<AvailabilityDto>> Availability(int id, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        CancellationToken ct) => service.Availability(id, from, to, ct);

    [HttpGet("policies")]
    public Task<PageDto<PolicyDto>> Policies([FromQuery] string? type, CancellationToken ct) => service.Policies(type, ct);

    [HttpGet("events/{id:int}/reviews")]
    public Task<PageDto<ReviewDto>> Reviews(int id, [FromQuery] string? cursor, CancellationToken ct) => service.Reviews(id, cursor, ct);

    [Authorize]
    [HttpPost("events/{id:int}/reviews")]
    public async Task<IActionResult> Review(int id, ReviewRequest request, CancellationToken ct)
    {
        var reviewId = await service.Review(AccountAccess.Actor(HttpContext), id, request, ct);
        return StatusCode(StatusCodes.Status201Created, new CreatedDto(reviewId));
    }
}
