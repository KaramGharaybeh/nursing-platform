namespace NursingPlatform.Application.Identity.Commands.UpdateCurrentUserProfile;

public class UpdateCurrentUserProfileRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
