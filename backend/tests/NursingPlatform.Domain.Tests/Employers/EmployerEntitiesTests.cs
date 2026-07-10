using NursingPlatform.Domain.Employers;

namespace NursingPlatform.Domain.Tests.Employers;

public class EmployerEntitiesTests
{
    [Fact]
    public void EmployerProfile_CanBeCreatedForUserWithoutDuplicatingIdentityFields()
    {
        var profile = new EmployerProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JobTitle = "Recruitment Manager",
            Department = "Talent Acquisition"
        };

        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.NotEqual(Guid.Empty, profile.UserId);
        Assert.Equal("Recruitment Manager", profile.JobTitle);
        Assert.Equal("Talent Acquisition", profile.Department);
        Assert.Null(profile.Organization);
        Assert.Null(profile.GetType().GetProperty("FirstName"));
        Assert.Null(profile.GetType().GetProperty("LastName"));
        Assert.Null(profile.GetType().GetProperty("Email"));
        Assert.Null(profile.GetType().GetProperty("PhoneNumber"));
    }

    [Fact]
    public void EmployerOrganization_StoresBusinessMetadataOnly()
    {
        var organization = new EmployerOrganization
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = Guid.NewGuid(),
            Name = "General Hospital",
            Type = "Hospital",
            WebsiteUrl = "https://general.example.com",
            City = "Toronto",
            AddressLine1 = "100 Care Street",
            AddressLine2 = "Suite 200",
            PostalCode = "A1B 2C3",
            Description = "Regional healthcare organization"
        };

        Assert.NotEqual(Guid.Empty, organization.Id);
        Assert.NotEqual(Guid.Empty, organization.EmployerProfileId);
        Assert.Equal("General Hospital", organization.Name);
        Assert.Equal("Hospital", organization.Type);
        Assert.Equal("https://general.example.com", organization.WebsiteUrl);
        Assert.Equal("Toronto", organization.City);
        Assert.Equal("100 Care Street", organization.AddressLine1);
        Assert.Equal("Suite 200", organization.AddressLine2);
        Assert.Equal("A1B 2C3", organization.PostalCode);
        Assert.Equal("Regional healthcare organization", organization.Description);
        Assert.Null(organization.GetType().GetProperty("PhoneNumber"));
        Assert.Null(organization.GetType().GetProperty("LogoStorageKey"));
        Assert.Null(organization.GetType().GetProperty("DocumentStorageKey"));
        Assert.Null(organization.GetType().GetProperty("InvitationToken"));
        Assert.Null(organization.GetType().GetProperty("MembershipRole"));
    }

    [Fact]
    public void EmployerOrganization_CountryReference_IsOptional()
    {
        var organization = new EmployerOrganization
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = Guid.NewGuid(),
            Name = "General Hospital"
        };

        Assert.Null(organization.CountryId);
        Assert.Null(organization.Country);
    }
}
