using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Employers.DTOs;
using NursingPlatform.Application.Employers.Queries.GetMyEmployerOrganization;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Employers;

public class GetMyEmployerOrganizationQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task GetOrganization_WhenProfileDoesNotExist_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [], [], []);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetMyEmployerOrganizationQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetOrganization_WhenOrganizationDoesNotExist_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile], [], []);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetMyEmployerOrganizationQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetOrganization_WhenOrganizationExists_ReturnsOrganizationDto()
    {
        var userId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var organization = new EmployerOrganization
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = profile.Id,
            Name = "General Hospital",
            Type = "Hospital",
            WebsiteUrl = "https://general.example.com",
            CountryId = countryId,
            City = "Toronto",
            AddressLine1 = "100 Care Street",
            AddressLine2 = "Suite 200",
            PostalCode = "A1B 2C3",
            Description = "Regional healthcare organization"
        };
        var country = new Country { Id = countryId, Name = "Canada", Code = "CA", IsActive = true };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile], [organization], [country]);

        var result = await handler.Handle(new GetMyEmployerOrganizationQuery(), CancellationToken.None);

        Assert.Equal(organization.Id, result.Id);
        Assert.Equal(profile.Id, result.EmployerProfileId);
        Assert.Equal("General Hospital", result.Name);
        Assert.Equal("Hospital", result.Type);
        Assert.Equal("https://general.example.com", result.WebsiteUrl);
        Assert.Equal(countryId, result.CountryId);
        Assert.Equal("Canada", result.CountryName);
        Assert.Equal("Toronto", result.City);
        Assert.Equal("100 Care Street", result.AddressLine1);
        Assert.Equal("Suite 200", result.AddressLine2);
        Assert.Equal("A1B 2C3", result.PostalCode);
        Assert.Equal("Regional healthcare organization", result.Description);
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("PasswordHash"));
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("Tokens"));
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("Roles"));
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("Permissions"));
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("EmployerProfile"));
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("Country"));
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("PhoneNumber"));
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("LogoStorageKey"));
        Assert.Null(typeof(EmployerOrganizationDto).GetProperty("InvitationToken"));
    }

    [Fact]
    public async Task GetOrganization_WithNonEmployerRole_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Nurse"), [], [], []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new GetMyEmployerOrganizationQuery(), CancellationToken.None));
    }

    private GetMyEmployerOrganizationQueryHandler CreateHandler(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<EmployerProfile> profiles,
        IReadOnlyCollection<EmployerOrganization> organizations,
        IReadOnlyCollection<Country> countries)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerOrganizations).Returns(organizations.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);

        return new GetMyEmployerOrganizationQueryHandler(
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
