using Microsoft.AspNetCore.Authorization;
using NursingPlatform.Application.Abstractions.Auth;

namespace NursingPlatform.Application.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(
        ICurrentUserService currentUser,
        IPermissionService permissionService)
    {
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return;

        var permissions = await _permissionService.GetUserPermissionsAsync(_currentUser.UserId.Value);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
