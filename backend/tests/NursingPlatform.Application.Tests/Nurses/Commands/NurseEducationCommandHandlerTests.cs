using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseEducation;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseEducation;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class NurseEducationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task CreateEducation_ValidRequest_AddsRecordForCurrentNurseProfile()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var country = CreateCountry(Guid.NewGuid(), "United Kingdom", true);
        var educationSet = new List<NurseEducation>().AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], [country], educationSet);
        var handler = CreateCreateHandler();

        var command = new CreateNurseEducationCommand
        {
            InstitutionName = "University of Nursing",
            Degree = "Bachelor of Nursing",
            FieldOfStudy = "Adult Nursing",
            CountryId = country.Id,
            StartDate = new DateOnly(2018, 9, 1),
            EndDate = new DateOnly(2022, 6, 30),
            Description = "Clinical nursing program"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(command.InstitutionName, result.InstitutionName);
        Assert.Equal(command.Degree, result.Degree);
        Assert.Equal(country.Id, result.CountryId);
        Assert.Equal("United Kingdom", result.CountryName);
        educationSet.Verify(e => e.Add(It.Is<NurseEducation>(x =>
            x.NurseProfileId == profile.Id &&
            x.InstitutionName == command.InstitutionName &&
            x.Degree == command.Degree &&
            x.FieldOfStudy == command.FieldOfStudy &&
            x.CountryId == country.Id &&
            x.StartDate == command.StartDate &&
            x.EndDate == command.EndDate &&
            x.Description == command.Description)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEducation_RecordOwnedByAnotherProfile_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var currentProfile = CreateProfile(userId);
        var otherProfile = CreateProfile(Guid.NewGuid());
        var education = CreateEducation(otherProfile.Id, new DateOnly(2020, 1, 1), new DateOnly(2022, 1, 1));
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [currentProfile, otherProfile], [education], []);
        var handler = CreateUpdateHandler();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new UpdateNurseEducationCommand
            {
                Id = education.Id,
                InstitutionName = "Updated University",
                Degree = "Updated Degree"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteEducation_RecordOwnedByCurrentProfile_RemovesRecord()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var education = CreateEducation(profile.Id, new DateOnly(2020, 1, 1), new DateOnly(2022, 1, 1));
        var educationSet = new List<NurseEducation> { education }.AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [education], [], educationSet);
        var handler = CreateDeleteHandler();

        await handler.Handle(new DeleteNurseEducationCommand(education.Id), CancellationToken.None);

        educationSet.Verify(e => e.Remove(education), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEducation_InactiveCountry_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var inactiveCountry = CreateCountry(Guid.NewGuid(), "Inactive", false);
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], [inactiveCountry]);
        var handler = CreateCreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateNurseEducationCommand
            {
                InstitutionName = "University of Nursing",
                Degree = "Bachelor of Nursing",
                CountryId = inactiveCountry.Id
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Education_NonNurse_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        ConfigureContext(userId, CreateUserWithRole(userId, "Employer"), [], [], []);
        var handler = CreateCreateHandler();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CreateNurseEducationCommand
            {
                InstitutionName = "University of Nursing",
                Degree = "Bachelor of Nursing"
            }, CancellationToken.None));
    }

    private CreateNurseEducationCommandHandler CreateCreateHandler()
    {
        return new CreateNurseEducationCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private UpdateNurseEducationCommandHandler CreateUpdateHandler()
    {
        return new UpdateNurseEducationCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private DeleteNurseEducationCommandHandler CreateDeleteHandler()
    {
        return new DeleteNurseEducationCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private void ConfigureContext(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseEducation> education,
        IReadOnlyCollection<Country> countries,
        Mock<Microsoft.EntityFrameworkCore.DbSet<NurseEducation>>? educationMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseEducation).Returns(educationMock?.Object ?? education.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static NurseProfile CreateProfile(Guid userId)
    {
        return new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
    }

    private static NurseEducation CreateEducation(Guid profileId, DateOnly? startDate, DateOnly? endDate)
    {
        return new NurseEducation
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profileId,
            InstitutionName = "University of Nursing",
            Degree = "Bachelor of Nursing",
            StartDate = startDate,
            EndDate = endDate
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
