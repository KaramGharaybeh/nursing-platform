using FluentValidation;

namespace NursingPlatform.Application.Recruitment.Commands.ApproveReceivedContactRequest;

public class ApproveReceivedContactRequestCommandValidator : AbstractValidator<ApproveReceivedContactRequestCommand>
{
    public ApproveReceivedContactRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Contact request id is required.");
    }
}
