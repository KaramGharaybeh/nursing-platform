using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Identity.Commands.ResetPassword;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class ResetPasswordEndpointTests
{
    private const string SuccessMessage = "Password has been reset successfully.";

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public ResetPasswordEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ResetPassword_WithValidRequest_Returns200WithoutExposingToken()
    {
        const string rawToken = "raw-token";
        _senderMock
            .Setup(s => s.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResetPasswordResponse { Message = SuccessMessage });

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new { email = "user@test.com", token = rawToken, newPassword = "NewPass1!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(rawToken, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
        var body = JsonSerializer.Deserialize<ResetPasswordResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Equal(SuccessMessage, body.Message);
    }

    [Fact]
    public async Task ResetPassword_SendsCommandWithRequestFields()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResetPasswordResponse { Message = SuccessMessage });

        await _client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new { email = "user@test.com", token = "raw-token", newPassword = "NewPass1!" });

        _senderMock.Verify(
            s => s.Send(
                It.Is<ResetPasswordCommand>(c =>
                    c.Email == "user@test.com" &&
                    c.Token == "raw-token" &&
                    c.NewPassword == "NewPass1!"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ValidationError_Returns400WithErrors()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(
                new[]
                {
                    new ValidationFailure("Email", "'Email' must not be empty."),
                    new ValidationFailure("Token", "'Token' must not be empty."),
                    new ValidationFailure("NewPassword", "'New Password' must not be empty.")
                }));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new { email = string.Empty, token = string.Empty, newPassword = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.True(body.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Email", out _));
        Assert.True(errors.TryGetProperty("Token", out _));
        Assert.True(errors.TryGetProperty("NewPassword", out _));
    }
}
