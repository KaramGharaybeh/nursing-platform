using FluentValidation;

namespace NursingPlatform.Application.Identity.Commands.UpdateUserRoles;

public class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    private static readonly string[] SupportedBusinessRoles = ["Admin", "Employer", "Expert", "Nurse"];

    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(roleName => SupportedBusinessRoles.Contains(roleName, StringComparer.Ordinal))
            .WithMessage("Role is not supported for user role management.");
    }

    public static IReadOnlyCollection<string> SupportedRoles => SupportedBusinessRoles;
}
