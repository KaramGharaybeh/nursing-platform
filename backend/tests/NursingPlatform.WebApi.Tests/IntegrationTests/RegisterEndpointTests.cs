using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Identity.Commands.Register;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class RegisterEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;

    public RegisterEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _permissionServiceMock = factory.PermissionServiceMock;
        _senderMock.Reset();
        _permissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Unauthenticated_Returns401()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "newuser@test.com",
            password = "NewUserPass1!",
            firstName = "New",
            lastName = "User",
            roleIds = new List<Guid>()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithoutPermission_Returns403()
    {
        var userId = Guid.NewGuid();
        var token = CreateJwt(userId);

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "newuser@test.com",
            password = "NewUserPass1!",
            firstName = "New",
            lastName = "User",
            roleIds = new List<Guid>()
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithPermission_Returns200()
    {
        var userId = Guid.NewGuid();
        var token = CreateJwt(userId);

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.Create" });

        var newUserId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUserId);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "newuser@test.com",
            password = "NewUserPass1!",
            firstName = "New",
            lastName = "User",
            roleIds = new List<Guid>()
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        Assert.NotNull(body);
        Assert.Equal(newUserId, body.UserId);
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
