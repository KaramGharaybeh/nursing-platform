using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.GetCurrentNurseProfile;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Queries;

public class GetCurrentNurseProfileQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task GetCurrentNurseProfile_ExistingProfile_ReturnsDtoWithoutSensitiveFields()
    {
        var userId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var profile = new NurseProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Headline = "Registered Nurse",
            ProfessionalSummary = "ICU nurse",
            LicenseNumber = "RN-123",
            LicenseCountryId = countryId,
            CurrentCountryId = countryId,
            YearsOfExperience = 5,
            IsAvailableForRecruitment = true
        };
        var handler = CreateHandler(userId, CreateNurseUser(userId), [profile], [new Country { Id = countryId, Name = "United Kingdom", Code = "GB", IsActive = true }]);

        var result = await handler.Handle(new GetCurrentNurseProfileQuery(), CancellationToken.None);

        Assert.Equal(profile.Id, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Registered Nurse", result.Headline);
        Assert.Equal("United Kingdom", result.LicenseCountryName);
        Assert.Equal("United Kingdom", result.CurrentCountryName);
        Assert.True(result.IsAvailableForRecruitment);
        Assert.Null(typeof(NurseProfileDto).GetProperty("PasswordHash"));
        Assert.Null(typeof(NurseProfileDto).GetProperty("Roles"));
        Assert.Null(typeof(NurseProfileDto).GetProperty("Permissions"));
        Assert.Null(typeof(NurseProfileDto).GetProperty("StorageKey"));
        Assert.Null(typeof(NurseProfileDto).GetProperty("User"));
    }

    [Fact]
    public async Task GetCurrentNurseProfile_NoProfile_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, CreateNurseUser(userId), [], []);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetCurrentNurseProfileQuery(), CancellationToken.None));
    }

    private GetCurrentNurseProfileQueryHandler CreateHandler(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<Country> countries)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);

        return new GetCurrentNurseProfileQueryHandler(
            _contextMock.Object,
            _currentUserMock.Object,
            new NursingPlatform.Application.Nurses.Common.NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
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
