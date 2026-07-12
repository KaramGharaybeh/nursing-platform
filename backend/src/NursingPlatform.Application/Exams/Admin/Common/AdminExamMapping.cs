using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Admin.Common;

internal static class AdminExamMapping
{
    public static AdminExamCategoryDto ToCategoryDto(ExamCategory category, string countryName)
    {
        return new AdminExamCategoryDto
        {
            Id = category.Id,
            CountryId = category.CountryId,
            CountryName = countryName,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }

    public static AdminExamDto ToExamDto(Exam exam, string countryName, string? categoryName)
    {
        return new AdminExamDto
        {
            Id = exam.Id,
            CountryId = exam.CountryId,
            CountryName = countryName,
            ExamCategoryId = exam.ExamCategoryId,
            CategoryName = categoryName,
            Title = exam.Title,
            Slug = exam.Slug,
            Description = exam.Description,
            Instructions = exam.Instructions,
            DurationMinutes = exam.DurationMinutes,
            PassingScorePercentage = exam.PassingScorePercentage,
            Status = exam.Status.ToString(),
            IsFree = exam.IsFree,
            PublishedAt = exam.PublishedAt
        };
    }

    public static AdminExamVersionDto ToVersionDto(ExamVersion version)
    {
        return new AdminExamVersionDto
        {
            Id = version.Id,
            ExamId = version.ExamId,
            VersionNumber = version.VersionNumber,
            Status = version.Status.ToString(),
            QuestionCount = version.QuestionCount,
            TotalPoints = version.TotalPoints,
            PublishedAt = version.PublishedAt,
            RetiredAt = version.RetiredAt
        };
    }

    public static AdminExamQuestionDto ToQuestionDto(
        ExamQuestion question,
        IEnumerable<ExamAnswerOption> options)
    {
        return new AdminExamQuestionDto
        {
            Id = question.Id,
            ExamVersionId = question.ExamVersionId,
            QuestionText = question.QuestionText,
            Explanation = question.Explanation,
            QuestionType = question.QuestionType.ToString(),
            Points = question.Points,
            DisplayOrder = question.DisplayOrder,
            IsActive = question.IsActive,
            Options = options
                .OrderBy(o => o.DisplayOrder)
                .ThenBy(o => o.Id)
                .Select(ToOptionDto)
                .ToList()
        };
    }

    public static AdminExamAnswerOptionDto ToOptionDto(ExamAnswerOption option)
    {
        return new AdminExamAnswerOptionDto
        {
            Id = option.Id,
            ExamQuestionId = option.ExamQuestionId,
            OptionText = option.OptionText,
            DisplayOrder = option.DisplayOrder,
            IsCorrect = option.IsCorrect,
            IsActive = option.IsActive
        };
    }
}
