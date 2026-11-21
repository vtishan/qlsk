using QLSK.Common.Http;

namespace QLSK.Features.Operations;

public sealed record ScopeOptionDto(int Id, string Name);
public sealed record ScopeSessionDto(int Id, string Name, IReadOnlyDictionary<string, IReadOnlyList<ScopeOptionDto>> Options);
public sealed record ScopeEventDto(int Id, string Name, IReadOnlyList<ScopeSessionDto> Sessions,
    IReadOnlyDictionary<string, IReadOnlyList<ScopeOptionDto>>? Options = null);
public sealed record ScopeDto(IReadOnlyList<ScopeEventDto> Items, IReadOnlyDictionary<string, IReadOnlyList<ScopeOptionDto>> Options);
public sealed record MetricDto(string Label, int Value);
public sealed record ActivityDto(string Label, string StatusLabel, DateTimeOffset? UpdatedAt);
public sealed record DashboardDto(IReadOnlyList<MetricDto> Metrics, IReadOnlyList<ActivityDto> Activity);
public sealed record AdminAccountDto(int Id, string Name, string RoleLabel, string VerificationLabel, string Status,
    string StatusLabel, IReadOnlyList<string>? AllowedActions = null, IReadOnlyList<DetailFieldDto>? Fields = null);
public sealed record AccountAdminRequest(string? Action, string? Reason, string? ExpectedState, int? PermissionGroupId);
public sealed record SettingDto(string Id, string Name, string ValueLabel, DateTimeOffset? UpdatedAt, IReadOnlyList<string> AllowedActions);
public sealed record SettingRequest(string? Key, string? Value, string? Reason);
public sealed record AuditDto(int Id, string ActionLabel, string? ActorName, string SubjectLabel, DateTimeOffset? CreatedAt,
    IReadOnlyList<DetailFieldDto>? Fields = null);
public sealed record ServiceMonitorDto(string Name, string StatusLabel, string Summary, DateTimeOffset? UpdatedAt);
