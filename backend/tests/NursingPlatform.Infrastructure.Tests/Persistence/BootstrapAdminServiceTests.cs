using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Configuration;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class BootstrapAdminServiceTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IPasswordHashingService> _passwordHasherMock = new();
    private readonly Mock<ILogger<BootstrapAdminService>> _loggerMock = new();

    private readonly AdminSettings _settings = new()
    {
        Email = "admin@test.com",
        Password = "AdminPass1",
        FirstName = "System",
        LastName = "Administrator"
    };

    private readonly Guid _adminRoleId = new("091A2B3C-4D5E-46F7-0819-2A3B4C5D6E7F");
    private const string AdminRoleName = "Admin";

    private BootstrapAdminService CreateService()
    {
        return new BootstrapAdminService(
            _contextMock.Object,
            _passwordHasherMock.Object,
            Options.Create(_settings),
            _loggerMock.Object);
    }

    [Fact]
    public async Task BootstrapAsync_WhenNoAdminUserExists_CreatesAdminUser()
    {
        var users = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var roles = new List<Role>
        {
            new() { Id = _adminRoleId, Name = AdminRoleName }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        var userRoles = new List<UserRole>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);
        _passwordHasherMock.Setup(p => p.Hash(_settings.Password)).Returns("hashed-value");

        var service = CreateService();
        await service.BootstrapAsync(default);

        _contextMock.Verify(c => c.Users.Add(It.Is<User>(u =>
            u.Email == _settings.Email
            && u.FirstName == _settings.FirstName
            && u.LastName == _settings.LastName
            && u.PasswordHash == "hashed-value"
            && u.IsActive)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BootstrapAsync_WhenAdminUserExistsWithoutRole_AssignsAdminRole()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = _settings.Email,
            FirstName = _settings.FirstName,
            LastName = _settings.LastName,
            PasswordHash = "existing-hash",
            IsActive = true
        };
        var users = new List<User> { existingUser }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var roles = new List<Role>
        {
            new() { Id = _adminRoleId, Name = AdminRoleName }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        var userRoles = new List<UserRole>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

        var service = CreateService();
        await service.BootstrapAsync(default);

        _contextMock.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Never);
        _contextMock.Verify(c => c.UserRoles.Add(It.Is<UserRole>(ur =>
            ur.UserId == existingUser.Id && ur.RoleId == _adminRoleId)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BootstrapAsync_WhenAdminUserExistsWithRole_DoesNotDuplicateUserRole()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = _settings.Email,
            FirstName = _settings.FirstName,
            LastName = _settings.LastName,
            PasswordHash = "existing-hash",
            IsActive = true
        };
        var users = new List<User> { existingUser }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var roles = new List<Role>
        {
            new() { Id = _adminRoleId, Name = AdminRoleName }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        var userRoles = new List<UserRole>
        {
            new() { UserId = existingUser.Id, RoleId = _adminRoleId }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

        var service = CreateService();
        await service.BootstrapAsync(default);

        _contextMock.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Never);
        _contextMock.Verify(c => c.UserRoles.Add(It.IsAny<UserRole>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BootstrapAsync_WhenAdminRoleMissing_CreatesAdminRole()
    {
        var users = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var roles = new List<Role>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        var userRoles = new List<UserRole>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);
        _passwordHasherMock.Setup(p => p.Hash(_settings.Password)).Returns("hashed-value");

        var service = CreateService();
        await service.BootstrapAsync(default);

        _contextMock.Verify(c => c.Roles.Add(It.Is<Role>(r =>
            r.Name == AdminRoleName)), Times.Once);
    }

    [Fact]
    public async Task BootstrapAsync_WhenAdminRoleExists_UsesExistingRole()
    {
        var users = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var roles = new List<Role>
        {
            new() { Id = _adminRoleId, Name = AdminRoleName }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        var userRoles = new List<UserRole>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);
        _passwordHasherMock.Setup(p => p.Hash(_settings.Password)).Returns("hashed-value");

        var service = CreateService();
        await service.BootstrapAsync(default);

        _contextMock.Verify(c => c.Roles.Add(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task BootstrapAsync_AssignsAdminRoleToUser()
    {
        var users = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var roles = new List<Role>
        {
            new() { Id = _adminRoleId, Name = AdminRoleName }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        var userRoles = new List<UserRole>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);
        _passwordHasherMock.Setup(p => p.Hash(_settings.Password)).Returns("hashed-value");

        var service = CreateService();
        await service.BootstrapAsync(default);

        _contextMock.Verify(c => c.UserRoles.Add(It.Is<UserRole>(ur =>
            ur.RoleId == _adminRoleId)), Times.Once);
    }

    [Fact]
    public async Task BootstrapAsync_StoresHashedPasswordNotPlaintext()
    {
        var hashedPassword = "$2a$11$hashedvalue";
        var users = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var roles = new List<Role>
        {
            new() { Id = _adminRoleId, Name = AdminRoleName }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        var userRoles = new List<UserRole>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);
        _passwordHasherMock.Setup(p => p.Hash(_settings.Password)).Returns(hashedPassword);

        var service = CreateService();
        await service.BootstrapAsync(default);

        _contextMock.Verify(c => c.Users.Add(It.Is<User>(u =>
            u.PasswordHash == hashedPassword
            && u.PasswordHash != _settings.Password)), Times.Once);
    }

    [Fact]
    public async Task BootstrapAsync_IsIdempotent()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = _settings.Email,
            FirstName = _settings.FirstName,
            LastName = _settings.LastName,
            PasswordHash = "existing-hash",
            IsActive = true
        };
        var users = new List<User> { existingUser }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var roles = new List<Role>
        {
            new() { Id = _adminRoleId, Name = AdminRoleName }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        var userRoles = new List<UserRole>
        {
            new() { UserId = existingUser.Id, RoleId = _adminRoleId }
        }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

        var service = CreateService();
        await service.BootstrapAsync(default);
        await service.BootstrapAsync(default);

        _contextMock.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Never);
        _contextMock.Verify(c => c.Roles.Add(It.IsAny<Role>()), Times.Never);
        _contextMock.Verify(c => c.UserRoles.Add(It.IsAny<UserRole>()), Times.Never);
    }

    [Theory]
    [InlineData("", "ValidPass1", "F", "L")]
    [InlineData("not-an-email", "ValidPass1", "F", "L")]
    [InlineData("a@b.com", "short", "F", "L")]
    [InlineData("a@b.com", "ValidPass1", "", "L")]
    [InlineData("a@b.com", "ValidPass1", "F", "")]
    public void AdminSettings_Validation_InvalidConfiguration_Fails(string email, string password, string firstName, string lastName)
    {
        var settings = new AdminSettings
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };

        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(settings, context, results, validateAllProperties: true);

        Assert.False(isValid);
    }

    [Fact]
    public void AdminSettings_Validation_ValidConfiguration_Passes()
    {
        var settings = new AdminSettings
        {
            Email = "admin@test.com",
            Password = "ValidPass1",
            FirstName = "System",
            LastName = "Administrator"
        };

        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(settings, context, results, validateAllProperties: true);

        Assert.True(isValid);
    }
}
