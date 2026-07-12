using FluentValidation;
using NursingPlatform.Application.Exams.Analytics.Common;

namespace NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByExam;

public class ListMyExamAnalyticsByExamQueryValidator : AbstractValidator<ListMyExamAnalyticsByExamQuery>
{
    public ListMyExamAnalyticsByExamQueryValidator()
    {
        ExamAnalyticsValidationRules.AddFilterRules(this);
        ExamAnalyticsValidationRules.AddPaginationRules(this);
    }
}
