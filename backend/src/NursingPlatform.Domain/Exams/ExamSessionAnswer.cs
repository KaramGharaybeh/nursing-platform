using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Exams;

public class ExamSessionAnswer : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ExamSessionQuestionId { get; set; }
    public Guid SelectedExamSessionAnswerOptionId { get; set; }
    public DateTime AnsweredAt { get; set; }
    public ExamSessionQuestion ExamSessionQuestion { get; set; } = null!;
    public ExamSessionAnswerOption SelectedExamSessionAnswerOption { get; set; } = null!;
}
