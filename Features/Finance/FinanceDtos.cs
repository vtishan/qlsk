using QLSK.Common.Http;

namespace QLSK.Features.Finance;

public sealed record PaymentDto(int Id, string Label, string SourceLabel, decimal Amount, DateTimeOffset? PaidAt,
    string Status, string StatusLabel, IReadOnlyList<DetailFieldDto>? Fields = null);
public sealed record ContractDto(int Id, string Label, string OrganizerName, string FeeLabel, DateTimeOffset? From,
    DateTimeOffset? To, string Status, IReadOnlyList<DetailFieldDto>? Fields = null, string? StatusLabel = null);
public sealed record SettlementDto(int Id, string Label, string EventName, decimal Net, DateTimeOffset? LockedAt, string Status,
    string StatusLabel, IReadOnlyList<string>? AllowedActions = null, IReadOnlyList<DetailFieldDto>? Fields = null);
public sealed record PayoutDto(int Id, string Label, string OrganizerName, decimal Amount, DateTimeOffset? TransferredAt,
    string Status, string StatusLabel, IReadOnlyList<DetailFieldDto>? Fields = null);
public sealed record ContractCreateRequest(int OrganizerId, string? FeeType, decimal FeeValue,
    DateTimeOffset From, DateTimeOffset To);
public sealed record SettlementCreateRequest(int EventId, DateOnly From, DateOnly To);
