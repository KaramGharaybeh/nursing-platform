using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Identity.Commands.Login;
using NursingPlatform.Application.Identity.Commands.RotateRefreshToken;
using NursingPlatform.Application.Identity.Common;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class AuthEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public AuthEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var expected = new AuthResult
        {
            AccessToken = "access-token-value",
            RefreshToken = "refresh-token-value",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "user@test.com", password = "ValidPass1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.NotNull(body);
        Assert.Equal(expected.AccessToken, body.AccessToken);
        Assert.Equal(expected.RefreshToken, body.RefreshToken);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "wrong@test.com", password = "WrongPass1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Unauthorized", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Login_ValidationError_Returns400WithErrors()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException(
                new[]
                {
                    new FluentValidation.Results.ValidationFailure("Email", "'Email' must not be empty."),
                    new FluentValidation.Results.ValidationFailure("Password", "'Password' must not be empty.")
                }));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "", password = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.True(body.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Email", out _));
        Assert.True(errors.TryGetProperty("Password", out _));
    }

    [Fact]
    public async Task RefreshToken_ValidToken_Returns200WithTokens()
    {
        var expected = new AuthResult
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<RotateRefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new { refreshToken = "valid-refresh-token" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.NotNull(body);
        Assert.Equal(expected.AccessToken, body.AccessToken);
        Assert.Equal(expected.RefreshToken, body.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_Returns401()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<RotateRefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid refresh token."));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new { refreshToken = "invalid-token" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Unauthorized", body.GetProperty("title").GetString());
    }
}
