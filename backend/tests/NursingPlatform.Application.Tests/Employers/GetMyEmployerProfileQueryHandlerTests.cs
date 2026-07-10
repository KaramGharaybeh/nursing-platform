using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Employers.DTOs;
using NursingPlatform.Application.Employers.Queries.GetMyEmployerProfile;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Employers;

public class GetMyEmployerProfileQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task GetProfile_WhenProfileDoesNotExist_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), []);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetMyEmployerProfileQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetProfile_WhenProfileExists_ReturnsProfileDto()
    {
        var userId = Guid.NewGuid();
        var profile = new EmployerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobTitle = "Recruitment Manager",
            Department = "Talent Acquisition"
        };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile]);

        var result = await handler.Handle(new GetMyEmployerProfileQuery(), CancellationToken.None);

        Assert.Equal(profile.Id, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Recruitment Manager", result.JobTitle);
        Assert.Equal("Talent Acquisition", result.Department);
        Assert.Null(typeof(EmployerProfileDto).GetProperty("PasswordHash"));
        Assert.Null(typeof(EmployerProfileDto).GetProperty("Tokens"));
        Assert.Null(typeof(EmployerProfileDto).GetProperty("Roles"));
        Assert.Null(typeof(EmployerProfileDto).GetProperty("Permissions"));
        Assert.Null(typeof(EmployerProfileDto).GetProperty("User"));
        Assert.Null(typeof(EmployerProfileDto).GetProperty("PhoneNumber"));
        Assert.Null(typeof(EmployerProfileDto).GetProperty("FirstName"));
        Assert.Null(typeof(EmployerProfileDto).GetProperty("LastName"));
        Assert.Null(typeof(EmployerProfileDto).GetProperty("Email"));
    }

    [Fact]
    public async Task GetProfile_WithNonEmployerRole_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Nurse"), []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new GetMyEmployerProfileQuery(), CancellationToken.None));
    }

    private GetMyEmployerProfileQueryHandler CreateHandler(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<EmployerProfile> profiles)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);

        return new GetMyEmployerProfileQueryHandler(
            _contextMock.Object,
            new EmployerRoleGuard(_contextMock.Object, _currentUserMock.Object));
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
