using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Identity.DTOs;
using NursingPlatform.Application.Identity.Queries.GetUser;
using NursingPlatform.Application.Identity.Queries.ListUsers;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class UsersEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;

    public UsersEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _permissionServiceMock = factory.PermissionServiceMock;
        _senderMock.Reset();
        _permissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task List_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_WithoutPermission_Returns403()
    {
        var token = CreateJwt(Guid.NewGuid());

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_WithPermission_Returns200WithPaginatedResult()
    {
        var token = CreateJwt(Guid.NewGuid());

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.View" });

        var expected = new PaginatedResult<UserListItemDto>
        {
            Items =
            [
                new UserListItemDto
                {
                    Id = Guid.NewGuid(),
                    Email = "user@test.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
                    LastLoginAt = new DateTime(2026, 7, 8, 9, 0, 0, DateTimeKind.Utc),
                    Roles = ["Admin", "Nurse"]
                }
            ],
            Page = 1,
            PageSize = 20,
            TotalCount = 1
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("permissions", json, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(20, root.GetProperty("pageSize").GetInt32());
        Assert.Equal(1, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, root.GetProperty("totalPages").GetInt32());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("items").ValueKind);
        Assert.Equal(1, root.GetProperty("items").GetArrayLength());

        var body = JsonSerializer.Deserialize<PaginatedResult<UserListItemDto>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(body);
        Assert.Contains("Admin", body.Items[0].Roles);
        Assert.Contains("Nurse", body.Items[0].Roles);
    }

    [Fact]
    public async Task List_SendsQueryWithDefaultValues()
    {
        var token = CreateJwt(Guid.NewGuid());

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.View" });

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<UserListItemDto>());

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await _client.GetAsync("/api/v1/users");

        _senderMock.Verify(
            s => s.Send(
                It.Is<ListUsersQuery>(q =>
                    q.Page == 1 &&
                    q.PageSize == 20 &&
                    q.Search == null &&
                    q.IsActive == null &&
                    q.Role == null &&
                    q.Sort == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task List_SendsQueryWithProvidedParams()
    {
        var token = CreateJwt(Guid.NewGuid());

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.View" });

        _senderMock
            .Setup(s => s.Send(It.IsAny<ListUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<UserListItemDto>());

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await _client.GetAsync("/api/v1/users?page=2&pageSize=10&search=john&isActive=true&role=Nurse&sort=-email");

        _senderMock.Verify(
            s => s.Send(
                It.Is<ListUsersQuery>(q =>
                    q.Page == 2 &&
                    q.PageSize == 10 &&
                    q.Search == "john" &&
                    q.IsActive == true &&
                    q.Role == "Nurse" &&
                    q.Sort == "-email"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutPermission_Returns403()
    {
        var userId = Guid.NewGuid();
        var token = CreateJwt(Guid.NewGuid());

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/v1/users/{userId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithPermission_Returns200WithUserDetailDto()
    {
        var targetId = Guid.NewGuid();
        var token = CreateJwt(Guid.NewGuid());

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.View" });

        var expected = new UserDetailDto
        {
            Id = targetId,
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailVerified = true,
            CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
            LastLoginAt = new DateTime(2026, 7, 8, 9, 0, 0, DateTimeKind.Utc),
            Roles = ["Admin"],
            Permissions = ["Users.View"]
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/v1/users/{targetId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);

        var body = JsonSerializer.Deserialize<UserDetailDto>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(body);
        Assert.Equal(targetId, body.Id);
        Assert.Equal("Admin", Assert.Single(body.Roles));
        Assert.Contains("Users.View", body.Permissions);
    }

    [Fact]
    public async Task GetById_SendsGetUserQueryWithRouteId()
    {
        var targetId = Guid.NewGuid();
        var token = CreateJwt(Guid.NewGuid());

        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.View" });

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDetailDto { Id = targetId, Email = "user@test.com" });

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await _client.GetAsync($"/api/v1/users/{targetId}");

        _senderMock.Verify(
            s => s.Send(
                It.Is<GetUserQuery>(q => q.UserId == targetId),
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
