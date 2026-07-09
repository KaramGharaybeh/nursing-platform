using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private const string SuccessMessage = "If the email exists, a password reset link has been sent.";

    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ForgotPasswordResponse> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email == command.Email && u.IsActive,
                cancellationToken);

        if (user is null)
            return CreateSuccessResponse();

        var now = DateTime.UtcNow;
        var existingTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in existingTokens)
            token.UsedAt = now;

        var rawToken = GenerateToken();
        var tokenHash = ComputeSha256Hash(rawToken);

        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(1),
            CreatedAt = now
        });

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                rawToken,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to send password reset email to {Email}",
                user.Email);
        }

        return CreateSuccessResponse();
    }

    private static ForgotPasswordResponse CreateSuccessResponse()
    {
        return new ForgotPasswordResponse { Message = SuccessMessage };
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
