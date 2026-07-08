namespace NursingPlatform.Application.Identity.Commands.RotateRefreshToken;

public class RotateRefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
