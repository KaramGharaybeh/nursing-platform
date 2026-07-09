using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.DTOs;

namespace NursingPlatform.Application.Identity.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDetailDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _context;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUser, IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<UserDetailDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = _currentUser.UserId.Value;

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDetailDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                EmailVerified = u.EmailVerified,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                Roles = u.UserRoles
                    .Select(ur => ur.Role.Name)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList(),
                Permissions = u.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID {userId} was not found.");
        }

        return user;
    }
}
