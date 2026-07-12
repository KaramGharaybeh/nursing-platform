using NursingPlatform.Application.Exams.Analytics.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Analytics.Common;

internal static class ExamAnalyticsMetricCalculator
{
    public static ExamAnalyticsMetricSnapshot Calculate(IEnumerable<ExamAnalyticsSessionRow> rows)
    {
        var items = rows.ToList();
        var counted = items
            .Where(IsCountedStatus)
            .ToList();

        var passedCount = counted.Count(s => s.Passed);
        var failedCount = counted.Count - passedCount;

        return new ExamAnalyticsMetricSnapshot
        {
            AttemptCount = items.Count,
            SubmittedCount = items.Count(s => s.Status == ExamSessionStatus.Submitted),
            ExpiredCount = items.Count(s => s.Status == ExamSessionStatus.Expired),
            AbandonedCount = items.Count(s => s.Status == ExamSessionStatus.Abandoned),
            InProgressCount = items.Count(s => s.Status == ExamSessionStatus.InProgress),
            CountedAttemptCount = counted.Count,
            PassedCount = passedCount,
            FailedCount = failedCount,
            PassRatePercentage = counted.Count == 0 ? null : passedCount * 100m / counted.Count,
            AverageScorePercentage = counted.Count == 0 ? null : counted.Average(s => s.Percentage),
            BestScorePercentage = counted.Count == 0 ? null : counted.Max(s => s.Percentage),
            LatestScorePercentage = counted
                .OrderByDescending(s => s.StartedAt)
                .ThenByDescending(s => s.SessionId)
                .Select(s => (decimal?)s.Percentage)
                .FirstOrDefault(),
            AverageScore = counted.Count == 0 ? null : counted.Average(s => (decimal)s.Score),
            AverageMaxScore = counted.Count == 0 ? null : counted.Average(s => (decimal)s.MaxScore),
            AverageCorrectCount = counted.Count == 0 ? null : counted.Average(s => (decimal)s.CorrectCount),
            AverageQuestionCount = counted.Count == 0 ? null : counted.Average(s => (decimal)s.QuestionCount),
            FirstAttemptStartedAt = items.Count == 0 ? null : items.Min(s => s.StartedAt),
            LatestAttemptStartedAt = items.Count == 0 ? null : items.Max(s => s.StartedAt)
        };
    }

    public static ExamAnalyticsTrendPointDto CreateTrendPoint(
        DateTime bucketStart,
        DateTime bucketEnd,
        IEnumerable<ExamAnalyticsSessionRow> rows)
    {
        var snapshot = Calculate(rows);

        return new ExamAnalyticsTrendPointDto
        {
            BucketStart = bucketStart,
            BucketEnd = bucketEnd,
            AttemptCount = snapshot.AttemptCount,
            CountedAttemptCount = snapshot.CountedAttemptCount,
            PassedCount = snapshot.PassedCount,
            FailedCount = snapshot.FailedCount,
            PassRatePercentage = snapshot.PassRatePercentage,
            AverageScorePercentage = snapshot.AverageScorePercentage,
            BestScorePercentage = snapshot.BestScorePercentage
        };
    }

    public static (DateTime Start, DateTime End) GetBucket(DateTime startedAt, ExamAnalyticsBucket bucket)
    {
        var utc = DateTime.SpecifyKind(startedAt, DateTimeKind.Utc);

        return bucket switch
        {
            ExamAnalyticsBucket.Day => (utc.Date, utc.Date.AddDays(1)),
            ExamAnalyticsBucket.Week => GetWeekBucket(utc),
            ExamAnalyticsBucket.Month => (new DateTime(utc.Year, utc.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(utc.Year, utc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
            _ => throw new ArgumentOutOfRangeException(nameof(bucket), bucket, "Unsupported analytics bucket.")
        };
    }

    public static void ApplySnapshot(ExamAnalyticsSummaryDto dto, ExamAnalyticsMetricSnapshot snapshot)
    {
        dto.AttemptCount = snapshot.AttemptCount;
        dto.SubmittedCount = snapshot.SubmittedCount;
        dto.ExpiredCount = snapshot.ExpiredCount;
        dto.AbandonedCount = snapshot.AbandonedCount;
        dto.InProgressCount = snapshot.InProgressCount;
        dto.CountedAttemptCount = snapshot.CountedAttemptCount;
        dto.PassedCount = snapshot.PassedCount;
        dto.FailedCount = snapshot.FailedCount;
        dto.PassRatePercentage = snapshot.PassRatePercentage;
        dto.AverageScorePercentage = snapshot.AverageScorePercentage;
        dto.BestScorePercentage = snapshot.BestScorePercentage;
        dto.LatestScorePercentage = snapshot.LatestScorePercentage;
        dto.AverageScore = snapshot.AverageScore;
        dto.AverageMaxScore = snapshot.AverageMaxScore;
        dto.AverageCorrectCount = snapshot.AverageCorrectCount;
        dto.AverageQuestionCount = snapshot.AverageQuestionCount;
        dto.FirstAttemptStartedAt = snapshot.FirstAttemptStartedAt;
        dto.LatestAttemptStartedAt = snapshot.LatestAttemptStartedAt;
    }

    public static void ApplySnapshot(ExamAnalyticsByExamDto dto, ExamAnalyticsMetricSnapshot snapshot)
    {
        dto.AttemptCount = snapshot.AttemptCount;
        dto.SubmittedCount = snapshot.SubmittedCount;
        dto.ExpiredCount = snapshot.ExpiredCount;
        dto.AbandonedCount = snapshot.AbandonedCount;
        dto.InProgressCount = snapshot.InProgressCount;
        dto.CountedAttemptCount = snapshot.CountedAttemptCount;
        dto.PassedCount = snapshot.PassedCount;
        dto.FailedCount = snapshot.FailedCount;
        dto.PassRatePercentage = snapshot.PassRatePercentage;
        dto.AverageScorePercentage = snapshot.AverageScorePercentage;
        dto.BestScorePercentage = snapshot.BestScorePercentage;
        dto.LatestScorePercentage = snapshot.LatestScorePercentage;
        dto.AverageScore = snapshot.AverageScore;
        dto.AverageMaxScore = snapshot.AverageMaxScore;
        dto.AverageCorrectCount = snapshot.AverageCorrectCount;
        dto.AverageQuestionCount = snapshot.AverageQuestionCount;
        dto.FirstAttemptStartedAt = snapshot.FirstAttemptStartedAt;
        dto.LatestAttemptStartedAt = snapshot.LatestAttemptStartedAt;
    }

    public static void ApplySnapshot(ExamAnalyticsByCategoryDto dto, ExamAnalyticsMetricSnapshot snapshot)
    {
        dto.AttemptCount = snapshot.AttemptCount;
        dto.SubmittedCount = snapshot.SubmittedCount;
        dto.ExpiredCount = snapshot.ExpiredCount;
        dto.AbandonedCount = snapshot.AbandonedCount;
        dto.InProgressCount = snapshot.InProgressCount;
        dto.CountedAttemptCount = snapshot.CountedAttemptCount;
        dto.PassedCount = snapshot.PassedCount;
        dto.FailedCount = snapshot.FailedCount;
        dto.PassRatePercentage = snapshot.PassRatePercentage;
        dto.AverageScorePercentage = snapshot.AverageScorePercentage;
        dto.BestScorePercentage = snapshot.BestScorePercentage;
        dto.LatestScorePercentage = snapshot.LatestScorePercentage;
        dto.AverageScore = snapshot.AverageScore;
        dto.AverageMaxScore = snapshot.AverageMaxScore;
        dto.AverageCorrectCount = snapshot.AverageCorrectCount;
        dto.AverageQuestionCount = snapshot.AverageQuestionCount;
        dto.FirstAttemptStartedAt = snapshot.FirstAttemptStartedAt;
        dto.LatestAttemptStartedAt = snapshot.LatestAttemptStartedAt;
    }

    private static bool IsCountedStatus(ExamAnalyticsSessionRow row) =>
        row.Status is ExamSessionStatus.Submitted or ExamSessionStatus.Expired;

    private static (DateTime Start, DateTime End) GetWeekBucket(DateTime utc)
    {
        var offset = ((int)utc.DayOfWeek + 6) % 7;
        var start = utc.Date.AddDays(-offset);
        return (start, start.AddDays(7));
    }
}

internal sealed class ExamAnalyticsMetricSnapshot
{
    public int AttemptCount { get; init; }
    public int SubmittedCount { get; init; }
    public int ExpiredCount { get; init; }
    public int AbandonedCount { get; init; }
    public int InProgressCount { get; init; }
    public int CountedAttemptCount { get; init; }
    public int PassedCount { get; init; }
    public int FailedCount { get; init; }
    public decimal? PassRatePercentage { get; init; }
    public decimal? AverageScorePercentage { get; init; }
    public decimal? BestScorePercentage { get; init; }
    public decimal? LatestScorePercentage { get; init; }
    public decimal? AverageScore { get; init; }
    public decimal? AverageMaxScore { get; init; }
    public decimal? AverageCorrectCount { get; init; }
    public decimal? AverageQuestionCount { get; init; }
    public DateTime? FirstAttemptStartedAt { get; init; }
    public DateTime? LatestAttemptStartedAt { get; init; }
}

internal sealed record ExamAnalyticsSessionRow(
    Guid SessionId,
    Guid ExamId,
    string ExamTitle,
    Guid CountryId,
    string CountryName,
    Guid? CategoryId,
    string? CategoryName,
    ExamSessionStatus Status,
    DateTime StartedAt,
    int Score,
    int MaxScore,
    decimal Percentage,
    bool Passed,
    int CorrectCount,
    int QuestionCount);
