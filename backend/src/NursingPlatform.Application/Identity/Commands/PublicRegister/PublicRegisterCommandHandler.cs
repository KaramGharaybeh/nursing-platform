using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.PublicRegister;

public class PublicRegisterCommandHandler : IRequestHandler<PublicRegisterCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger<PublicRegisterCommandHandler> _logger;

    public PublicRegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHashingService passwordHasher,
        IEmailService emailService,
        ILogger<PublicRegisterCommandHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Unit> Handle(PublicRegisterCommand command, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
            return Unit.Value;

        var role = await _context.Roles.SingleOrDefaultAsync(
            r => r.Name == command.RoleName,
            cancellationToken);

        if (role is null)
            throw new InvalidOperationException($"Required public registration role '{command.RoleName}' was not found.");

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

        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        _context.Users.Add(user);

        var rawToken = GenerateToken();
        _context.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeSha256Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (_context.IsUniqueUserEmailViolation(exception))
        {
            return Unit.Value;
        }

        try
        {
            await _emailService.SendVerificationEmailAsync(user.Email, rawToken, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to send verification email to {Email}",
                user.Email);
        }

        return Unit.Value;
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
