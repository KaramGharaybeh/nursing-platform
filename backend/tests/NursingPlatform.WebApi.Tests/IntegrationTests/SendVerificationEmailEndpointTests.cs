using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NursingPlatform.Application.Identity.Commands.SendVerificationEmail;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class SendVerificationEmailEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public SendVerificationEmailEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SendVerificationEmail_WithoutToken_Returns401()
    {
        var response = await _client.PostAsync("/api/v1/auth/send-verification-email", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendVerificationEmail_WithValidToken_Returns200()
    {
        var userId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.IsAny<SendVerificationEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendVerificationEmailResponse { Message = "Verification email sent." });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(userId));

        var response = await _client.PostAsync("/api/v1/auth/send-verification-email", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SendVerificationEmailResponse>();
        Assert.NotNull(body);
        Assert.Equal("Verification email sent.", body.Message);
    }

    [Fact]
    public async Task SendVerificationEmail_SendsCommand()
    {
        var userId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.IsAny<SendVerificationEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendVerificationEmailResponse { Message = "Verification email sent." });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(userId));

        await _client.PostAsync("/api/v1/auth/send-verification-email", null);

        _senderMock.Verify(
            s => s.Send(It.IsAny<SendVerificationEmailCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static string CreateJwt(Guid userId)
    {
        const string secret = "test-secret-key-that-is-at-least-32-characters-long";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        {
            KeyId = "nursing-platform-key"
        };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
