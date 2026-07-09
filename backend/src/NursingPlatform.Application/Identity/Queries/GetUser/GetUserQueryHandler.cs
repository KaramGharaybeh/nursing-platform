using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.DTOs;

namespace NursingPlatform.Application.Identity.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDetailDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Where(u => u.Id == request.UserId)
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
            throw new KeyNotFoundException($"User with ID {request.UserId} was not found.");
        }

        return user;
    }
}
