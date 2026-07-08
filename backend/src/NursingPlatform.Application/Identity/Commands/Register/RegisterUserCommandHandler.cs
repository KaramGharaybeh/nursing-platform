using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHasher;

    public RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHashingService passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
            throw new InvalidOperationException($"Email '{command.Email}' is already registered.");

        var validRoleIds = await _context.Roles
            .Where(r => command.RoleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var invalidRoleIds = command.RoleIds.Except(validRoleIds).ToList();
        if (invalidRoleIds.Count != 0)
            throw new InvalidOperationException($"Role(s) not found: {string.Join(", ", invalidRoleIds)}");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = _passwordHasher.Hash(command.Password),
            FirstName = command.FirstName,
            LastName = command.LastName,
            IsActive = true,
            EmailVerified = false
        };

        foreach (var roleId in validRoleIds)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
