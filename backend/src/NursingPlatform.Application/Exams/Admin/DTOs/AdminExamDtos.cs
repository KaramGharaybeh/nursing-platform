using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Admin.DTOs;

public class AdminExamCategoryDto
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class AdminExamDto
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public Guid? ExamCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public int DurationMinutes { get; set; }
    public decimal PassingScorePercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsFree { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class AdminExamVersionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int TotalPoints { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? RetiredAt { get; set; }
}

public class AdminExamQuestionDto
{
    public Guid Id { get; set; }
    public Guid ExamVersionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string QuestionType { get; set; } = ExamQuestionType.SingleBestAnswer.ToString();
    public int Points { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<AdminExamAnswerOptionDto> Options { get; set; } = [];
}

public class AdminExamAnswerOptionDto
{
    public Guid Id { get; set; }
    public Guid ExamQuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsActive { get; set; }
}

public class AdminExamVersionValidationDto
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = [];
    public int QuestionCount { get; set; }
    public int TotalPoints { get; set; }
}
