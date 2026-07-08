using FluentValidation;

namespace NursingPlatform.Application.Identity.Commands.RotateRefreshToken;

public class RotateRefreshTokenCommandValidator : AbstractValidator<RotateRefreshTokenCommand>
{
    public RotateRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
