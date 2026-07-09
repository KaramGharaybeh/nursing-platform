using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.DTOs;
using NursingPlatform.Application.Identity.Queries.ListUsers;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Identity.Queries.ListUsers;

public class ListUsersQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly ListUsersQueryHandler _handler;

    public ListUsersQueryHandlerTests()
    {
        _handler = new ListUsersQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_DefaultQuery_ReturnsUsersSortedByCreatedAtDescending()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "a@test.com", FirstName = "A", LastName = "A", IsActive = true, CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = Guid.NewGuid(), Email = "b@test.com", FirstName = "B", LastName = "B", IsActive = true, CreatedAt = new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = Guid.NewGuid(), Email = "c@test.com", FirstName = "C", LastName = "C", IsActive = true, CreatedAt = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc) }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery(), CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("c@test.com", result.Items[0].Email);
        Assert.Equal("b@test.com", result.Items[1].Email);
        Assert.Equal("a@test.com", result.Items[2].Email);
    }

    [Fact]
    public async Task Handle_Pagination_WorksCorrectly()
    {
        var users = Enumerable.Range(1, 25).Select(i => new User
        {
            Id = Guid.NewGuid(),
            Email = $"user{i}@test.com",
            FirstName = $"First{i}",
            LastName = $"Last{i}",
            IsActive = true,
            CreatedAt = new DateTime(2026, 7, i, 0, 0, 0, DateTimeKind.Utc)
        }).ToList();
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Page = 2, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal("user15@test.com", result.Items[0].Email);
        Assert.Equal("user6@test.com", result.Items[^1].Email);
    }

    [Fact]
    public async Task Handle_Search_FiltersByEmailCaseInsensitive()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "test@example.com", FirstName = "John", LastName = "Doe", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "other@example.com", FirstName = "Jane", LastName = "Smith", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Search = "TEST@EXAMPLE.COM" }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("test@example.com", result.Items[0].Email);
    }

    [Fact]
    public async Task Handle_Search_FiltersByFirstNameCaseInsensitive()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "john@test.com", FirstName = "John", LastName = "Doe", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "jane@test.com", FirstName = "Jane", LastName = "Smith", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Search = "john" }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("John", result.Items[0].FirstName);
    }

    [Fact]
    public async Task Handle_Search_FiltersByLastNameCaseInsensitive()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "john@test.com", FirstName = "John", LastName = "Smith", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "jane@test.com", FirstName = "Jane", LastName = "Jones", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Search = "SMITH" }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Smith", result.Items[0].LastName);
    }

    [Fact]
    public async Task Handle_IsActiveTrue_FiltersCorrectly()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "active@test.com", FirstName = "A", LastName = "A", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "inactive@test.com", FirstName = "B", LastName = "B", IsActive = false, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { IsActive = true }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("active@test.com", result.Items[0].Email);
    }

    [Fact]
    public async Task Handle_IsActiveFalse_FiltersCorrectly()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "active@test.com", FirstName = "A", LastName = "A", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "inactive@test.com", FirstName = "B", LastName = "B", IsActive = false, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { IsActive = false }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("inactive@test.com", result.Items[0].Email);
    }

    [Fact]
    public async Task Handle_RoleFilter_FiltersByExactRoleName()
    {
        var nurseRoleId = Guid.NewGuid();
        var adminRoleId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        var users = new List<User>
        {
            new()
            {
                Id = userId1, Email = "nurse1@test.com", FirstName = "A", LastName = "A", IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserRoles =
                [
                    new UserRole { UserId = userId1, RoleId = nurseRoleId, Role = new Role { Id = nurseRoleId, Name = "Nurse" } }
                ]
            },
            new()
            {
                Id = userId2, Email = "admin@test.com", FirstName = "B", LastName = "B", IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserRoles =
                [
                    new UserRole { UserId = userId2, RoleId = adminRoleId, Role = new Role { Id = adminRoleId, Name = "Admin" } }
                ]
            },
            new()
            {
                Id = userId3, Email = "nurse2@test.com", FirstName = "C", LastName = "C", IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserRoles =
                [
                    new UserRole { UserId = userId3, RoleId = nurseRoleId, Role = new Role { Id = nurseRoleId, Name = "Nurse" } }
                ]
            }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Role = "Nurse" }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, i => i.Email == "nurse1@test.com");
        Assert.Contains(result.Items, i => i.Email == "nurse2@test.com");
    }

    [Fact]
    public async Task Handle_SortAscendingByEmail_Works()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "b@test.com", FirstName = "B", LastName = "B", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "a@test.com", FirstName = "A", LastName = "A", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Sort = "email", PageSize = 10 }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("a@test.com", result.Items[0].Email);
        Assert.Equal("b@test.com", result.Items[1].Email);
    }

    [Fact]
    public async Task Handle_SortDescendingByEmail_Works()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "a@test.com", FirstName = "A", LastName = "A", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "b@test.com", FirstName = "B", LastName = "B", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Sort = "-email", PageSize = 10 }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("b@test.com", result.Items[0].Email);
        Assert.Equal("a@test.com", result.Items[1].Email);
    }

    [Fact]
    public async Task Handle_SortByFirstName_Works()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "b@test.com", FirstName = "Charlie", LastName = "B", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "a@test.com", FirstName = "Alice", LastName = "A", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Sort = "firstName", PageSize = 10 }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Alice", result.Items[0].FirstName);
        Assert.Equal("Charlie", result.Items[1].FirstName);
    }

    [Fact]
    public async Task Handle_SortByLastName_Works()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "b@test.com", FirstName = "B", LastName = "Zeta", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "a@test.com", FirstName = "A", LastName = "Alpha", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Sort = "lastName", PageSize = 10 }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Alpha", result.Items[0].LastName);
        Assert.Equal("Zeta", result.Items[1].LastName);
    }

    [Fact]
    public async Task Handle_SortByCreatedAt_Works()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "b@test.com", FirstName = "B", LastName = "B", IsActive = true, CreatedAt = new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = Guid.NewGuid(), Email = "a@test.com", FirstName = "A", LastName = "A", IsActive = true, CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc) }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Sort = "createdAt", PageSize = 10 }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("a@test.com", result.Items[0].Email);
        Assert.Equal("b@test.com", result.Items[1].Email);
    }

    [Fact]
    public async Task Handle_SortByLastLoginAt_Works()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "b@test.com", FirstName = "B", LastName = "B", IsActive = true, CreatedAt = DateTime.UtcNow, LastLoginAt = new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = Guid.NewGuid(), Email = "a@test.com", FirstName = "A", LastName = "A", IsActive = true, CreatedAt = DateTime.UtcNow, LastLoginAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc) }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery { Sort = "lastLoginAt", PageSize = 10 }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("a@test.com", result.Items[0].Email);
        Assert.Equal("b@test.com", result.Items[1].Email);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyItemsAndTotalCountZero()
    {
        var users = new List<User>();
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery(), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Handle_Roles_AreDeDuplicatedAndSorted()
    {
        var userId = Guid.NewGuid();
        var nurseRoleId = Guid.NewGuid();
        var adminRoleId = Guid.NewGuid();

        var users = new List<User>
        {
            new()
            {
                Id = userId, Email = "user@test.com", FirstName = "U", LastName = "U", IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserRoles =
                [
                    new UserRole { UserId = userId, RoleId = nurseRoleId, Role = new Role { Id = nurseRoleId, Name = "Nurse" } },
                    new UserRole { UserId = userId, RoleId = adminRoleId, Role = new Role { Id = adminRoleId, Name = "Admin" } },
                    new UserRole { UserId = userId, RoleId = Guid.NewGuid(), Role = new Role { Id = Guid.NewGuid(), Name = "Nurse" } }
                ]
            }
        };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

        var result = await _handler.Handle(new ListUsersQuery(), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Roles.Count);
        Assert.Equal("Admin", result.Items[0].Roles[0]);
        Assert.Equal("Nurse", result.Items[0].Roles[1]);
    }

    [Fact]
    public void Handle_ListItemDto_DoesNotExposePasswordHash()
    {
        var property = typeof(UserListItemDto).GetProperty("PasswordHash");

        Assert.Null(property);
    }

    [Fact]
    public void Handle_ListItemDto_DoesNotExposePermissions()
    {
        var property = typeof(UserListItemDto).GetProperty("Permissions");

        Assert.Null(property);
    }
}
