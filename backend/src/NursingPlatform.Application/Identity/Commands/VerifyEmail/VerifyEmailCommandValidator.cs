using FluentValidation;

namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
