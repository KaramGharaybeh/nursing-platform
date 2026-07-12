using FluentValidation;

namespace NursingPlatform.Application.Exams.Queries.ListMyExamAttempts;

public class ListMyExamAttemptsQueryValidator : AbstractValidator<ListMyExamAttemptsQuery>
{
    public ListMyExamAttemptsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status is not null);
    }
}
