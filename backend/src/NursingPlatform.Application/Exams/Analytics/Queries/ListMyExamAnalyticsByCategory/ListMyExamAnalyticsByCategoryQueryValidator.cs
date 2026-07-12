using FluentValidation;
using NursingPlatform.Application.Exams.Analytics.Common;

namespace NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByCategory;

public class ListMyExamAnalyticsByCategoryQueryValidator : AbstractValidator<ListMyExamAnalyticsByCategoryQuery>
{
    public ListMyExamAnalyticsByCategoryQueryValidator()
    {
        ExamAnalyticsValidationRules.AddFilterRules(this);
        ExamAnalyticsValidationRules.AddPaginationRules(this);
    }
}
