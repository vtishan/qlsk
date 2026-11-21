namespace QLSK.Features.Accounts;

public sealed record RegisterRequest(string? Name, string? UserName, string? Email, string? Phone, string? Password, int AcceptedPolicyId);
public sealed record LoginRequest(string? Identifier, string? Password);
public sealed record ResetRequest(string? Identifier);
public sealed record ConfirmResetRequest(string? Token, string? NewPassword);
public sealed record ProfileRequest(string? Name, string? Email, string? Phone);
public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);
public sealed record VerificationRequest(string? Channel);
public sealed record ConfirmVerificationRequest(string? Code);
public sealed record ProfileDto(int Id, string Name, string UserName, string? Email, string? Phone,
    string RoleLabel, string VerificationLabel);
public sealed record NotificationDto(int Id, string Title, string Detail, DateTimeOffset? CreatedAt, bool Read);
