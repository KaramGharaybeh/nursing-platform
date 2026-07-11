using FluentValidation;

namespace NursingPlatform.Application.Recruitment.Queries.GetMyContactRequest;

public class GetMyContactRequestQueryValidator : AbstractValidator<GetMyContactRequestQuery>
{
    public GetMyContactRequestQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Contact request id is required.");
    }
}
