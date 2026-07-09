using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.DTOs;
using NursingPlatform.Application.Identity.Queries.GetUser;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Identity.Queries.GetUser;

public class GetUserQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTests()
    {
        _handler = new GetUserQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_UserFound_ReturnsUserDetailDtoWithRolesAndPermissions()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permView = new Permission { Id = Guid.NewGuid(), Name = "Users.View" };
        var permCreate = new Permission { Id = Guid.NewGuid(), Name = "Users.Create" };

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailVerified = true,
            CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
            LastLoginAt = new DateTime(2026, 7, 8, 9, 0, 0, DateTimeKind.Utc),
            UserRoles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    Role = new Role
                    {
                        Id = roleId,
                        Name = "Nurse",
                        RolePermissions =
                        [
                            new RolePermission { PermissionId = permView.Id, Permission = permView },
                            new RolePermission { PermissionId = permCreate.Id, Permission = permCreate }
                        ]
                    }
                }
            ]
        };

        var users = new List<User> { user }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        var result = await _handler.Handle(new GetUserQuery { UserId = userId }, CancellationToken.None);

        Assert.Equal(userId, result.Id);
        Assert.Equal("user@test.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.True(result.IsActive);
        Assert.True(result.EmailVerified);
        Assert.Equal(new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc), result.CreatedAt);
        Assert.Equal(new DateTime(2026, 7, 8, 9, 0, 0, DateTimeKind.Utc), result.LastLoginAt);

        Assert.Single(result.Roles);
        Assert.Equal("Nurse", result.Roles[0]);

        Assert.Equal(2, result.Permissions.Count);
        Assert.Contains("Users.Create", result.Permissions);
        Assert.Contains("Users.View", result.Permissions);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var users = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(new GetUserQuery { UserId = userId }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MultipleRoles_AreSortedDeterministically()
    {
        var userId = Guid.NewGuid();
        var roleIdA = Guid.NewGuid();
        var roleIdB = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailVerified = false,
            CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
            UserRoles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleIdA,
                    Role = new Role
                    {
                        Id = roleIdA,
                        Name = "Nurse",
                        RolePermissions = []
                    }
                },
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleIdB,
                    Role = new Role
                    {
                        Id = roleIdB,
                        Name = "Admin",
                        RolePermissions = []
                    }
                }
            ]
        };

        var users = new List<User> { user }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        var result = await _handler.Handle(new GetUserQuery { UserId = userId }, CancellationToken.None);

        Assert.Equal(2, result.Roles.Count);
        Assert.Equal("Admin", result.Roles[0]);
        Assert.Equal("Nurse", result.Roles[1]);
    }

    [Fact]
    public async Task Handle_DuplicateRoleNames_AreDeDuplicated()
    {
        var userId = Guid.NewGuid();
        var roleIdA = Guid.NewGuid();
        var roleIdB = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailVerified = false,
            CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
            UserRoles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleIdA,
                    Role = new Role
                    {
                        Id = roleIdA,
                        Name = "Nurse",
                        RolePermissions = []
                    }
                },
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleIdB,
                    Role = new Role
                    {
                        Id = roleIdB,
                        Name = "Nurse",
                        RolePermissions = []
                    }
                }
            ]
        };

        var users = new List<User> { user }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        var result = await _handler.Handle(new GetUserQuery { UserId = userId }, CancellationToken.None);

        Assert.Single(result.Roles);
        Assert.Equal("Nurse", result.Roles[0]);
    }

    [Fact]
    public async Task Handle_DuplicatePermissions_AreDeDuplicated()
    {
        var userId = Guid.NewGuid();
        var roleIdA = Guid.NewGuid();
        var roleIdB = Guid.NewGuid();
        var permView = new Permission { Id = Guid.NewGuid(), Name = "Users.View" };

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailVerified = false,
            CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc),
            UserRoles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleIdA,
                    Role = new Role
                    {
                        Id = roleIdA,
                        Name = "Nurse",
                        RolePermissions =
                        [
                            new RolePermission { PermissionId = permView.Id, Permission = permView }
                        ]
                    }
                },
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleIdB,
                    Role = new Role
                    {
                        Id = roleIdB,
                        Name = "Admin",
                        RolePermissions =
                        [
                            new RolePermission { PermissionId = permView.Id, Permission = permView }
                        ]
                    }
                }
            ]
        };

        var users = new List<User> { user }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        var result = await _handler.Handle(new GetUserQuery { UserId = userId }, CancellationToken.None);

        Assert.Single(result.Permissions);
        Assert.Equal("Users.View", result.Permissions[0]);
    }

    [Fact]
    public void Handle_UserFound_DoesNotExposePasswordHash()
    {
        var userDtoType = typeof(UserDetailDto);
        var property = userDtoType.GetProperty("PasswordHash");

        Assert.Null(property);
    }
}
