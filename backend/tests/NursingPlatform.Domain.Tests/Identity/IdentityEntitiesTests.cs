using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Tests.Identity;

public class IdentityEntitiesTests
{
    [Fact]
    public void User_Creation_SetsDefaultValues()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe"
        };

        Assert.True(user.IsActive);
        Assert.False(user.EmailVerified);
        Assert.Null(user.LastLoginAt);
    }

    [Fact]
    public void UserRole_LinksUserAndRole()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRole = new UserRole { UserId = userId, RoleId = roleId };

        Assert.Equal(userId, userRole.UserId);
        Assert.Equal(roleId, userRole.RoleId);
    }

    [Fact]
    public void RefreshToken_EnforcesExpiration()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow
        };

        Assert.False(token.ExpiresAt <= DateTime.UtcNow);
    }

    [Fact]
    public void EmailVerificationToken_StoresHashAndUsageState()
    {
        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal("hash", token.TokenHash);
        Assert.Null(token.UsedAt);
        Assert.False(token.ExpiresAt <= DateTime.UtcNow);
    }

    [Fact]
    public void PasswordResetToken_StoresHashAndUsageState()
    {
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal("hash", token.TokenHash);
        Assert.Null(token.UsedAt);
        Assert.False(token.ExpiresAt <= DateTime.UtcNow);
    }

    [Fact]
    public void UserRole_DoesNotInheritAuditableEntity()
    {
        Assert.False(typeof(UserRole).IsSubclassOf(typeof(NursingPlatform.Domain.Common.AuditableEntity)));
    }

    [Fact]
    public void RefreshToken_DoesNotInheritAuditableEntity()
    {
        Assert.False(typeof(RefreshToken).IsSubclassOf(typeof(NursingPlatform.Domain.Common.AuditableEntity)));
    }

    [Fact]
    public void EmailVerificationToken_DoesNotInheritAuditableEntity()
    {
        Assert.False(typeof(EmailVerificationToken).IsSubclassOf(typeof(NursingPlatform.Domain.Common.AuditableEntity)));
    }

    [Fact]
    public void PasswordResetToken_DoesNotInheritAuditableEntity()
    {
        Assert.False(typeof(PasswordResetToken).IsSubclassOf(typeof(NursingPlatform.Domain.Common.AuditableEntity)));
    }

    [Fact]
    public void Role_HasUserRolesNavigation()
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "TestRole" };
        var userRole = new UserRole { RoleId = role.Id };
        role.UserRoles = new List<UserRole> { userRole };

        Assert.Single(role.UserRoles);
        Assert.Equal(role.Id, role.UserRoles.First().RoleId);
    }
}
