using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Authorization;

namespace NursingPlatform.Application.Tests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IPermissionService> _permissionServiceMock = new();

    private PermissionAuthorizationHandler CreateHandler() =>
        new(_currentUserMock.Object, _permissionServiceMock.Object);

    private static AuthorizationHandlerContext CreateContext(string permission) =>
        new([new PermissionRequirement(permission)],
            new ClaimsPrincipal(new ClaimsIdentity()),
            null);

    [Fact]
    public async Task HandleAsync_UnauthenticatedUser_ShouldNotSucceed()
    {
        _currentUserMock.SetupGet(u => u.IsAuthenticated).Returns(false);

        var context = CreateContext("Users.Create");
        await CreateHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_AuthenticatedUserWithNullUserId_ShouldNotSucceed()
    {
        _currentUserMock.SetupGet(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(u => u.UserId).Returns((Guid?)null);

        var context = CreateContext("Users.Create");
        await CreateHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserWithoutRequiredPermission_ShouldNotSucceed()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.SetupGet(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(u => u.UserId).Returns(userId);
        _permissionServiceMock
            .Setup(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.View" });

        var context = CreateContext("Users.Create");
        await CreateHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserWithRequiredPermission_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.SetupGet(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(u => u.UserId).Returns(userId);
        _permissionServiceMock
            .Setup(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.Create" });

        var context = CreateContext("Users.Create");
        await CreateHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_CaseInsensitiveComparison_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.SetupGet(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(u => u.UserId).Returns(userId);
        _permissionServiceMock
            .Setup(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "users.create" });

        var context = CreateContext("Users.Create");
        await CreateHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }
}
