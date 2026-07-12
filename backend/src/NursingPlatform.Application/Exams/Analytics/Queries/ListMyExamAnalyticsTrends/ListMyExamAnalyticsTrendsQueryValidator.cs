using FluentValidation;
using NursingPlatform.Application.Exams.Analytics.Common;

namespace NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsTrends;

public class ListMyExamAnalyticsTrendsQueryValidator : AbstractValidator<ListMyExamAnalyticsTrendsQuery>
{
    public ListMyExamAnalyticsTrendsQueryValidator()
    {
        ExamAnalyticsValidationRules.AddFilterRules(this);

        RuleFor(x => x.Bucket).IsInEnum();
        RuleFor(x => x.To)
            .Must((query, _) => IsWithinDailyRange(query))
            .WithMessage($"Daily trend range must be {ExamAnalyticsValidationRules.MaxDailyTrendDays} days or less.");
    }

    private static bool IsWithinDailyRange(ListMyExamAnalyticsTrendsQuery query)
    {
        if (query.Bucket != ExamAnalyticsBucket.Day || query.From is null || query.To is null)
        {
            return true;
        }

        return (query.To.Value.Date - query.From.Value.Date).TotalDays <= ExamAnalyticsValidationRules.MaxDailyTrendDays;
    }
}
