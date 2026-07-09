namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailRequest
{
    public string Token { get; init; } = string.Empty;
}
