using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Commands.UpdateUserRoles;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class UpdateUserRolesCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Handle_NurseToEmployer_ReplacesBusinessRoleAndRevokesRefreshTokens()
    {
        var targetUserId = Guid.NewGuid();
        var nurseRole = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var employerRole = new Role { Id = Guid.NewGuid(), Name = "Employer" };
        var targetUser = new User
        {
            Id = targetUserId,
            Email = "nurse@test.com",
            Username = "nurse-user",
            NormalizedUsername = "NURSE-USER",
            FirstName = "Nora",
            LastName = "Nurse",
            IsActive = true,
            EmailVerified = true
        };
        targetUser.UserRoles.Add(new UserRole { UserId = targetUserId, RoleId = nurseRole.Id, Role = nurseRole });

        SetupUsers(targetUser);
        SetupRoles(nurseRole, employerRole);
        var refreshTokens = SetupRefreshTokens(
            new RefreshToken { Id = Guid.NewGuid(), UserId = targetUserId, RevokedAt = null },
            new RefreshToken { Id = Guid.NewGuid(), UserId = targetUserId, RevokedAt = null });
        var transaction = new Mock<IApplicationDbTransaction>();
        _contextMock
            .Setup(c => c.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        _contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateUserRolesCommandHandler(_contextMock.Object);

        var result = await handler.Handle(
            new UpdateUserRolesCommand { UserId = targetUserId, RoleName = "Employer" },
            default);

        Assert.Contains("Employer", result.Roles);
        Assert.DoesNotContain("Nurse", result.Roles);
        Assert.All(refreshTokens, t => Assert.NotNull(t.RevokedAt));
        transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmployerToExpert_PreservesUnrelatedSystemRole()
    {
        var targetUserId = Guid.NewGuid();
        var employerRole = new Role { Id = Guid.NewGuid(), Name = "Employer" };
        var expertRole = new Role { Id = Guid.NewGuid(), Name = "Expert" };
        var superAdminRole = new Role { Id = Guid.NewGuid(), Name = "SuperAdmin" };
        var targetUser = new User
        {
            Id = targetUserId,
            Email = "expert@test.com",
            Username = "expert-user",
            NormalizedUsername = "EXPERT-USER",
            FirstName = "Eve",
            LastName = "Expert",
            IsActive = true,
            EmailVerified = true
        };
        targetUser.UserRoles.Add(new UserRole { UserId = targetUserId, RoleId = employerRole.Id, Role = employerRole });
        targetUser.UserRoles.Add(new UserRole { UserId = targetUserId, RoleId = superAdminRole.Id, Role = superAdminRole });

        SetupUsers(targetUser);
        SetupRoles(employerRole, expertRole, superAdminRole);
        SetupRefreshTokens();
        var transaction = new Mock<IApplicationDbTransaction>();
        _contextMock
            .Setup(c => c.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        _contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateUserRolesCommandHandler(_contextMock.Object);

        var result = await handler.Handle(
            new UpdateUserRolesCommand { UserId = targetUserId, RoleName = "Expert" },
            default);

        Assert.Contains("Expert", result.Roles);
        Assert.Contains("SuperAdmin", result.Roles);
        Assert.DoesNotContain("Employer", result.Roles);
    }

    [Fact]
    public async Task Handle_InvalidRole_ThrowsWithoutMutatingRoles()
    {
        var targetUserId = Guid.NewGuid();
        var nurseRole = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var targetUser = new User
        {
            Id = targetUserId,
            Email = "user@test.com",
            Username = "some-user",
            NormalizedUsername = "SOME-USER",
            IsActive = true,
            EmailVerified = true
        };
        targetUser.UserRoles.Add(new UserRole { UserId = targetUserId, RoleId = nurseRole.Id, Role = nurseRole });

        SetupUsers(targetUser);
        SetupRoles(nurseRole);
        SetupRefreshTokens();
        _contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateUserRolesCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateUserRolesCommand { UserId = targetUserId, RoleName = "SuperAdmin" }, default));

        Assert.Single(targetUser.UserRoles);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingUser_ThrowsKeyNotFoundException()
    {
        SetupUsers();
        SetupRoles(new Role { Id = Guid.NewGuid(), Name = "Nurse" });
        SetupRefreshTokens();

        var handler = new UpdateUserRolesCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new UpdateUserRolesCommand { UserId = Guid.NewGuid(), RoleName = "Nurse" }, default));
    }

    [Fact]
    public void Validator_RejectsEmptyRoleName()
    {
        var validator = new UpdateUserRolesCommandValidator();
        var command = new UpdateUserRolesCommand { UserId = Guid.NewGuid(), RoleName = string.Empty };

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validator_RejectsUnsupportedRoleName()
    {
        var validator = new UpdateUserRolesCommandValidator();
        var command = new UpdateUserRolesCommand { UserId = Guid.NewGuid(), RoleName = "SuperAdmin" };

        Assert.False(validator.Validate(command).IsValid);
    }

    private void SetupUsers(params User[] users)
    {
        var dbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(dbSet.Object);
    }

    private void SetupRoles(params Role[] roles)
    {
        var dbSet = roles.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(dbSet.Object);
    }

    private List<RefreshToken> SetupRefreshTokens(params RefreshToken[] tokens)
    {
        var list = tokens.ToList();
        var dbSet = list.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.RefreshTokens).Returns(dbSet.Object);
        return list;
    }
}
