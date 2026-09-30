using FluentValidation;

namespace NursingPlatform.Application.Identity.Commands.PublicRegister;

public class PublicRegisterCommandValidator : AbstractValidator<PublicRegisterCommand>
{
    public PublicRegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).Matches("[A-Z]").Matches("[0-9]");
        RuleFor(x => x.RoleName)
            .Must(PublicRegistrationRoleNames.IsAllowed)
            .WithMessage("Public registration role is not supported.");
    }
}
