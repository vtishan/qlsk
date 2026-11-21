namespace QLSK.Features.Reports;

public sealed class ReportFilter
{
    public string? Type { get; init; }
    public int? EventId { get; init; }
    public int? SessionId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public string? Status { get; init; }
    public string? Format { get; init; }
}
public sealed record ReportDto(string Title, IReadOnlyList<string> Columns, IReadOnlyList<string[]> Rows, IReadOnlyList<string> AllowedActions);
public sealed record ReportFile(byte[] Content, string ContentType, string Filename);
