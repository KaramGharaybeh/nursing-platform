using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Identity.Commands.VerifyEmail;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class VerifyEmailEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public VerifyEmailEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_Returns200WithoutExposingToken()
    {
        const string rawToken = "raw-token";
        _senderMock
            .Setup(s => s.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerifyEmailResponse { Message = "Email verified successfully." });

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/verify-email",
            new { token = rawToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(rawToken, json, StringComparison.OrdinalIgnoreCase);
        var body = JsonSerializer.Deserialize<VerifyEmailResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Equal("Email verified successfully.", body.Message);
    }

    [Fact]
    public async Task VerifyEmail_SendsCommandWithRequestToken()
    {
        const string rawToken = "raw-token";
        _senderMock
            .Setup(s => s.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerifyEmailResponse { Message = "Email verified successfully." });

        await _client.PostAsJsonAsync(
            "/api/v1/auth/verify-email",
            new { token = rawToken });

        _senderMock.Verify(
            s => s.Send(
                It.Is<VerifyEmailCommand>(c => c.Token == rawToken),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_ValidationError_Returns400WithErrors()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(
                new[]
                {
                    new ValidationFailure("Token", "'Token' must not be empty.")
                }));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/verify-email",
            new { token = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.True(body.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Token", out _));
    }
}
