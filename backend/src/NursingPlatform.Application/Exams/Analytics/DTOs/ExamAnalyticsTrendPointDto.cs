namespace NursingPlatform.Application.Exams.Analytics.DTOs;

public class ExamAnalyticsTrendPointDto
{
    public DateTime BucketStart { get; set; }
    public DateTime BucketEnd { get; set; }
    public int AttemptCount { get; set; }
    public int CountedAttemptCount { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal? PassRatePercentage { get; set; }
    public decimal? AverageScorePercentage { get; set; }
    public decimal? BestScorePercentage { get; set; }
}
