namespace NursingPlatform.Application.Identity.Commands.UpdateCurrentUserProfile;

public class UpdateCurrentUserProfileResponse
{
    public string Username { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsProfileComplete { get; init; }
}
