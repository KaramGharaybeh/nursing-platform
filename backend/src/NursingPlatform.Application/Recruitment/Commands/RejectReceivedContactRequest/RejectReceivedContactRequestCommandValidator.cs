using FluentValidation;

namespace NursingPlatform.Application.Recruitment.Commands.RejectReceivedContactRequest;

public class RejectReceivedContactRequestCommandValidator : AbstractValidator<RejectReceivedContactRequestCommand>
{
    public RejectReceivedContactRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Contact request id is required.");
    }
}
