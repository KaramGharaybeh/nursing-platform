using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.PreparationPackages.Reports.DTOs;

public sealed class PackageAnalyticalReportDto
{
    public Guid Id { get; init; }
    public Guid ExamSessionId { get; init; }
    public DateTime GeneratedAt { get; init; }
    public ExamSessionStatus FinalizedSessionStatus { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime FinalizedAt { get; init; }
    public int Score { get; init; }
    public int MaxScore { get; init; }
    public decimal Percentage { get; init; }
    public bool Passed { get; init; }
    public int CorrectCount { get; init; }
    public int QuestionCount { get; init; }
    public IReadOnlyList<PackageAnalyticalReportTopicResultDto> TopicResults { get; init; } = [];
    public IReadOnlyList<PackageAnalyticalReportGuidanceItemDto> GuidanceItems { get; init; } = [];
}
