using QLSK.Common.Http;

namespace QLSK.Features.Catalog;

public sealed record CatalogChangeRequest(string? Action, string? Name, string? Title, string? Address, int? Capacity,
    decimal? Rate, string? Amenities, int? Position, string? Description, string? Image, string? Target,
    string? Type, string? Content, DateTimeOffset? EffectiveFrom, int? VenueId, string? Seating, string? SeatLayout,
    DateTimeOffset? From, DateTimeOffset? To, string? Reason, string? ExpectedState);
public sealed record CatalogAdminDto(int Id, string? Name = null, string? Title = null, string? Label = null,
    string? Description = null, string? Address = null, int? Capacity = null, decimal? Rate = null,
    string? Amenities = null, string? VenueName = null, string? Seating = null, int? Position = null,
    string? VersionLabel = null, DateTimeOffset? EffectiveFrom = null, DateTimeOffset? From = null, DateTimeOffset? To = null,
    string? Status = null, string? StatusLabel = null, IReadOnlyList<string>? AllowedActions = null,
    IReadOnlyList<DetailFieldDto>? Fields = null);
