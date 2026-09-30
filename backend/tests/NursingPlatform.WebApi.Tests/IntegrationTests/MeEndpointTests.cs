using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NursingPlatform.Application.Identity.Commands.UpdateCurrentUserProfile;
using NursingPlatform.Application.Identity.DTOs;
using NursingPlatform.Application.Identity.Queries.GetCurrentUser;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class MeEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public MeEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMe_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidToken_Returns200WithUserDetailDto()
    {
        var userId = Guid.NewGuid();
        var token = CreateJwt(userId);

        var expectedUser = new UserDetailDto
        {
            Id = userId,
            Email = "user@test.com",
            Username = "user-name",
            FirstName = "John",
            LastName = "Doe",
            IsProfileComplete = true,
            IsActive = true,
            EmailVerified = true,
            CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
            LastLoginAt = new DateTime(2026, 7, 8, 9, 0, 0, DateTimeKind.Utc),
            Roles = ["Admin"],
            Permissions = ["Users.View", "Users.Create"]
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);

        var body = JsonSerializer.Deserialize<UserDetailDto>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Equal(userId, body.Id);
        Assert.Equal("user@test.com", body.Email);
        Assert.Equal("user-name", body.Username);
        Assert.Equal("John", body.FirstName);
        Assert.Equal("Doe", body.LastName);
        Assert.True(body.IsProfileComplete);
        Assert.True(body.IsActive);
        Assert.True(body.EmailVerified);
        Assert.Equal(new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc), body.CreatedAt);
        Assert.Equal(new DateTime(2026, 7, 8, 9, 0, 0, DateTimeKind.Utc), body.LastLoginAt);
        Assert.Equal("Admin", Assert.Single(body.Roles));
        Assert.Contains("Users.View", body.Permissions);
        Assert.Contains("Users.Create", body.Permissions);
    }

    [Fact]
    public async Task GetMe_SenderReceivesGetCurrentUserQuery()
    {
        var userId = Guid.NewGuid();
        var token = CreateJwt(userId);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDetailDto { Id = userId, Email = "user@test.com" });

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await _client.GetAsync("/api/v1/me");

        _senderMock.Verify(
            s => s.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMe_NoPermissionServiceSetup_Works()
    {
        var userId = Guid.NewGuid();
        var token = CreateJwt(userId);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDetailDto { Id = userId, Email = "user@test.com" });

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithoutToken_Returns401()
    {
        var response = await _client.PutAsJsonAsync("/api/v1/me/profile", new
        {
            firstName = "John",
            lastName = "Doe"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithValidToken_SendsCommandAndReturnsProfileState()
    {
        var userId = Guid.NewGuid();
        var token = CreateJwt(userId);

        _senderMock
            .Setup(s => s.Send(It.IsAny<UpdateCurrentUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateCurrentUserProfileResponse
            {
                Username = "user-name",
                FirstName = "John",
                LastName = "Doe",
                IsProfileComplete = true
            });

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync("/api/v1/me/profile", new
        {
            firstName = "John",
            lastName = "Doe"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);

        _senderMock.Verify(
            s => s.Send(
                It.Is<UpdateCurrentUserProfileCommand>(command =>
                    command.FirstName == "John" &&
                    command.LastName == "Doe"),
                It.IsAny<CancellationToken>()),
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
