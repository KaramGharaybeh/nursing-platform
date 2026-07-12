namespace NursingPlatform.Application.Exams.DTOs;

public class ExamSessionReviewDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public bool Passed { get; set; }
    public IReadOnlyList<ExamSessionReviewQuestionDto> Items { get; set; } = [];
}
