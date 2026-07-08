using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Configuration;

namespace NursingPlatform.Infrastructure.Persistence;

public class BootstrapAdminService
{
    private static readonly Guid AdminRoleId = new("091A2B3C-4D5E-46F7-0819-2A3B4C5D6E7F");
    private const string AdminRoleName = "Admin";

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly AdminSettings _settings;
    private readonly ILogger<BootstrapAdminService> _logger;

    public BootstrapAdminService(
        IApplicationDbContext context,
        IPasswordHashingService passwordHasher,
        IOptions<AdminSettings> settings,
        ILogger<BootstrapAdminService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task BootstrapAsync(CancellationToken cancellationToken = default)
    {
        var hasChanges = false;

        var adminRole = await _context.Roles.FirstOrDefaultAsync(
            r => r.Name == AdminRoleName, cancellationToken);

        if (adminRole is null)
        {
            adminRole = new Role
            {
                Id = AdminRoleId,
                Name = AdminRoleName,
                Description = "Administrative access"
            };
            _context.Roles.Add(adminRole);
            hasChanges = true;
            _logger.LogInformation("Created Admin role");
        }

        var adminUser = await _context.Users.FirstOrDefaultAsync(
            u => u.Email == _settings.Email, cancellationToken);

        if (adminUser is null)
        {
            var passwordHash = _passwordHasher.Hash(_settings.Password);

            adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = _settings.Email,
                FirstName = _settings.FirstName,
                LastName = _settings.LastName,
                PasswordHash = passwordHash,
                IsActive = true,
                EmailVerified = true
            };
            _context.Users.Add(adminUser);
            _context.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });
            hasChanges = true;
            _logger.LogInformation("Bootstrapped admin user: {Email}", _settings.Email);
        }
        else
        {
            var hasRole = await _context.UserRoles.AnyAsync(
                ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id,
                cancellationToken);

            if (!hasRole)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                hasChanges = true;
                _logger.LogInformation("Assigned Admin role to existing user: {Email}", _settings.Email);
            }
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
