namespace NursingPlatform.Application.Exams.DTOs;

public class ExamCatalogItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int DurationMinutes { get; set; }
    public int QuestionCount { get; set; }
    public decimal PassingScorePercentage { get; set; }
    public bool IsFree { get; set; }
    public bool CanStart { get; set; }
}
