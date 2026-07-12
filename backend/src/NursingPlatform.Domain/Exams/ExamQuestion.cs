using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Exams;

public class ExamQuestion : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ExamVersionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public ExamQuestionType QuestionType { get; set; } = ExamQuestionType.SingleBestAnswer;
    public int Points { get; set; } = 1;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ExamVersion ExamVersion { get; set; } = null!;
}
