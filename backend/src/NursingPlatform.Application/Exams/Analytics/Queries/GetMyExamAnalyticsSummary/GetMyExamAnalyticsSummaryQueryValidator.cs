using FluentValidation;
using NursingPlatform.Application.Exams.Analytics.Common;

namespace NursingPlatform.Application.Exams.Analytics.Queries.GetMyExamAnalyticsSummary;

public class GetMyExamAnalyticsSummaryQueryValidator : AbstractValidator<GetMyExamAnalyticsSummaryQuery>
{
    public GetMyExamAnalyticsSummaryQueryValidator()
    {
        ExamAnalyticsValidationRules.AddFilterRules(this);
    }
}
