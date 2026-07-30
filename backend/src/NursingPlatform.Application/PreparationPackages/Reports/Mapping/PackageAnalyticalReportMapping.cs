using NursingPlatform.Application.PreparationPackages.Reports.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Reports.Mapping;

internal static class PackageAnalyticalReportMapping
{
    public static PackageAnalyticalReportDto ToDto(PackageAnalyticalReport report)
    {
        return new PackageAnalyticalReportDto
        {
            Id = report.Id,
            ExamSessionId = report.ExamSessionId,
            GeneratedAt = report.GeneratedAt,
            FinalizedSessionStatus = report.FinalizedSessionStatus,
            SubmittedAt = report.SubmittedAt,
            FinalizedAt = report.FinalizedAt,
            Score = report.Score,
            MaxScore = report.MaxScore,
            Percentage = report.Percentage,
            Passed = report.Passed,
            CorrectCount = report.CorrectCount,
            QuestionCount = report.QuestionCount,
            TopicResults = report.TopicResults
                .OrderBy(topic => topic.SortOrder)
                .ThenBy(topic => topic.ReportingTopicId)
                .Select(topic => new PackageAnalyticalReportTopicResultDto
                {
                    ReportingTopicId = topic.ReportingTopicId,
                    TopicName = topic.TopicNameSnapshot,
                    TopicDescription = topic.TopicDescriptionSnapshot,
                    ScoredQuestionCount = topic.ScoredQuestionCount,
                    CorrectCount = topic.CorrectCount,
                    EarnedPoints = topic.EarnedPoints,
                    AvailablePoints = topic.AvailablePoints,
                    Percentage = topic.Percentage,
                    SortOrder = topic.SortOrder
                })
                .ToList(),
            GuidanceItems = report.GuidanceItems
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.ReportingTopicId)
                .ThenBy(item => item.SourceVersionId)
                .Select(item => new PackageAnalyticalReportGuidanceItemDto
                {
                    ReportingTopicId = item.ReportingTopicId,
                    SourceType = item.SourceType.ToString(),
                    SourceVersionId = item.SourceVersionId,
                    Title = item.TitleSnapshot,
                    SourceMetadata = item.SourceMetadataSnapshot,
                    SortOrder = item.SortOrder
                })
                .ToList()
        };
    }
}
