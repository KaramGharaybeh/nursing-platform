namespace NursingPlatform.Domain.PreparationPackages;

public class PackageAnalyticalReportGuidanceItem
{
    private PackageAnalyticalReportGuidanceItem()
    {
    }

    public Guid Id { get; private set; }
    public Guid PackageAnalyticalReportId { get; private set; }
    public Guid ReportingTopicId { get; private set; }
    public PackageReportGuidanceSourceType SourceType { get; private set; }
    public Guid SourceVersionId { get; private set; }
    public string TitleSnapshot { get; private set; } = string.Empty;
    public string? SourceMetadataSnapshot { get; private set; }
    public int SortOrder { get; private set; }

    internal static PackageAnalyticalReportGuidanceItem Create(
        Guid packageAnalyticalReportId,
        Guid reportingTopicId,
        PackageReportGuidanceSourceType sourceType,
        Guid sourceVersionId,
        string titleSnapshot,
        string? sourceMetadataSnapshot,
        int sortOrder)
    {
        RequireNotEmpty(packageAnalyticalReportId, nameof(packageAnalyticalReportId));
        RequireNotEmpty(reportingTopicId, nameof(reportingTopicId));
        RequireNotEmpty(sourceVersionId, nameof(sourceVersionId));

        if (!Enum.IsDefined(sourceType))
        {
            throw new InvalidOperationException("Package report guidance source type is invalid.");
        }

        if (sortOrder < 1)
        {
            throw new InvalidOperationException("Guidance item sort order must be positive.");
        }

        return new PackageAnalyticalReportGuidanceItem
        {
            Id = Guid.NewGuid(),
            PackageAnalyticalReportId = packageAnalyticalReportId,
            ReportingTopicId = reportingTopicId,
            SourceType = sourceType,
            SourceVersionId = sourceVersionId,
            TitleSnapshot = RequireText(titleSnapshot, nameof(titleSnapshot)),
            SourceMetadataSnapshot = NormalizeOptional(sourceMetadataSnapshot),
            SortOrder = sortOrder
        };
    }

    private static void RequireNotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{name} is required.");
        }
    }

    private static string RequireText(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
