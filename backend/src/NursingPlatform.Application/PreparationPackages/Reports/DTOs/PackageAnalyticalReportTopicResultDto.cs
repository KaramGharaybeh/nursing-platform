namespace NursingPlatform.Application.PreparationPackages.Reports.DTOs;

public sealed class PackageAnalyticalReportTopicResultDto
{
    public Guid ReportingTopicId { get; init; }
    public string TopicName { get; init; } = string.Empty;
    public string? TopicDescription { get; init; }
    public int ScoredQuestionCount { get; init; }
    public int CorrectCount { get; init; }
    public int EarnedPoints { get; init; }
    public int AvailablePoints { get; init; }
    public decimal Percentage { get; init; }
    public int SortOrder { get; init; }
}
