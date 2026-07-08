using MediatR;
using NursingPlatform.Application.Identity.Common;

namespace NursingPlatform.Application.Identity.Commands.Login;

public class LoginCommand : IRequest<AuthResult>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
