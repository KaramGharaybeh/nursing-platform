using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Exams;

public class ExamVersion : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int VersionNumber { get; set; }
    public ExamVersionStatus Status { get; set; } = ExamVersionStatus.Draft;
    public int QuestionCount { get; set; }
    public int TotalPoints { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? RetiredAt { get; set; }
    public Exam Exam { get; set; } = null!;
}
