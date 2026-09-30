using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Identity.Commands.Login;
using NursingPlatform.Application.Payments.Abstractions;
using NursingPlatform.Application.PreparationPackages.ExamSessions.Exceptions;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class ExceptionMiddlewareEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public ExceptionMiddlewareEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthenticationChallenge_ReturnsEmpty401WithBearerHeader()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile/preparation-packages/entitlements");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
        Assert.Null(response.Content.Headers.ContentType);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task ForbiddenAccessException_Returns403ProblemDetails()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Nurse role is required."));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "nurse@test.com", password = "ValidPass1" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Forbidden", body.GetProperty("title").GetString());
        Assert.Equal(403, body.GetProperty("status").GetInt32());
        Assert.Equal("Nurse role is required.", body.GetProperty("detail").GetString());
        Assert.True(body.GetProperty("traceId").GetString()?.Length > 0);
    }

    [Fact]
    public async Task ValidationException_ReturnsValidationProblemDetailsWithErrors()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException(
            [
                new FluentValidation.Results.ValidationFailure("Email", "Email is required.")
            ]));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "", password = "ValidPass1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.True(body.GetProperty("traceId").GetString()?.Length > 0);
        Assert.Equal("Email is required.", body.GetProperty("errors").GetProperty("Email")[0].GetString());
    }

    [Fact]
    public async Task CheckoutInitializationInProgressException_Returns409ProblemDetailsWithRetryAfter()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CheckoutInitializationInProgressException(TimeSpan.FromSeconds(12)));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "nurse@test.com", password = "ValidPass1" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Conflict", body.GetProperty("title").GetString());
        Assert.Equal(409, body.GetProperty("status").GetInt32());
        Assert.Equal("Checkout initialization is already in progress.", body.GetProperty("detail").GetString());
        Assert.Equal(12, body.GetProperty("retryAfterSeconds").GetInt32());
        Assert.Equal(TimeSpan.FromSeconds(12), response.Headers.RetryAfter?.Delta);
        Assert.True(body.GetProperty("traceId").GetString()?.Length > 0);
    }

    [Fact]
    public async Task PackageConflict_ReturnsCodedProblemDetails()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PackageExamSessionConflictException(
                "package-attempt-consumed",
                "Package attempt was already consumed."));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "nurse@test.com", password = "ValidPass1" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("package-attempt-consumed", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("traceId").GetString()?.Length > 0);
    }
}
