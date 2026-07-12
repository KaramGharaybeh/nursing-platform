using FluentValidation;

namespace NursingPlatform.Application.Exams.Analytics.Common;

internal static class ExamAnalyticsValidationRules
{
    public const int MaxDailyTrendDays = 366;

    public static void AddFilterRules<T>(AbstractValidator<T> validator)
        where T : ExamAnalyticsFilters
    {
        validator.RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue);

        validator.RuleFor(x => x.CountryId)
            .Must(BeNullOrNonEmptyGuid)
            .WithMessage("CountryId must not be empty.");

        validator.RuleFor(x => x.CategoryId)
            .Must(BeNullOrNonEmptyGuid)
            .WithMessage("CategoryId must not be empty.");

        validator.RuleFor(x => x.ExamId)
            .Must(BeNullOrNonEmptyGuid)
            .WithMessage("ExamId must not be empty.");
    }

    public static void AddPaginationRules<T>(AbstractValidator<T> validator)
        where T : IExamAnalyticsPaginatedQuery
    {
        validator.RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        validator.RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }

    private static bool BeNullOrNonEmptyGuid(Guid? value) => value is null || value.Value != Guid.Empty;
}

public interface IExamAnalyticsPaginatedQuery
{
    int Page { get; }
    int PageSize { get; }
}
