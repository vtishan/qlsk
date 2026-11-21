using QLSK.Common.Http;

namespace QLSK.Features.Events;

public sealed record EventCreateRequest(string? Name, int CategoryId, string? Description, string? Program,
    string? Participants, string? Rules, string? Policy, string? Image, string? AllowRefund);
public sealed record EventVersionRequest(string? ChangeType, string? Name, string? Description, string? Reason);
public sealed record SessionChangeRequest(int VersionId, string? Name, int VenueId, int? ZoneId, DateTimeOffset? From,
    DateTimeOffset? To, DateTimeOffset? SalesFrom, DateTimeOffset? SalesTo, int? Capacity, int? RentalId = null);
public sealed record ConfirmedRentalOptionDto(int Id, int VenueId, string Venue, DateTimeOffset? From,
    DateTimeOffset? To, int? Capacity, decimal RentalTotal, decimal Deposit);
public sealed record TicketTypeChangeRequest(int VersionId, int ZoneId, string? Type, decimal? Price, int? Quantity,
    DateTimeOffset? SalesFrom, DateTimeOffset? SalesTo, string? Benefits);
public sealed record EventWorkDto(int Id, string Name, string? OrganizerName, string Status, string StatusLabel,
    DateTimeOffset? NextStart = null, int? VersionId = null, string? ChangeTypeLabel = null, DateTimeOffset? SubmittedAt = null,
    string? Description = null, IReadOnlyList<string>? AllowedActions = null, IReadOnlyList<DetailFieldDto>? Fields = null,
    IReadOnlyList<HistoryDto>? History = null);
public sealed record VersionDto(int Id, string Name, string ChangeTypeLabel, string Status, string StatusLabel, DateTimeOffset? UpdatedAt,
    string? Description, IReadOnlyList<string>? AllowedActions = null);
public sealed record SessionWorkDto(int Id, string Name, string VenueName, DateTimeOffset? From, DateTimeOffset? To, int Capacity,
    string Status, IReadOnlyList<string>? AllowedActions = null, IReadOnlyList<DetailFieldDto>? Fields = null,
    IReadOnlyDictionary<string, object?>? FormValues = null);
public sealed record TicketTypeWorkDto(int Id, string Name, string Zone, decimal Price, DateTimeOffset? SalesFrom,
    DateTimeOffset? SalesTo, IReadOnlyList<string>? AllowedActions = null, IReadOnlyList<DetailFieldDto>? Fields = null,
    IReadOnlyDictionary<string, object?>? FormValues = null);
internal sealed record EventContent(string Name, int CategoryId, string? Description, string? Program, string? Participants,
    string? Rules, string? Policy, string? Image, bool AllowRefund);
