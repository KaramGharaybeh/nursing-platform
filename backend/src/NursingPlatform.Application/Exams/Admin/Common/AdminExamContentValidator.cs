using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Admin.Common;

internal static class AdminExamContentValidator
{
    public const int MaxDurationMinutes = 480;

    public static async Task<AdminExamVersionValidationDto> ValidateDraftVersionAsync(
        IApplicationDbContext context,
        Guid examId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId, cancellationToken);
        if (exam is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        var version = await context.ExamVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.ExamId == examId, cancellationToken);
        if (version is null)
        {
            throw new KeyNotFoundException("Exam version was not found.");
        }

        var result = new AdminExamVersionValidationDto();

        if (version.Status != ExamVersionStatus.Draft)
        {
            result.Errors.Add("Only draft exam versions can be published.");
        }

        if (exam.Status == ExamStatus.Archived)
        {
            result.Errors.Add("Archived exams cannot publish versions.");
        }

        if (string.IsNullOrWhiteSpace(exam.Title))
        {
            result.Errors.Add("Exam title is required.");
        }

        if (string.IsNullOrWhiteSpace(exam.Slug))
        {
            result.Errors.Add("Exam slug is required.");
        }

        if (exam.DurationMinutes <= 0 || exam.DurationMinutes > MaxDurationMinutes)
        {
            result.Errors.Add("Exam duration must be between 1 and 480 minutes.");
        }

        if (exam.PassingScorePercentage is < 0 or > 100)
        {
            result.Errors.Add("Passing score percentage must be between 0 and 100.");
        }

        var countryExists = await context.Countries.AnyAsync(c => c.Id == exam.CountryId, cancellationToken);
        if (!countryExists)
        {
            result.Errors.Add("Exam country is invalid.");
        }

        if (exam.ExamCategoryId is not null)
        {
            var categoryValid = await context.ExamCategories.AnyAsync(c =>
                    c.Id == exam.ExamCategoryId
                    && c.CountryId == exam.CountryId
                    && c.IsActive,
                cancellationToken);

            if (!categoryValid)
            {
                result.Errors.Add("Exam category must be active and belong to the same country.");
            }
        }

        var questions = await context.ExamQuestions
            .Where(q => q.ExamVersionId == versionId && q.IsActive)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.Id)
            .ToListAsync(cancellationToken);

        if (questions.Count == 0)
        {
            result.Errors.Add("At least one active question is required.");
        }

        if (questions.Any(q => q.QuestionType != ExamQuestionType.SingleBestAnswer))
        {
            result.Errors.Add("Only single-best-answer questions are supported.");
        }

        if (questions.Any(q => q.Points <= 0))
        {
            result.Errors.Add("Every active question must have positive points.");
        }

        var questionIds = questions.Select(q => q.Id).ToList();
        var options = await context.ExamAnswerOptions
            .Where(o => questionIds.Contains(o.ExamQuestionId) && o.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var question in questions)
        {
            var questionOptions = options.Where(o => o.ExamQuestionId == question.Id).ToList();
            if (questionOptions.Count < 2)
            {
                result.Errors.Add($"Question {question.DisplayOrder} must have at least two active options.");
            }

            if (questionOptions.Count(o => o.IsCorrect) != 1)
            {
                result.Errors.Add($"Question {question.DisplayOrder} must have exactly one correct active option.");
            }
        }

        result.QuestionCount = questions.Count;
        result.TotalPoints = questions.Sum(q => q.Points);
        return result;
    }

    public static async Task EnsureCategoryMatchesCountryAsync(
        IApplicationDbContext context,
        Guid? categoryId,
        Guid countryId,
        CancellationToken cancellationToken)
    {
        if (categoryId is null)
        {
            return;
        }

        var categoryMatches = await context.ExamCategories.AnyAsync(c =>
                c.Id == categoryId.Value
                && c.CountryId == countryId
                && c.IsActive,
            cancellationToken);

        if (!categoryMatches)
        {
            throw new InvalidOperationException("Exam category must be active and belong to the same country.");
        }
    }
}
