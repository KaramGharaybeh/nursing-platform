using FluentValidation;

namespace NursingPlatform.Application.Recruitment.Commands.CreateContactRequest;

public class CreateContactRequestCommandValidator : AbstractValidator<CreateContactRequestCommand>
{
    public CreateContactRequestCommandValidator()
    {
        RuleFor(x => x.NurseProfileId)
            .NotEmpty()
            .WithMessage("Nurse profile id is required.");
    }
}
