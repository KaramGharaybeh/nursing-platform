namespace NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs;

public sealed class PackageExamSessionStateDto
{
    public bool HasSession { get; init; }
    public Guid? SessionId { get; init; }
    public Guid? ExamId { get; init; }
    public string? Status { get; init; }
    public DateTime? ExpiresAt { get; init; }
}
