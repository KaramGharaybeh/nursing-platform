namespace NursingPlatform.Application.PreparationPackages.Reports.DTOs;

public sealed class PackageAnalyticalReportGuidanceItemDto
{
    public Guid ReportingTopicId { get; init; }
    public string SourceType { get; init; } = string.Empty;
    public Guid SourceVersionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? SourceMetadata { get; init; }
    public int SortOrder { get; init; }
}
