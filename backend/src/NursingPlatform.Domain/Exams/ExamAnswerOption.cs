using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Exams;

public class ExamAnswerOption : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ExamQuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsActive { get; set; } = true;
    public ExamQuestion ExamQuestion { get; set; } = null!;
}
