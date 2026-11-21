using Microsoft.AspNetCore.Mvc;

namespace QLSK.Features.Catalog;

public sealed class EventFilter
{
    public string? Q { get; init; }
    public string? Category { get; init; }
    public string? City { get; init; }
    public string? Venue { get; init; }
    public int? VenueId { get; init; }
    public DateOnly? Date { get; init; }
    public string? Session { get; init; }
    public string? Organizer { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Availability { get; init; }
    public bool Upcoming { get; init; }
    public bool Featured { get; init; }
    public string? Cursor { get; init; }
    public int? Limit { get; init; }
}

public sealed class VenueFilter
{
    public string? Q { get; init; }
    public string? City { get; init; }
    public int? MinCapacity { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Amenities { get; init; }
    public DateOnly? Date { get; init; }
    public string? Cursor { get; init; }
}

public sealed record EventCardDto(int Id, string Title, string Category, string? Image, DateTimeOffset? NextStart,
    string? VenueName, decimal? MinPrice, bool Favorite = false);
public sealed record EventReferenceDto(int Id, string Title);
public sealed record PriceBandDto(decimal Min, decimal Max);
public sealed record VenueFilterOptionDto(int Id, string Name, string Address, decimal Rate);
public sealed record EventFilterOptionsDto(IReadOnlyList<PriceBandDto> PriceBands, IReadOnlyList<string> Cities,
    IReadOnlyList<VenueFilterOptionDto> Venues, IReadOnlyList<string> Categories);
public sealed record VenueReferenceDto(int Id, string Name, string Address);
public sealed record PriceDto(string Name, decimal Amount, DateTimeOffset? From, DateTimeOffset? To, string? Benefits);
public sealed record SessionDto(int Id, string Name, DateTimeOffset? Start, DateTimeOffset? End, VenueReferenceDto Venue,
    EventReferenceDto Event, bool Bookable, IReadOnlyList<PriceDto>? Prices = null);
public sealed record EventDetailDto(int Id, string Title, string? Image, string Category, string Organizer, string? Description,
    string? Participants, string? Program, string? Rules, string? Policy, string Status, IReadOnlyList<SessionDto> Sessions,
    IReadOnlyList<string> AllowedActions, bool Favorite = false);
public sealed record VenueDto(int Id, string Name, string Address, int Capacity, decimal Rate, string? Amenities,
    IReadOnlyList<VenueZoneDto>? Zones = null, string? Image = null);
public sealed record VenueZoneDto(string Name, int Capacity, string Seating);
public sealed record AvailabilityDto(DateTimeOffset? From, DateTimeOffset? To, string Status);
public sealed record BannerDto(int Id, string Title, string? Text, string? Image);
public sealed record CategoryDto(int Id, string Name);
public sealed record PolicyDto(int Id, string Title, string Content);
public sealed record ReviewDto(int Id, int Rating, string? Comment);
public sealed record ReviewRequest(int Rating, string? Comment);
