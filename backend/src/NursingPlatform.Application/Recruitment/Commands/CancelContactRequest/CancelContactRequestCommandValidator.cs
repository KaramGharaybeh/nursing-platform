using FluentValidation;

namespace NursingPlatform.Application.Recruitment.Commands.CancelContactRequest;

public class CancelContactRequestCommandValidator : AbstractValidator<CancelContactRequestCommand>
{
    public CancelContactRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Contact request id is required.");
    }
}
