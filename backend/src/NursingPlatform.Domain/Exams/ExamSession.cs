using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Domain.Exams;

public class ExamSession : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public Guid ExamId { get; set; }
    public Guid ExamVersionId { get; set; }
    public ExamSessionStatus Status { get; set; } = ExamSessionStatus.InProgress;
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public bool Passed { get; set; }
    public int CorrectCount { get; set; }
    public int QuestionCount { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;
    public Exam Exam { get; set; } = null!;
    public ExamVersion ExamVersion { get; set; } = null!;

    public bool IsTerminal => Status is ExamSessionStatus.Submitted or ExamSessionStatus.Expired or ExamSessionStatus.Abandoned;

    public static ExamSession Create(
        Guid nurseProfileId,
        Guid examId,
        Guid examVersionId,
        DateTime startedAt,
        int durationMinutes)
    {
        return new ExamSession
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            ExamId = examId,
            ExamVersionId = examVersionId,
            Status = ExamSessionStatus.InProgress,
            StartedAt = startedAt,
            ExpiresAt = startedAt.AddMinutes(durationMinutes)
        };
    }
}
