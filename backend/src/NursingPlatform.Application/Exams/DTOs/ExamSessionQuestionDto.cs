namespace NursingPlatform.Application.Exams.DTOs;

public class ExamSessionQuestionDto
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public Guid? SelectedExamSessionAnswerOptionId { get; set; }
    public IReadOnlyList<ExamSessionAnswerOptionDto> Options { get; set; } = [];
}
