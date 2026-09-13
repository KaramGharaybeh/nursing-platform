using System.Net;
using System.Net.Http.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Identity.Commands.PublicRegister;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class PublicRegisterEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public PublicRegisterEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/v1/auth/register/nurse", "Nurse")]
    [InlineData("/api/v1/auth/register/employer", "Employer")]
    public async Task PublicRegister_ValidAnonymousRequest_Returns202EmptyBodyAndDispatchesEndpointRole(
        string path,
        string expectedRole)
    {
        PublicRegisterCommand? captured = null;
        _senderMock
            .Setup(s => s.Send(It.IsAny<PublicRegisterCommand>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((command, _) => captured = (PublicRegisterCommand)command)
            .ReturnsAsync(Unit.Value);

        var response = await _client.PostAsJsonAsync(path, new
        {
            email = "new@test.com",
            password = "Password1!",
            firstName = "New",
            lastName = "User",
            roleIds = new[] { Guid.NewGuid() },
            roleName = "SuperAdmin"
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(string.Empty, json);
        Assert.DoesNotContain("accessToken", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("refreshToken", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("session", json, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(captured);
        Assert.Equal(expectedRole, captured.RoleName);
        Assert.Equal("new@test.com", captured.Email);
    }

    [Fact]
    public async Task PublicRegister_DuplicateOutcome_Returns202EmptyBody()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<PublicRegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/nurse", new
        {
            email = "existing@test.com",
            password = "Password1!",
            firstName = "Existing",
            lastName = "User"
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
    }
}
