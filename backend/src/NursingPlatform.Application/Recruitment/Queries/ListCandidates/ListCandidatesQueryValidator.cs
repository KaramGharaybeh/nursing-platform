using FluentValidation;

namespace NursingPlatform.Application.Recruitment.Queries.ListCandidates;

public class ListCandidatesQueryValidator : AbstractValidator<ListCandidatesQuery>
{
    public ListCandidatesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100.");
    }
}
