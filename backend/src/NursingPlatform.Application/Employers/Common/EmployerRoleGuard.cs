using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;

namespace NursingPlatform.Application.Employers.Common;

public class EmployerRoleGuard
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public EmployerRoleGuard(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> EnsureEmployerAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;
        var isEmployer = await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .AnyAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Employer"), cancellationToken);

        if (!isEmployer)
        {
            throw new ForbiddenAccessException("Employer role is required.");
        }

        return userId;
    }
}
