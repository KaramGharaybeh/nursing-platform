namespace NursingPlatform.Application.Exams.DTOs;

public class ExamSessionReviewQuestionDto
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int Points { get; set; }
    public int PointsEarned { get; set; }
    public Guid? SelectedExamSessionAnswerOptionId { get; set; }
    public Guid CorrectAnswerOptionId { get; set; }
    public IReadOnlyList<ExamSessionReviewOptionDto> Options { get; set; } = [];
}
