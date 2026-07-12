using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Analytics.Common;

internal static class ExamAnalyticsDataLoader
{
    public static async Task<List<ExamAnalyticsSessionRow>> LoadRowsAsync(
        IApplicationDbContext context,
        Guid nurseProfileId,
        ExamAnalyticsFilters filters,
        CancellationToken cancellationToken)
    {
        var sessionQuery = context.ExamSessions
            .Where(s => s.NurseProfileId == nurseProfileId);

        if (filters.From is not null)
        {
            sessionQuery = sessionQuery.Where(s => s.StartedAt >= filters.From.Value);
        }

        if (filters.To is not null)
        {
            sessionQuery = sessionQuery.Where(s => s.StartedAt <= filters.To.Value);
        }

        if (filters.ExamId is not null)
        {
            sessionQuery = sessionQuery.Where(s => s.ExamId == filters.ExamId.Value);
        }

        var sessions = await sessionQuery.ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return [];
        }

        var sessionExamIds = sessions.Select(s => s.ExamId).Distinct().ToList();
        var examQuery = context.Exams.Where(e => sessionExamIds.Contains(e.Id));

        if (filters.CountryId is not null)
        {
            examQuery = examQuery.Where(e => e.CountryId == filters.CountryId.Value);
        }

        if (filters.CategoryId is not null)
        {
            examQuery = examQuery.Where(e => e.ExamCategoryId == filters.CategoryId.Value);
        }

        var exams = await examQuery
            .Select(e => new ExamAnalyticsExamProjection(e.Id, e.Title, e.CountryId, e.ExamCategoryId))
            .ToListAsync(cancellationToken);

        if (exams.Count == 0)
        {
            return [];
        }

        var allowedExamIds = exams.Select(e => e.Id).ToHashSet();
        var countries = await context.Countries
            .Where(c => exams.Select(e => e.CountryId).Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var categoryIds = exams
            .Where(e => e.CategoryId.HasValue)
            .Select(e => e.CategoryId!.Value)
            .Distinct()
            .ToList();

        var categories = await context.ExamCategories
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var examLookup = exams.ToDictionary(e => e.Id);

        return sessions
            .Where(s => allowedExamIds.Contains(s.ExamId))
            .Select(s =>
            {
                var exam = examLookup[s.ExamId];
                var categoryName = exam.CategoryId.HasValue
                    ? categories.GetValueOrDefault(exam.CategoryId.Value)
                    : null;

                return new ExamAnalyticsSessionRow(
                    s.Id,
                    s.ExamId,
                    exam.Title,
                    exam.CountryId,
                    countries.GetValueOrDefault(exam.CountryId, string.Empty),
                    exam.CategoryId,
                    categoryName,
                    s.Status,
                    DateTime.SpecifyKind(s.StartedAt, DateTimeKind.Utc),
                    s.Score,
                    s.MaxScore,
                    s.Percentage,
                    s.Passed,
                    s.CorrectCount,
                    s.QuestionCount);
            })
            .ToList();
    }

    private sealed record ExamAnalyticsExamProjection(
        Guid Id,
        string Title,
        Guid CountryId,
        Guid? CategoryId);
}
