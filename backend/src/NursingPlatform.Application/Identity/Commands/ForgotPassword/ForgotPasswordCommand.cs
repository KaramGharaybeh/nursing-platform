using MediatR;

namespace NursingPlatform.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<ForgotPasswordResponse>
{
    public string Email { get; init; } = string.Empty;
}
