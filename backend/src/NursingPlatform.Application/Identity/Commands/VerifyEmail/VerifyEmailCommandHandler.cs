using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;

namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    private readonly IApplicationDbContext _context;

    public VerifyEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VerifyEmailResponse> Handle(
        VerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256Hash(command.Token);
        var storedToken = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
            throw new InvalidOperationException("Invalid verification token.");

        if (storedToken.UsedAt is not null)
            throw new InvalidOperationException("Verification token has already been used.");

        var now = DateTime.UtcNow;
        if (storedToken.ExpiresAt <= now)
            throw new InvalidOperationException("Verification token has expired.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
            throw new InvalidOperationException("User not found.");

        user.EmailVerified = true;
        storedToken.UsedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponse { Message = "Email verified successfully." };
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
