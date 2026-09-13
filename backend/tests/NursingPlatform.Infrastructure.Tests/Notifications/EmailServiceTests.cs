using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NursingPlatform.Infrastructure.Configuration;
using NursingPlatform.Infrastructure.Notifications;

namespace NursingPlatform.Infrastructure.Tests.Notifications;

public class EmailServiceTests
{
    [Theory]
    [InlineData("verify-email", "http://localhost:4200/auth/verify-email/confirm?token=token%20value")]
    [InlineData("reset-password", "http://localhost:4200/auth/reset-password?token=token%20value")]
    public void BuildUrl_UsesCanonicalFrontendAuthRoutes(string path, string expectedUrl)
    {
        var service = new EmailService(
            Options.Create(new EmailSettings
            {
                ApplicationUrl = "http://localhost:4200/",
                FromAddress = "noreply@nursingplatform.dev",
                FromName = "Nursing Platform",
                SmtpHost = "localhost",
                SmtpPort = 1025
            }),
            Mock.Of<ILogger<EmailService>>());
        var method = typeof(EmailService).GetMethod("BuildUrl", BindingFlags.Instance | BindingFlags.NonPublic);

        var url = Assert.IsType<string>(method?.Invoke(service, [path, "token value"]));

        Assert.Equal(expectedUrl, url);
    }
}
