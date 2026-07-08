namespace NursingPlatform.Application.Abstractions.Auth;

public sealed class AccessTokenResult
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
