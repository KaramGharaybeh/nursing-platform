using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.SendVerificationEmail;

public class SendVerificationEmailCommandHandler : IRequestHandler<SendVerificationEmailCommand, SendVerificationEmailResponse>
{
    private const string SuccessMessage = "Verification email sent.";

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendVerificationEmailCommandHandler> _logger;

    public SendVerificationEmailCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ILogger<SendVerificationEmailCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SendVerificationEmailResponse> Handle(
        SendVerificationEmailCommand command,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == _currentUserService.UserId.Value,
                cancellationToken);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found.");

        if (user.EmailVerified)
            return new SendVerificationEmailResponse { Message = SuccessMessage };

        var now = DateTime.UtcNow;
        var existingTokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == user.Id && t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in existingTokens)
            token.UsedAt = now;

        var rawToken = GenerateToken();
        var tokenHash = ComputeSha256Hash(rawToken);

        _context.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(24),
            CreatedAt = now
        });

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendVerificationEmailAsync(
                user.Email,
                rawToken,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to send verification email to {Email}",
                user.Email);
            throw new InvalidOperationException("Failed to send verification email.");
        }

        return new SendVerificationEmailResponse { Message = SuccessMessage };
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
