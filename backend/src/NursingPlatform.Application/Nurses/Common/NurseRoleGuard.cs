using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;

namespace NursingPlatform.Application.Nurses.Common;

public class NurseRoleGuard
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public NurseRoleGuard(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> EnsureCurrentUserIsNurseAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;
        var isNurse = await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .AnyAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Nurse"), cancellationToken);

        if (!isNurse)
        {
            throw new ForbiddenAccessException("Nurse role is required.");
        }

        return userId;
    }
}
