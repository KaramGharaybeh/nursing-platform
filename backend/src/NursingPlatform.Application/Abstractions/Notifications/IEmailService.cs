namespace NursingPlatform.Application.Abstractions.Notifications;

public interface IEmailService
{
    Task SendVerificationEmailAsync(
        string to,
        string token,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string to,
        string token,
        CancellationToken cancellationToken = default);
}
