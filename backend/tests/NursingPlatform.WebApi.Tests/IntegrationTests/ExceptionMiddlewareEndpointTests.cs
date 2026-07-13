using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Identity.Commands.Login;
using NursingPlatform.Application.Payments.Abstractions;

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
    }
}
