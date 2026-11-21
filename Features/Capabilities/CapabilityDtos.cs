namespace QLSK.Features.Capabilities;

public sealed record WorkspaceDto(string Role, string RoleLabel, IReadOnlyList<string> Sections);
public sealed record CapabilityDto(int Version, bool Authenticated, IReadOnlyList<string> Operations, string? CsrfToken,
    WorkspaceDto? Workspace, IReadOnlyList<string> VerificationChannels, IReadOnlyList<string> ReportTypes,
    string? DisplayName, int UnreadNotifications);
