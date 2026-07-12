using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Exams;

public class ExamSessionQuestion : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ExamSessionId { get; set; }
    public Guid ExamQuestionId { get; set; }
    public int DisplayOrder { get; set; }
    public string QuestionTextSnapshot { get; set; } = string.Empty;
    public string? ExplanationSnapshot { get; set; }
    public int Points { get; set; }
    public ExamSession ExamSession { get; set; } = null!;
    public ExamQuestion ExamQuestion { get; set; } = null!;
}
