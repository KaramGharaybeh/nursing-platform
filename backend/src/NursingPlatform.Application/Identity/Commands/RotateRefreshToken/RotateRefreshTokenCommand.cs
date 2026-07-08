using MediatR;
using NursingPlatform.Application.Identity.Common;

namespace NursingPlatform.Application.Identity.Commands.RotateRefreshToken;

public class RotateRefreshTokenCommand : IRequest<AuthResult>
{
    public string RefreshToken { get; init; } = string.Empty;
}
