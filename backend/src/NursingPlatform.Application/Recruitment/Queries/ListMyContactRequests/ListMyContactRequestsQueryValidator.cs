using FluentValidation;

namespace NursingPlatform.Application.Recruitment.Queries.ListMyContactRequests;

public class ListMyContactRequestsQueryValidator : AbstractValidator<ListMyContactRequestsQuery>
{
    public ListMyContactRequestsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");
        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}
