using FluentValidation;

namespace NursingPlatform.Application.Exams.Queries.ListExams;

public class ListExamsQueryValidator : AbstractValidator<ListExamsQuery>
{
    public ListExamsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
