using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Domain.Exams;

public class ExamAccessGrant : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public Guid ExamId { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Reason { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;
    public Exam Exam { get; set; } = null!;
}
