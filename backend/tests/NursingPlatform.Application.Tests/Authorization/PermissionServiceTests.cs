using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Authorization;

public class PermissionServiceTests
{
    [Fact]
    public async Task GetUserPermissionsAsync_WithRolePermissions_ReturnsDistinctPermissions()
    {
        var userId = Guid.NewGuid();
        var permCreate = new Permission { Id = Guid.NewGuid(), Name = "Users.Create" };
        var permView = new Permission { Id = Guid.NewGuid(), Name = "Users.View" };

        var userRoles = new List<UserRole>
        {
            new()
            {
                UserId = userId,
                Role = new Role
                {
                    RolePermissions = new List<RolePermission>
                    {
                        new() { Permission = permCreate },
                        new() { Permission = permView }
                    }
                }
            }
        }.AsQueryable().BuildMockDbSet();

        var contextMock = new Mock<IApplicationDbContext>();
        contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

        var service = new PermissionService(contextMock.Object);
        var result = await service.GetUserPermissionsAsync(userId);

        Assert.Contains("Users.Create", result);
        Assert.Contains("Users.View", result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_UserWithNoRoles_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();
        var userRoles = new List<UserRole>().AsQueryable().BuildMockDbSet();

        var contextMock = new Mock<IApplicationDbContext>();
        contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

        var service = new PermissionService(contextMock.Object);
        var result = await service.GetUserPermissionsAsync(userId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_DuplicatePermissionsAcrossRoles_ReturnsDistinct()
    {
        var userId = Guid.NewGuid();
        var permCreate = new Permission { Id = Guid.NewGuid(), Name = "Users.Create" };

        var userRoles = new List<UserRole>
        {
            new()
            {
                UserId = userId,
                Role = new Role
                {
                    RolePermissions = new List<RolePermission>
                    {
                        new() { Permission = permCreate }
                    }
                }
            },
            new()
            {
                UserId = userId,
                Role = new Role
                {
                    RolePermissions = new List<RolePermission>
                    {
                        new() { Permission = permCreate }
                    }
                }
            }
        }.AsQueryable().BuildMockDbSet();

        var contextMock = new Mock<IApplicationDbContext>();
        contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

        var service = new PermissionService(contextMock.Object);
        var result = await service.GetUserPermissionsAsync(userId);

        Assert.Single(result);
        Assert.Contains("Users.Create", result);
    }
}
