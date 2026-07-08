using Microsoft.AspNetCore.Authorization;
using NursingPlatform.Application.Authorization;

namespace NursingPlatform.WebApi.Extensions;

public static class AuthorizationExtensions
{
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        string permission)
    {
        return builder.RequireAuthorization(new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permission))
            .Build());
    }
}
