using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerProfile;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Employers;

public class UpsertMyEmployerProfileCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task UpsertProfile_WhenProfileDoesNotExist_CreatesProfileForCurrentUser()
    {
        var userId = Guid.NewGuid();
        var profileMock = new List<EmployerProfile>().AsQueryable().BuildMockDbSet();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [], profileMock);

        var result = await handler.Handle(new UpsertMyEmployerProfileCommand
        {
            JobTitle = "Recruitment Manager",
            Department = "Talent Acquisition"
        }, CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        Assert.Equal("Recruitment Manager", result.JobTitle);
        Assert.Equal("Talent Acquisition", result.Department);
        _contextMock.Verify(c => c.EmployerProfiles.Add(It.Is<EmployerProfile>(p =>
            p.UserId == userId &&
            p.JobTitle == "Recruitment Manager" &&
            p.Department == "Talent Acquisition")), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertProfile_WhenProfileExists_UpdatesExistingProfileWithoutDuplicate()
    {
        var userId = Guid.NewGuid();
        var profile = new EmployerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobTitle = "Old title",
            Department = "Old department"
        };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile]);

        var result = await handler.Handle(new UpsertMyEmployerProfileCommand
        {
            JobTitle = "Updated title",
            Department = "Updated department"
        }, CancellationToken.None);

        Assert.Equal(profile.Id, result.Id);
        Assert.Equal("Updated title", profile.JobTitle);
        Assert.Equal("Updated department", profile.Department);
        _contextMock.Verify(c => c.EmployerProfiles.Add(It.IsAny<EmployerProfile>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertProfile_TrimsProfileFieldsBeforeSaving()
    {
        var userId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile]);

        var result = await handler.Handle(new UpsertMyEmployerProfileCommand
        {
            JobTitle = "  Recruitment Manager  ",
            Department = "   "
        }, CancellationToken.None);

        Assert.Equal("Recruitment Manager", profile.JobTitle);
        Assert.Null(profile.Department);
        Assert.Equal("Recruitment Manager", result.JobTitle);
        Assert.Null(result.Department);
    }

    [Fact]
    public async Task UpsertProfile_WithNonEmployerRole_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Nurse"), []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new UpsertMyEmployerProfileCommand(), CancellationToken.None));
    }

    private UpsertMyEmployerProfileCommandHandler CreateHandler(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<EmployerProfile> profiles,
        Mock<Microsoft.EntityFrameworkCore.DbSet<EmployerProfile>>? profileMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerProfiles).Returns(profileMock?.Object ?? profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new UpsertMyEmployerProfileCommandHandler(
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
