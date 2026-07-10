using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Commands.UpsertNurseProfile;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class UpsertNurseProfileCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Handle_NurseWithoutProfile_CreatesProfile()
    {
        var userId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var profiles = new List<NurseProfile>().AsQueryable().BuildMockDbSet();
        var handler = CreateHandler(userId, CreateNurseUser(userId), [], [CreateActiveCountry(countryId, "United Kingdom")], profiles);

        var command = new UpsertNurseProfileCommand
        {
            Headline = "Registered Nurse",
            ProfessionalSummary = "ICU nurse",
            LicenseNumber = "RN-123",
            LicenseCountryId = countryId,
            CurrentCountryId = countryId,
            YearsOfExperience = 5,
            IsAvailableForRecruitment = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        Assert.Equal("Registered Nurse", result.Headline);
        Assert.Equal("United Kingdom", result.LicenseCountryName);
        _contextMock.Verify(c => c.NurseProfiles.Add(It.Is<NurseProfile>(p =>
            p.UserId == userId &&
            p.Headline == command.Headline &&
            p.IsAvailableForRecruitment)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NurseWithProfile_UpdatesProfile()
    {
        var userId = Guid.NewGuid();
        var profile = new NurseProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Headline = "Old headline",
            YearsOfExperience = 1
        };
        var profiles = new List<NurseProfile> { profile }.AsQueryable().BuildMockDbSet();
        var handler = CreateHandler(userId, CreateNurseUser(userId), [profile], [], profiles);

        var result = await handler.Handle(new UpsertNurseProfileCommand
        {
            Headline = "Updated headline",
            YearsOfExperience = 6,
            IsAvailableForRecruitment = true
        }, CancellationToken.None);

        Assert.Equal(profile.Id, result.Id);
        Assert.Equal("Updated headline", profile.Headline);
        Assert.Equal(6, profile.YearsOfExperience);
        Assert.True(profile.IsAvailableForRecruitment);
        _contextMock.Verify(c => c.NurseProfiles.Add(It.IsAny<NurseProfile>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonNurse_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [], []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new UpsertNurseProfileCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MissingCurrentUser_ThrowsUnauthorizedAccessException()
    {
        var handler = CreateHandler(null, null, [], []);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new UpsertNurseProfileCommand(), CancellationToken.None));

        Assert.Equal("User is not authenticated.", exception.Message);
    }

    [Fact]
    public async Task Handle_InactiveCountry_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var inactiveCountryId = Guid.NewGuid();
        var handler = CreateHandler(
            userId,
            CreateNurseUser(userId),
            [],
            [new Country { Id = inactiveCountryId, Name = "Inactive", Code = "IN", IsActive = false }]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpsertNurseProfileCommand
            {
                LicenseCountryId = inactiveCountryId,
                YearsOfExperience = 2
            }, CancellationToken.None));
    }

    private UpsertNurseProfileCommandHandler CreateHandler(
        Guid? currentUserId,
        User? user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<Country> countries,
        Mock<Microsoft.EntityFrameworkCore.DbSet<NurseProfile>>? profileMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns((user is null ? [] : new[] { user }).AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profileMock?.Object ?? profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new UpsertNurseProfileCommandHandler(
            _contextMock.Object,
            _currentUserMock.Object,
            new NursingPlatform.Application.Nurses.Common.NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private static User CreateNurseUser(Guid userId) => CreateUserWithRole(userId, "Nurse");

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

    private static Country CreateActiveCountry(Guid id, string name)
    {
        return new Country { Id = id, Name = name, Code = "GB", IsActive = true };
    }
}
