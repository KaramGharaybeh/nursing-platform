using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Exams;

public class Exam : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public Guid? ExamCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public int DurationMinutes { get; set; }
    public decimal PassingScorePercentage { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.Draft;
    public bool IsFree { get; set; } = true;
    public DateTime? PublishedAt { get; set; }
    public Country Country { get; set; } = null!;
    public ExamCategory? ExamCategory { get; set; }
}
