namespace NursingPlatform.Application.Exams.DTOs;

public class ExamSessionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int RemainingSeconds { get; set; }
    public IReadOnlyList<ExamSessionQuestionDto> Items { get; set; } = [];
}
