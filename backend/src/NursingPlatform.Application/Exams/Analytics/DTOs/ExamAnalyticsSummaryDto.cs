namespace NursingPlatform.Application.Exams.Analytics.DTOs;

public class ExamAnalyticsSummaryDto
{
    public int AttemptCount { get; set; }
    public int SubmittedCount { get; set; }
    public int ExpiredCount { get; set; }
    public int AbandonedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CountedAttemptCount { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal? PassRatePercentage { get; set; }
    public decimal? AverageScorePercentage { get; set; }
    public decimal? BestScorePercentage { get; set; }
    public decimal? LatestScorePercentage { get; set; }
    public decimal? AverageScore { get; set; }
    public decimal? AverageMaxScore { get; set; }
    public decimal? AverageCorrectCount { get; set; }
    public decimal? AverageQuestionCount { get; set; }
    public DateTime? FirstAttemptStartedAt { get; set; }
    public DateTime? LatestAttemptStartedAt { get; set; }
}
