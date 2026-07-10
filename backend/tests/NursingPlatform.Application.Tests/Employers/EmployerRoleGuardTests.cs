using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Employers;

public class EmployerRoleGuardTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task EnsureEmployerAsync_WithEmployerRole_AllowsAccess()
    {
        var userId = Guid.NewGuid();
        var guard = CreateGuard(userId, CreateUserWithRole(userId, "Employer"));

        var result = await guard.EnsureEmployerAsync(CancellationToken.None);

        Assert.Equal(userId, result);
    }

    [Fact]
    public async Task EnsureEmployerAsync_WithoutEmployerRole_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var guard = CreateGuard(userId, CreateUserWithRole(userId, "Nurse"));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            guard.EnsureEmployerAsync(CancellationToken.None));
    }

    private EmployerRoleGuard CreateGuard(Guid? currentUserId, User user)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

        return new EmployerRoleGuard(_contextMock.Object, _currentUserMock.Object);
    }

    private static User CreateUserWithRole(Guid userId, string roleName)
    {
        var roleId = Guid.NewGuid();
        return new User
        {
            Id = userId,
            IsActive = true,
            UserRoles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    Role = new Role { Id = roleId, Name = roleName }
                }
            ]
        };
    }
}
