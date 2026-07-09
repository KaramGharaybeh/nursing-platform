using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Identity.Commands.ForgotPassword;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class ForgotPasswordEndpointTests
{
    private const string SuccessMessage = "If the email exists, a password reset link has been sent.";

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public ForgotPasswordEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_WithEmail_Returns200WithoutExposingToken()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ForgotPasswordResponse { Message = SuccessMessage });

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new { email = "user@test.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
        var body = JsonSerializer.Deserialize<ForgotPasswordResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Equal(SuccessMessage, body.Message);
    }

    [Fact]
    public async Task ForgotPassword_WithNonexistentEmail_ReturnsSameGeneric200Response()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ForgotPasswordResponse { Message = SuccessMessage });

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new { email = "missing@test.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(body);
        Assert.Equal(SuccessMessage, body.Message);
    }

    [Fact]
    public async Task ForgotPassword_SendsCommandWithRequestEmail()
    {
        const string email = "user@test.com";
        _senderMock
            .Setup(s => s.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ForgotPasswordResponse { Message = SuccessMessage });

        await _client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new { email });

        _senderMock.Verify(
            s => s.Send(
                It.Is<ForgotPasswordCommand>(c => c.Email == email),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_ValidationError_Returns400WithErrors()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(
                new[]
                {
                    new ValidationFailure("Email", "'Email' is not a valid email address.")
                }));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new { email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.True(body.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Email", out _));
    }
}
