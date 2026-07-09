using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Infrastructure.Configuration;

namespace NursingPlatform.Infrastructure.Notifications;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(
        string to,
        string token,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("verify-email", token);
        var body = $"<p>Click <a href=\"{url}\">here</a> to verify your email address.</p>";

        _logger.LogInformation("Sending verification email to {Email}", to);
        await SendEmailAsync(to, "Verify your email address", body, cancellationToken);
        _logger.LogInformation("Verification email sent to {Email}", to);
    }

    public async Task SendPasswordResetEmailAsync(
        string to,
        string token,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("reset-password", token);
        var body = $"<p>Click <a href=\"{url}\">here</a> to reset your password.</p>";

        _logger.LogInformation("Sending password reset email to {Email}", to);
        await SendEmailAsync(to, "Reset your password", body, cancellationToken);
        _logger.LogInformation("Password reset email sent to {Email}", to);
    }

    private string BuildUrl(string path, string token)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApplicationUrl))
            throw new InvalidOperationException("ApplicationUrl is not configured.");

        var encodedToken = Uri.EscapeDataString(token);
        return $"{_settings.ApplicationUrl.TrimEnd('/')}/{path}?token={encodedToken}";
    }

    private async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        var socketOptions = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        await client.ConnectAsync(
            _settings.SmtpHost,
            _settings.SmtpPort,
            socketOptions,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
