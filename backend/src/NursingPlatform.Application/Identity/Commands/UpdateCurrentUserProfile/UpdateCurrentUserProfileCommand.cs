using MediatR;

namespace NursingPlatform.Application.Identity.Commands.UpdateCurrentUserProfile;

public class UpdateCurrentUserProfileCommand : IRequest<UpdateCurrentUserProfileResponse>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
