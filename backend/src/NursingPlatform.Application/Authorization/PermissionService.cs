using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;

namespace NursingPlatform.Application.Authorization;

public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;

    public PermissionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
    }
}
