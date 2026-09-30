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

    [Fact]
    public async Task PublicSignUp_ValidAnonymousRequest_Returns202EmptyBodyAndDispatchesServerNurseRole()
    {
        PublicRegisterCommand? captured = null;
        _senderMock
            .Setup(s => s.Send(It.IsAny<PublicRegisterCommand>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((command, _) => captured = (PublicRegisterCommand)command)
            .ReturnsAsync(Unit.Value);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-up", new
        {
            email = "new@test.com",
            username = "new-user",
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
        Assert.Equal(PublicRegistrationRoleNames.Nurse, captured.RoleName);
        Assert.Equal("new@test.com", captured.Email);
        Assert.Equal("new-user", captured.Username);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Expert")]
    [InlineData("Employer")]
    public async Task PublicSignUp_InjectedRoleFields_DoNotControlAssignedRole(string injectedRole)
    {
        PublicRegisterCommand? captured = null;
        _senderMock
            .Setup(s => s.Send(It.IsAny<PublicRegisterCommand>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((command, _) => captured = (PublicRegisterCommand)command)
            .ReturnsAsync(Unit.Value);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-up", new
        {
            email = "malicious@test.com",
            username = "malicious-user",
            password = "Password1!",
            role = injectedRole,
            roleId = Guid.NewGuid(),
            roleIds = new[] { Guid.NewGuid() },
            accountType = injectedRole,
            actorType = injectedRole
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(PublicRegistrationRoleNames.Nurse, captured.RoleName);
    }

    [Theory]
    [InlineData("/api/v1/auth/register/nurse")]
    [InlineData("/api/v1/auth/register/employer")]
    public async Task RoleSpecificPublicRegisterEndpoints_AreRetired(string path)
    {
        var response = await _client.PostAsJsonAsync(path, new
        {
            email = "old@test.com",
            username = "old-user",
            password = "Password1!"
        });

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        _senderMock.Verify(
            s => s.Send(It.IsAny<PublicRegisterCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PublicRegister_DuplicateOutcome_Returns202EmptyBody()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<PublicRegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-up", new
        {
            email = "existing@test.com",
            username = "existing-user",
            password = "Password1!",
        });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
    }
}
