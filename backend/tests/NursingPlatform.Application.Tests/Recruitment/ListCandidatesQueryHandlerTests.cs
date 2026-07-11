using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Application.Recruitment.Queries.ListCandidates;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Recruitment;

public class ListCandidatesQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Handle_WhenCurrentUserIsUnauthenticated_ThrowsUnauthorizedAccessException()
    {
        var handler = CreateHandler(null, [], [], [], [], [], [], [], [], []);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new ListCandidatesQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNotEmployer_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, [CreateUserWithRole(userId, "Nurse")], [], [], [], [], [], [], [], []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new ListCandidatesQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEmployerProfileIsMissing_ThrowsForbiddenAccessExceptionAndDoesNotQueryCandidates()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandlerWithCandidateSetsThatThrow(userId, [CreateUserWithRole(userId, "Employer")], [], []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new ListCandidatesQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEmployerOrganizationIsMissing_ThrowsForbiddenAccessExceptionAndDoesNotQueryCandidates()
    {
        var userId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var handler = CreateHandlerWithCandidateSetsThatThrow(userId, [CreateUserWithRole(userId, "Employer")], [profile], []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new ListCandidatesQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsOnlyRecruitmentVisibleActiveEmailVerifiedNurses()
    {
        var employerUserId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = employerUserId };
        var organization = new EmployerOrganization { Id = Guid.NewGuid(), EmployerProfileId = profile.Id, Name = "General Hospital" };

        var eligible = CreateCandidate("Eligible", 7, true, true, true, new DateTime(2026, 7, 4, 0, 0, 0, DateTimeKind.Utc));
        var unavailable = CreateCandidate("Unavailable", 9, false, true, true, new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc));
        var inactive = CreateCandidate("Inactive", 8, true, false, true, new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc));
        var unverified = CreateCandidate("Unverified", 10, true, true, false, new DateTime(2026, 7, 7, 0, 0, 0, DateTimeKind.Utc));

        var handler = CreateHandler(
            employerUserId,
            [CreateUserWithRole(employerUserId, "Employer")],
            [profile],
            [organization],
            [eligible.Profile, unavailable.Profile, inactive.Profile, unverified.Profile],
            [],
            [],
            [],
            [],
            []);

        var result = await handler.Handle(new ListCandidatesQuery(), CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        var item = Assert.Single(result.Items);
        Assert.Equal(eligible.Profile.Id, item.NurseProfileId);
        Assert.Equal("Eligible", item.Headline);
    }

    [Fact]
    public async Task Handle_Pagination_SkipsAndTakesAfterDeterministicSorting()
    {
        var employerUserId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = employerUserId };
        var organization = new EmployerOrganization { Id = Guid.NewGuid(), EmployerProfileId = profile.Id, Name = "General Hospital" };

        var candidates = Enumerable.Range(1, 25)
            .Select(i => CreateCandidate($"Candidate {i}", i, true, true, true, new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(i)))
            .ToList();

        var handler = CreateHandler(
            employerUserId,
            [CreateUserWithRole(employerUserId, "Employer")],
            [profile],
            [organization],
            candidates.Select(c => c.Profile).ToList(),
            [],
            [],
            [],
            [],
            []);

        var result = await handler.Handle(new ListCandidatesQuery { Page = 2, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal("Candidate 15", result.Items[0].Headline);
        Assert.Equal("Candidate 6", result.Items[^1].Headline);
    }

    [Fact]
    public async Task Handle_DefaultSorting_OrdersByExperienceThenCreatedAtThenId()
    {
        var employerUserId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = employerUserId };
        var organization = new EmployerOrganization { Id = Guid.NewGuid(), EmployerProfileId = profile.Id, Name = "General Hospital" };

        var olderLowerExperience = CreateCandidate("Older lower experience", 8, true, true, true, new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
        olderLowerExperience.Profile.Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var highestExperience = CreateCandidate("Highest experience", 10, true, true, true, new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
        highestExperience.Profile.Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var newerHigherId = CreateCandidate("Newer higher id", 8, true, true, true, new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc));
        newerHigherId.Profile.Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var newerLowerId = CreateCandidate("Newer lower id", 8, true, true, true, new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc));
        newerLowerId.Profile.Id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var handler = CreateHandler(
            employerUserId,
            [CreateUserWithRole(employerUserId, "Employer")],
            [profile],
            [organization],
            [olderLowerExperience.Profile, highestExperience.Profile, newerHigherId.Profile, newerLowerId.Profile],
            [],
            [],
            [],
            [],
            []);

        var result = await handler.Handle(new ListCandidatesQuery(), CancellationToken.None);

        Assert.Collection(
            result.Items,
            item => Assert.Equal("Highest experience", item.Headline),
            item => Assert.Equal("Newer lower id", item.Headline),
            item => Assert.Equal("Newer higher id", item.Headline),
            item => Assert.Equal("Older lower experience", item.Headline));
    }

    [Fact]
    public async Task Handle_ProjectsOnlyAllowedCandidateSummaryFields()
    {
        var employerUserId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = employerUserId };
        var organization = new EmployerOrganization { Id = Guid.NewGuid(), EmployerProfileId = profile.Id, Name = "General Hospital" };
        var licenseCountryId = Guid.NewGuid();
        var currentCountryId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        var candidate = CreateCandidate("Critical care nurse", 8, true, true, true, new DateTime(2026, 7, 4, 0, 0, 0, DateTimeKind.Utc));
        candidate.Profile.ProfessionalSummary = "Experienced ICU nurse";
        candidate.Profile.LicenseCountryId = licenseCountryId;
        candidate.Profile.CurrentCountryId = currentCountryId;

        var currentExperience = new NurseExperience
        {
            Id = Guid.NewGuid(),
            NurseProfileId = candidate.Profile.Id,
            JobTitle = "Senior ICU Nurse",
            FacilityName = "General Hospital",
            StartDate = new DateOnly(2024, 1, 1),
            IsCurrent = true
        };
        var olderExperience = new NurseExperience
        {
            Id = Guid.NewGuid(),
            NurseProfileId = candidate.Profile.Id,
            JobTitle = "Staff Nurse",
            FacilityName = "City Hospital",
            StartDate = new DateOnly(2025, 1, 1),
            IsCurrent = false
        };
        var education = new NurseEducation
        {
            Id = Guid.NewGuid(),
            NurseProfileId = candidate.Profile.Id,
            InstitutionName = "Nursing University",
            Degree = "Bachelor",
            FieldOfStudy = "Nursing"
        };
        var certificates = new[]
        {
            new NurseCertificate { Id = Guid.NewGuid(), NurseProfileId = candidate.Profile.Id, Name = "BLS", IssuingOrganization = "AHA" },
            new NurseCertificate { Id = Guid.NewGuid(), NurseProfileId = candidate.Profile.Id, Name = "ACLS", IssuingOrganization = "AHA" }
        };
        var language = new Language { Id = languageId, Name = "English", Code = "en", IsActive = true };
        var nurseLanguage = new NurseLanguage
        {
            Id = Guid.NewGuid(),
            NurseProfileId = candidate.Profile.Id,
            LanguageId = languageId,
            Language = language,
            Proficiency = "Fluent"
        };
        var skills = new[]
        {
            new NurseSkill { Id = Guid.NewGuid(), NurseProfileId = candidate.Profile.Id, Name = "Triage", NormalizedName = "TRIAGE" },
            new NurseSkill { Id = Guid.NewGuid(), NurseProfileId = candidate.Profile.Id, Name = "ICU", NormalizedName = "ICU" }
        };

        var handler = CreateHandler(
            employerUserId,
            [CreateUserWithRole(employerUserId, "Employer")],
            [profile],
            [organization],
            [candidate.Profile],
            [new Country { Id = licenseCountryId, Name = "Canada", Code = "CA", IsActive = true }, new Country { Id = currentCountryId, Name = "United Kingdom", Code = "GB", IsActive = true }],
            [currentExperience, olderExperience],
            [education],
            certificates,
            [nurseLanguage],
            skills);

        var result = await handler.Handle(new ListCandidatesQuery(), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(candidate.Profile.Id, item.NurseProfileId);
        Assert.Equal("Critical care nurse", item.Headline);
        Assert.Equal("Experienced ICU nurse", item.ProfessionalSummary);
        Assert.Equal("Canada", item.LicenseCountryName);
        Assert.Equal("United Kingdom", item.CurrentCountryName);
        Assert.Equal(8, item.YearsOfExperience);
        Assert.Equal(["ICU", "Triage"], item.Skills);
        var languageDto = Assert.Single(item.Languages);
        Assert.Equal("English", languageDto.Name);
        Assert.Equal("en", languageDto.Code);
        Assert.Equal("Fluent", languageDto.Proficiency);
        Assert.Equal("2 certificates", item.CertificatesSummary);
        Assert.Equal(2, item.CertificatesCount);
        Assert.Equal("Senior ICU Nurse", item.LatestExperienceTitle);
        Assert.Equal("Bachelor of Nursing", item.EducationSummary);

        AssertForbiddenCandidateDtoPropertiesAreAbsent();
    }

    private ListCandidatesQueryHandler CreateHandler(
        Guid? currentUserId,
        IReadOnlyCollection<User> users,
        IReadOnlyCollection<EmployerProfile> employerProfiles,
        IReadOnlyCollection<EmployerOrganization> employerOrganizations,
        IReadOnlyCollection<NurseProfile> nurseProfiles,
        IReadOnlyCollection<Country> countries,
        IReadOnlyCollection<NurseExperience> experiences,
        IReadOnlyCollection<NurseEducation> education,
        IReadOnlyCollection<NurseCertificate> certificates,
        IReadOnlyCollection<NurseLanguage> languages,
        IReadOnlyCollection<NurseSkill>? skills = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerProfiles).Returns(employerProfiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerOrganizations).Returns(employerOrganizations.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(nurseProfiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseExperiences).Returns(experiences.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseEducation).Returns(education.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseCertificates).Returns(certificates.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseLanguages).Returns(languages.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseSkills).Returns((skills ?? []).AsQueryable().BuildMockDbSet().Object);

        return new ListCandidatesQueryHandler(
            _contextMock.Object,
            new EmployerRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private ListCandidatesQueryHandler CreateHandlerWithCandidateSetsThatThrow(
        Guid? currentUserId,
        IReadOnlyCollection<User> users,
        IReadOnlyCollection<EmployerProfile> employerProfiles,
        IReadOnlyCollection<EmployerOrganization> employerOrganizations)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerProfiles).Returns(employerProfiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerOrganizations).Returns(employerOrganizations.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Throws(new InvalidOperationException("Candidate profiles must not be queried before employer prerequisites pass."));
        _contextMock.Setup(c => c.NurseExperiences).Throws(new InvalidOperationException("Candidate experiences must not be queried before employer prerequisites pass."));
        _contextMock.Setup(c => c.NurseEducation).Throws(new InvalidOperationException("Candidate education must not be queried before employer prerequisites pass."));
        _contextMock.Setup(c => c.NurseCertificates).Throws(new InvalidOperationException("Candidate certificates must not be queried before employer prerequisites pass."));
        _contextMock.Setup(c => c.NurseLanguages).Throws(new InvalidOperationException("Candidate languages must not be queried before employer prerequisites pass."));
        _contextMock.Setup(c => c.NurseSkills).Throws(new InvalidOperationException("Candidate skills must not be queried before employer prerequisites pass."));

        return new ListCandidatesQueryHandler(
            _contextMock.Object,
            new EmployerRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private static (NurseProfile Profile, User User) CreateCandidate(
        string headline,
        int yearsOfExperience,
        bool isAvailableForRecruitment,
        bool isActive,
        bool emailVerified,
        DateTime createdAt)
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = $"{headline.Replace(" ", string.Empty).ToLowerInvariant()}@example.com",
            FirstName = "Hidden",
            LastName = "Candidate",
            PasswordHash = "hash",
            IsActive = isActive,
            EmailVerified = emailVerified,
            CreatedAt = createdAt
        };

        var profile = new NurseProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            Headline = headline,
            ProfessionalSummary = $"{headline} summary",
            LicenseNumber = "hidden-license",
            YearsOfExperience = yearsOfExperience,
            IsAvailableForRecruitment = isAvailableForRecruitment,
            CreatedAt = createdAt
        };

        return (profile, user);
    }

    private static User CreateUserWithRole(Guid userId, string roleName)
    {
        var roleId = Guid.NewGuid();
        return new User
        {
            Id = userId,
            IsActive = true,
            EmailVerified = true,
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

    private static void AssertForbiddenCandidateDtoPropertiesAreAbsent()
    {
        var forbiddenProperties = new[]
        {
            "UserId",
            "Email",
            "Phone",
            "PhoneNumber",
            "PasswordHash",
            "Roles",
            "Permissions",
            "Tokens",
            "AccessToken",
            "RefreshToken",
            "TokenHash",
            "EmailVerified",
            "IsActive",
            "LicenseNumber",
            "CvStorageKey",
            "CvFileUrl",
            "StorageKey",
            "InternalPath",
            "FileUrl",
            "FirstName",
            "LastName",
            "DisplayLabel",
            "User",
            "NurseProfile",
            "LicenseCountry",
            "CurrentCountry",
            "Country"
        };

        foreach (var propertyName in forbiddenProperties)
        {
            Assert.Null(typeof(CandidateListItemDto).GetProperty(propertyName));
            Assert.Null(typeof(CandidateLanguageDto).GetProperty(propertyName));
        }
    }
}
