using MediatR;

namespace NursingPlatform.Application.Identity.Commands.UpdateUserRoles;

public class UpdateUserRolesCommand : IRequest<UpdateUserRolesResponse>
{
    public Guid UserId { get; init; }
    public string RoleName { get; init; } = string.Empty;
}
