using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Employers;

public class UpsertMyEmployerOrganizationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task UpsertOrganization_WhenProfileDoesNotExist_CreatesProfileAndOrganizationForCurrentUser()
    {
        var userId = Guid.NewGuid();
        var profileMock = new List<EmployerProfile>().AsQueryable().BuildMockDbSet();
        var organizationMock = new List<EmployerOrganization>().AsQueryable().BuildMockDbSet();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [], [], [], profileMock, organizationMock);

        var result = await handler.Handle(new UpsertMyEmployerOrganizationCommand { Name = "General Hospital" }, CancellationToken.None);

        Assert.Equal("General Hospital", result.Name);
        _contextMock.Verify(c => c.EmployerProfiles.Add(It.Is<EmployerProfile>(p =>
            p.UserId == userId && p.Id != Guid.Empty)), Times.Once);
        _contextMock.Verify(c => c.EmployerOrganizations.Add(It.Is<EmployerOrganization>(o =>
            o.EmployerProfileId != Guid.Empty && o.Name == "General Hospital")), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertOrganization_WhenOrganizationExists_UpdatesExistingOrganizationWithoutDuplicate()
    {
        var userId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var organization = new EmployerOrganization
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = profile.Id,
            Name = "Old name"
        };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile], [organization], []);

        var result = await handler.Handle(new UpsertMyEmployerOrganizationCommand
        {
            Name = "Updated name",
            Type = "Hospital"
        }, CancellationToken.None);

        Assert.Equal(organization.Id, result.Id);
        Assert.Equal("Updated name", organization.Name);
        Assert.Equal("Hospital", organization.Type);
        _contextMock.Verify(c => c.EmployerProfiles.Add(It.IsAny<EmployerProfile>()), Times.Never);
        _contextMock.Verify(c => c.EmployerOrganizations.Add(It.IsAny<EmployerOrganization>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertOrganization_TrimsOrganizationFieldsBeforeSaving()
    {
        var userId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var organization = new EmployerOrganization { Id = Guid.NewGuid(), EmployerProfileId = profile.Id, Name = "Old" };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile], [organization], []);

        var result = await handler.Handle(new UpsertMyEmployerOrganizationCommand
        {
            Name = "  General Hospital  ",
            Type = "   ",
            WebsiteUrl = "  https://general.example.com  ",
            City = "  Toronto  ",
            AddressLine1 = "  100 Care Street  ",
            AddressLine2 = "   ",
            PostalCode = "  A1B 2C3  ",
            Description = "  Regional healthcare organization  "
        }, CancellationToken.None);

        Assert.Equal("General Hospital", organization.Name);
        Assert.Null(organization.Type);
        Assert.Equal("https://general.example.com", organization.WebsiteUrl);
        Assert.Equal("Toronto", organization.City);
        Assert.Equal("100 Care Street", organization.AddressLine1);
        Assert.Null(organization.AddressLine2);
        Assert.Equal("A1B 2C3", organization.PostalCode);
        Assert.Equal("Regional healthcare organization", organization.Description);
        Assert.Equal("General Hospital", result.Name);
        Assert.Null(result.Type);
    }

    [Fact]
    public async Task UpsertOrganization_WithActiveCountryId_SavesCountryReference()
    {
        var userId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var country = new Country { Id = countryId, Name = "Canada", Code = "CA", IsActive = true };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile], [], [country]);

        var result = await handler.Handle(new UpsertMyEmployerOrganizationCommand
        {
            Name = "General Hospital",
            CountryId = countryId
        }, CancellationToken.None);

        Assert.Equal(countryId, result.CountryId);
        Assert.Equal("Canada", result.CountryName);
        _contextMock.Verify(c => c.EmployerOrganizations.Add(It.Is<EmployerOrganization>(o =>
            o.CountryId == countryId)), Times.Once);
    }

    [Fact]
    public async Task UpsertOrganization_WithMissingCountryId_FollowsPhase5CountryValidationPattern()
    {
        var userId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile], [], []);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpsertMyEmployerOrganizationCommand
            {
                Name = "General Hospital",
                CountryId = Guid.NewGuid()
            }, CancellationToken.None));
    }

    [Fact]
    public async Task UpsertOrganization_WithInactiveCountryId_FollowsPhase5CountryValidationPattern()
    {
        var userId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var profile = new EmployerProfile { Id = Guid.NewGuid(), UserId = userId };
        var country = new Country { Id = countryId, Name = "Inactive", Code = "IN", IsActive = false };
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Employer"), [profile], [], [country]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpsertMyEmployerOrganizationCommand
            {
                Name = "General Hospital",
                CountryId = countryId
            }, CancellationToken.None));
    }

    [Fact]
    public async Task UpsertOrganization_WithNonEmployerRole_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateHandler(userId, CreateUserWithRole(userId, "Nurse"), [], [], []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new UpsertMyEmployerOrganizationCommand { Name = "General Hospital" }, CancellationToken.None));
    }

    private UpsertMyEmployerOrganizationCommandHandler CreateHandler(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<EmployerProfile> profiles,
        IReadOnlyCollection<EmployerOrganization> organizations,
        IReadOnlyCollection<Country> countries,
        Mock<Microsoft.EntityFrameworkCore.DbSet<EmployerProfile>>? profileMock = null,
        Mock<Microsoft.EntityFrameworkCore.DbSet<EmployerOrganization>>? organizationMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerProfiles).Returns(profileMock?.Object ?? profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerOrganizations).Returns(organizationMock?.Object ?? organizations.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new UpsertMyEmployerOrganizationCommandHandler(
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
