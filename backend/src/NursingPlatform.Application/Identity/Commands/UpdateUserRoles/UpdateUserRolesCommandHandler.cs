using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.UpdateUserRoles;

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, UpdateUserRolesResponse>
{
    private static readonly string[] BusinessRoleNames = ["Admin", "Employer", "Expert", "Nurse"];
    private readonly IApplicationDbContext _context;

    public UpdateUserRolesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateUserRolesResponse> Handle(UpdateUserRolesCommand command, CancellationToken cancellationToken)
    {
        if (!BusinessRoleNames.Contains(command.RoleName, StringComparer.Ordinal))
            throw new InvalidOperationException($"Role '{command.RoleName}' is not supported for user role management.");

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
            throw new KeyNotFoundException($"User with ID {command.UserId} was not found.");

        var targetRole = await _context.Roles
            .SingleOrDefaultAsync(r => r.Name == command.RoleName, cancellationToken);

        if (targetRole is null)
            throw new InvalidOperationException($"Role '{command.RoleName}' was not found.");

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        var existingBusinessRoles = user.UserRoles
            .Where(ur => ur.Role is not null && BusinessRoleNames.Contains(ur.Role.Name, StringComparer.Ordinal))
            .ToList();

        foreach (var userRole in existingBusinessRoles)
            user.UserRoles.Remove(userRole);

        if (user.UserRoles.All(ur => ur.RoleId != targetRole.Id))
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = targetRole.Id, Role = targetRole });

        var now = DateTime.UtcNow;
        var activeRefreshTokens = await _context.RefreshTokens
            .Where(token => token.UserId == user.Id && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeRefreshTokens)
            token.RevokedAt = now;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new UpdateUserRolesResponse
        {
            UserId = user.Id,
            Roles = user.UserRoles
                .Select(ur => ur.Role?.Name ?? targetRole.Name)
                .Distinct()
                .OrderBy(roleName => roleName)
                .ToList()
        };
    }
}
