using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;

namespace NursingPlatform.Application.Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHasher;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHashingService passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null || !user.IsActive)
            throw new InvalidOperationException("Invalid password reset request.");

        var tokenHash = ComputeSha256Hash(command.Token);
        var storedToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.UserId == user.Id,
                cancellationToken);

        if (storedToken is null)
            throw new InvalidOperationException("Invalid password reset request.");

        if (storedToken.UsedAt is not null)
            throw new InvalidOperationException("Password reset token has already been used.");

        var now = DateTime.UtcNow;
        if (storedToken.ExpiresAt <= now)
            throw new InvalidOperationException("Password reset token has expired.");

        user.PasswordHash = _passwordHasher.Hash(command.NewPassword);
        storedToken.UsedAt = now;

        var activeRefreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
            refreshToken.RevokedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResponse { Message = "Password has been reset successfully." };
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
