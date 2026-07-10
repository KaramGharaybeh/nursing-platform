using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Commands.CreateNurseExperience;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseExperience;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseExperience;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class NurseExperienceCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Create_ValidExperience_AddsRecordForCurrentNurseProfile()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var country = CreateCountry(Guid.NewGuid(), "United Kingdom", true);
        var experiences = new List<NurseExperience>().AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], [country], experiences);
        var handler = CreateCreateHandler();

        var command = new CreateNurseExperienceCommand
        {
            FacilityName = "General Hospital",
            JobTitle = "Registered Nurse",
            CountryId = country.Id,
            StartDate = new DateOnly(2022, 1, 1),
            EndDate = new DateOnly(2023, 1, 1),
            IsCurrent = false,
            Description = "Emergency department"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(command.FacilityName, result.FacilityName);
        Assert.Equal(command.JobTitle, result.JobTitle);
        Assert.Equal(country.Id, result.CountryId);
        Assert.Equal("United Kingdom", result.CountryName);
        experiences.Verify(e => e.Add(It.Is<NurseExperience>(x =>
            x.NurseProfileId == profile.Id &&
            x.FacilityName == command.FacilityName &&
            x.JobTitle == command.JobTitle &&
            x.CountryId == country.Id &&
            x.StartDate == command.StartDate &&
            x.EndDate == command.EndDate &&
            !x.IsCurrent &&
            x.Description == command.Description)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_RecordOwnedByAnotherProfile_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var currentProfile = CreateProfile(userId);
        var otherProfile = CreateProfile(Guid.NewGuid());
        var experience = CreateExperience(otherProfile.Id, new DateOnly(2021, 1, 1));
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [currentProfile, otherProfile], [experience], []);
        var handler = CreateUpdateHandler();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new UpdateNurseExperienceCommand
            {
                Id = experience.Id,
                FacilityName = "Updated Hospital",
                JobTitle = "Senior Nurse",
                StartDate = new DateOnly(2022, 1, 1)
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_RecordOwnedByCurrentProfile_RemovesRecord()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var experience = CreateExperience(profile.Id, new DateOnly(2021, 1, 1));
        var experiences = new List<NurseExperience> { experience }.AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [experience], [], experiences);
        var handler = CreateDeleteHandler();

        await handler.Handle(new DeleteNurseExperienceCommand(experience.Id), CancellationToken.None);

        experiences.Verify(e => e.Remove(experience), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_InactiveCountry_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var inactiveCountry = CreateCountry(Guid.NewGuid(), "Inactive", false);
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], [inactiveCountry]);
        var handler = CreateCreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateNurseExperienceCommand
            {
                FacilityName = "General Hospital",
                JobTitle = "Registered Nurse",
                CountryId = inactiveCountry.Id,
                StartDate = new DateOnly(2022, 1, 1)
            }, CancellationToken.None));
    }

    [Fact]
    public async Task NonNurse_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        ConfigureContext(userId, CreateUserWithRole(userId, "Employer"), [], [], []);
        var handler = CreateCreateHandler();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CreateNurseExperienceCommand
            {
                FacilityName = "General Hospital",
                JobTitle = "Registered Nurse",
                StartDate = new DateOnly(2022, 1, 1)
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Create_NurseWithoutProfile_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [], [], []);
        var handler = CreateCreateHandler();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new CreateNurseExperienceCommand
            {
                FacilityName = "General Hospital",
                JobTitle = "Registered Nurse",
                StartDate = new DateOnly(2022, 1, 1)
            }, CancellationToken.None));
    }

    private CreateNurseExperienceCommandHandler CreateCreateHandler()
    {
        return new CreateNurseExperienceCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private UpdateNurseExperienceCommandHandler CreateUpdateHandler()
    {
        return new UpdateNurseExperienceCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private DeleteNurseExperienceCommandHandler CreateDeleteHandler()
    {
        return new DeleteNurseExperienceCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private void ConfigureContext(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseExperience> experiences,
        IReadOnlyCollection<Country> countries,
        Mock<Microsoft.EntityFrameworkCore.DbSet<NurseExperience>>? experienceMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseExperiences).Returns(experienceMock?.Object ?? experiences.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static NurseProfile CreateProfile(Guid userId)
    {
        return new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
    }

    private static NurseExperience CreateExperience(Guid profileId, DateOnly startDate)
    {
        return new NurseExperience
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profileId,
            FacilityName = "General Hospital",
            JobTitle = "Registered Nurse",
            StartDate = startDate
        };
    }

    private static Country CreateCountry(Guid id, string name, bool isActive)
    {
        return new Country { Id = id, Name = name, Code = "GB", IsActive = isActive };
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
