using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseEducation;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Queries;

public class ListCurrentNurseEducationQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task ListEducation_ReturnsRecordsSortedByEndDateDescendingNullsFirstThenStartDateDescending()
    {
        var userId = Guid.NewGuid();
        var profile = new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
        var country = new Country { Id = Guid.NewGuid(), Name = "United Kingdom", Code = "GB", IsActive = true };
        var current = CreateEducation(profile.Id, "Current University", new DateOnly(2022, 1, 1), null, country.Id);
        var latestEnded = CreateEducation(profile.Id, "Latest Ended University", new DateOnly(2018, 1, 1), new DateOnly(2021, 1, 1), country.Id);
        var earlierEnded = CreateEducation(profile.Id, "Earlier Ended University", new DateOnly(2019, 1, 1), new DateOnly(2020, 1, 1), null);
        var otherProfileEducation = CreateEducation(Guid.NewGuid(), "Other University", new DateOnly(2024, 1, 1), null, country.Id);
        ConfigureContext(
            userId,
            CreateNurseUser(userId),
            [profile],
            [latestEnded, current, otherProfileEducation, earlierEnded],
            [country]);
        var handler = new ListCurrentNurseEducationQueryHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));

        var result = await handler.Handle(new ListCurrentNurseEducationQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first => Assert.Equal(current.Id, first.Id),
            second => Assert.Equal(latestEnded.Id, second.Id),
            third => Assert.Equal(earlierEnded.Id, third.Id));
        Assert.All(result, item => Assert.IsType<NurseEducationDto>(item));
        Assert.Equal("United Kingdom", result.Single(item => item.Id == current.Id).CountryName);
        Assert.DoesNotContain(result, item => item.Id == otherProfileEducation.Id);
        Assert.Null(typeof(NurseEducationDto).GetProperty("NurseProfile"));
        Assert.Null(typeof(NurseEducationDto).GetProperty("Country"));
        Assert.Null(typeof(NurseEducationDto).GetProperty("User"));
    }

    private void ConfigureContext(
        Guid currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseEducation> education,
        IReadOnlyCollection<Country> countries)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseEducation).Returns(education.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
    }

    private static NurseEducation CreateEducation(
        Guid profileId,
        string institutionName,
        DateOnly? startDate,
        DateOnly? endDate,
        Guid? countryId)
    {
        return new NurseEducation
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profileId,
            InstitutionName = institutionName,
            Degree = "Bachelor of Nursing",
            CountryId = countryId,
            StartDate = startDate,
            EndDate = endDate
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
