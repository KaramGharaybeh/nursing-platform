namespace NursingPlatform.Application.Identity.Commands.ResetPassword;

public class ResetPasswordRequest
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
