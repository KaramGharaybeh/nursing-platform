namespace NursingPlatform.Application.Identity.Commands.UpdateUserRoles;

public class UpdateUserRolesResponse
{
    public Guid UserId { get; init; }
    public List<string> Roles { get; init; } = [];
}
