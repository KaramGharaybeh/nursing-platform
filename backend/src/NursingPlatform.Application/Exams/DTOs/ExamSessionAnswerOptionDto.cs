namespace NursingPlatform.Application.Exams.DTOs;

public class ExamSessionAnswerOptionDto
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
    public string Text { get; set; } = string.Empty;
}
