namespace QLSK.Features.Admissions;

public sealed record CheckInRequest(string? Code, string? Method, int? TicketId);
public sealed record AttendeeLookupRequest(string? Query);
public sealed record AttendeeDto(int Id, int TicketId, string Label, string TicketLabel, string? Seat, string Status,
    IReadOnlyList<string> AllowedActions);
public sealed record CheckInDto(string ResultLabel, string? TicketLabel, DateTimeOffset? CheckedInAt, string? Reason = null);
public sealed record CheckInLogDto(int Id, string TicketLabel, string OperatorName, string MethodLabel, DateTimeOffset? CheckedInAt,
    string StatusLabel);
public sealed record StaffGrantRequest(string? Email, int? SessionId, string? Permission);
public sealed record StaffGrantDto(int Id, string Name, string? SessionName, string PermissionLabel, DateTimeOffset? CreatedAt,
    string Status, string StatusLabel, IReadOnlyList<string> AllowedActions);
