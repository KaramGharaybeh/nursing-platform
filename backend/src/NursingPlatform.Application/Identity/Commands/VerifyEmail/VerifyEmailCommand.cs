using MediatR;

namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailCommand : IRequest<VerifyEmailResponse>
{
    public string Token { get; init; } = string.Empty;
}
