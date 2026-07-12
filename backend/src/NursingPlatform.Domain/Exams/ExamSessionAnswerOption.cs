using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Exams;

public class ExamSessionAnswerOption : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ExamSessionQuestionId { get; set; }
    public Guid ExamAnswerOptionId { get; set; }
    public int DisplayOrder { get; set; }
    public string OptionTextSnapshot { get; set; } = string.Empty;
    public bool IsCorrectSnapshot { get; set; }
    public ExamSessionQuestion ExamSessionQuestion { get; set; } = null!;
    public ExamAnswerOption ExamAnswerOption { get; set; } = null!;
}
