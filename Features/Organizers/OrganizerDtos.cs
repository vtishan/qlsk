using QLSK.Common.Http;

namespace QLSK.Features.Organizers;

public sealed class OrganizerSubmission
{
    public string? RegistrationType { get; init; }
    public string? FullName { get; init; }
    public string? IdentityType { get; init; }
    public string? IdentityNumber { get; init; }
    public DateOnly? IssuedDate { get; init; }
    public string? IssuedPlace { get; init; }
    public string? PersonalTax { get; init; }
    public string? OrganizationName { get; init; }
    public string? OrganizationTax { get; init; }
    public string? Representative { get; init; }
    public string? Position { get; init; }
    public string? HeadOffice { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? ContactAddress { get; init; }
    public string? Website { get; init; }
    public string? BankName { get; init; }
    public string? BankBranch { get; init; }
    public string? BankAccount { get; init; }
    public string? BankOwner { get; init; }
    public string? Agreement { get; init; }
    public IFormFile? IdentityFront { get; init; }
    public IFormFile? IdentityBack { get; init; }
    public IFormFile? Passport { get; init; }
    public IFormFile? BusinessLicense { get; init; }
    public IFormFile? Authorization { get; init; }
}

public sealed record ApplicationDto(int Id, string Name, DateTimeOffset? SubmittedAt, DateTimeOffset? DueAt, string Status,
    string StatusLabel, string? Reason = null, IReadOnlyList<HistoryDto>? History = null,
    IReadOnlyList<ApplicationDocumentDto>? Documents = null, IReadOnlyList<string>? AllowedActions = null,
    IReadOnlyList<DetailFieldDto>? Fields = null);
public sealed record ApplicationDocumentDto(int Id, string Label, string Name);
public sealed record PrivateDocumentDto(byte[] Content, string ContentType, string Name);
