using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Common;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.RotateRefreshToken;

public class RotateRefreshTokenCommandHandler : IRequestHandler<RotateRefreshTokenCommand, AuthResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public RotateRefreshTokenCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResult> Handle(RotateRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256Hash(command.RefreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        if (storedToken.RevokedAt is not null)
        {
            // Reuse detected — revoke all tokens for this user as a security measure
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in userTokens)
                token.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Refresh token has been revoked.");
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        // Revoke old token
        storedToken.RevokedAt = DateTime.UtcNow;

        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.Id == storedToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newTokenHash = ComputeSha256Hash(newRefreshToken);

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            AccessToken = accessToken.Token,
            RefreshToken = newRefreshToken,
            ExpiresAt = accessToken.ExpiresAt
        };
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
