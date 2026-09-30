using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs;

public sealed class PackageExamSessionStartDto
{
    public ExamSessionDto Session { get; init; } = null!;
    public string Source { get; init; } = string.Empty;
    public Guid EntitlementId { get; init; }
    public Guid IncludedExamId { get; init; }
    public Guid IncludedExamVersionId { get; init; }
    public Guid PreparationPackageOfferId { get; init; }
    public DateTime AccessEndsAt { get; init; }
}
