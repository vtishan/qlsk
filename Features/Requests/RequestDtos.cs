using QLSK.Common.Http;

namespace QLSK.Features.Requests;

public sealed record RentalCreateRequest(int VenueId, DateTimeOffset From, DateTimeOffset To, int Capacity, int PolicyId,
    string? EventName, string? EventType, string? Reason, string? Program, string? Equipment, string? SpecialNotes,
    string? Representative, string? Phone, Guid? RequestKey = null);
public sealed record CancelRentalRequest(string? Reason);
public sealed record RefundCreateRequest(int TransactionId, int[]? TicketIds, string? Reason, string? Description);
public sealed record RefundEligibleDto(int Id, string Label, int TransactionId, int OrderId, string OrderLabel, string SessionName, string? Seat);
public sealed record SupportCreateRequest(string? Subject, string? Description, int? EventId, int? TicketId, int? RentalId);
public sealed record RentalDto(int Id, string Title, string VenueName, DateTimeOffset? CreatedAt, DateTimeOffset? From, DateTimeOffset? To,
    decimal Total, decimal DepositAmount, DateTimeOffset? DepositDueAt, string Status, string StatusLabel,
    string? Summary = null, string? Description = null, string? Resolution = null, IReadOnlyList<string>? AllowedActions = null,
    IReadOnlyList<HistoryDto>? History = null, IReadOnlyList<DetailFieldDto>? Fields = null);
public sealed record SupportDto(int Id, string Title, DateTimeOffset? CreatedAt, string Status, string StatusLabel,
    string? Summary, string? Description = null, string? ContextLabel = null, string? Resolution = null,
    string? Response = null, IReadOnlyList<string>? AllowedActions = null, IReadOnlyList<HistoryDto>? History = null);
public sealed record RefundDto(int Id, string Title, DateTimeOffset? CreatedAt, string Status, string StatusLabel, string? OrderLabel,
    decimal? RefundAmount, string? Description, string? TransferStatusLabel, string? Summary, IReadOnlyList<string>? AllowedActions = null,
    IReadOnlyList<HistoryDto>? History = null);
