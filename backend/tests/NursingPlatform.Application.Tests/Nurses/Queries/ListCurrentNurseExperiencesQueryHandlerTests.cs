using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseExperiences;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Queries;

public class ListCurrentNurseExperiencesQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task List_ReturnsRecordsSortedByStartDateDescending()
    {
        var userId = Guid.NewGuid();
        var profile = new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
        var country = new Country { Id = Guid.NewGuid(), Name = "United Kingdom", Code = "GB", IsActive = true };
        var oldest = CreateExperience(profile.Id, "Old Hospital", new DateOnly(2020, 1, 1), new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), country.Id);
        var newest = CreateExperience(profile.Id, "Newest Hospital", new DateOnly(2024, 1, 1), new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), country.Id);
        var tieBreakerNewest = CreateExperience(profile.Id, "Tie Newest Hospital", new DateOnly(2024, 1, 1), new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), null);
        var otherProfileExperience = CreateExperience(Guid.NewGuid(), "Other Hospital", new DateOnly(2025, 1, 1), DateTime.UtcNow, country.Id);
        ConfigureContext(
            userId,
            CreateNurseUser(userId),
            [profile],
            [oldest, newest, tieBreakerNewest, otherProfileExperience],
            [country]);
        var handler = new ListCurrentNurseExperiencesQueryHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));

        var result = await handler.Handle(new ListCurrentNurseExperiencesQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first => Assert.Equal(tieBreakerNewest.Id, first.Id),
            second => Assert.Equal(newest.Id, second.Id),
            third => Assert.Equal(oldest.Id, third.Id));
        Assert.All(result, item => Assert.IsType<NurseExperienceDto>(item));
        Assert.Equal("United Kingdom", result.Single(item => item.Id == newest.Id).CountryName);
        Assert.DoesNotContain(result, item => item.Id == otherProfileExperience.Id);
        Assert.Null(typeof(NurseExperienceDto).GetProperty("NurseProfile"));
        Assert.Null(typeof(NurseExperienceDto).GetProperty("Country"));
        Assert.Null(typeof(NurseExperienceDto).GetProperty("User"));
    }

    private void ConfigureContext(
        Guid currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseExperience> experiences,
        IReadOnlyCollection<Country> countries)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseExperiences).Returns(experiences.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
    }

    private static NurseExperience CreateExperience(
        Guid profileId,
        string facilityName,
        DateOnly startDate,
        DateTime createdAt,
        Guid? countryId)
    {
        return new NurseExperience
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profileId,
            FacilityName = facilityName,
            JobTitle = "Registered Nurse",
            CountryId = countryId,
            StartDate = startDate,
            CreatedAt = createdAt
        };
    }

    private static User CreateNurseUser(Guid userId)
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
                    Role = new Role { Id = roleId, Name = "Nurse" }
                }
            ]
        };
    }
}
