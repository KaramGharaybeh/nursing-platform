namespace NursingPlatform.Application.Identity.Commands.RotateRefreshToken;

public class RotateRefreshTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
