using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class NurseProfileConfigurationTests
{
    [Fact]
    public void NurseExperienceConfiguration_TextLengthsMatchApplicationValidationContract()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(NurseExperience))!;

        Assert.Equal(200, entity.FindProperty(nameof(NurseExperience.FacilityName))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(NurseExperience.JobTitle))!.GetMaxLength());
        Assert.Equal(2000, entity.FindProperty(nameof(NurseExperience.Description))!.GetMaxLength());
    }

    [Fact]
    public void NurseEducationConfiguration_TextLengthsMatchApplicationValidationContract()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(NurseEducation))!;

        Assert.Equal(200, entity.FindProperty(nameof(NurseEducation.InstitutionName))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(NurseEducation.Degree))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(NurseEducation.FieldOfStudy))!.GetMaxLength());
        Assert.Equal(2000, entity.FindProperty(nameof(NurseEducation.Description))!.GetMaxLength());
    }

    [Fact]
    public void NurseCertificateConfiguration_TextLengthsMatchApplicationValidationContract()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(NurseCertificate))!;

        Assert.Equal(200, entity.FindProperty(nameof(NurseCertificate.Name))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(NurseCertificate.IssuingOrganization))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(NurseCertificate.CredentialId))!.GetMaxLength());
        Assert.Equal(500, entity.FindProperty(nameof(NurseCertificate.CredentialUrl))!.GetMaxLength());
    }

    [Fact]
    public void NurseProfileConfiguration_TextLengthsMatchApplicationValidationContract()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(NurseProfile))!;

        Assert.Equal(160, entity.FindProperty(nameof(NurseProfile.Headline))!.GetMaxLength());
        Assert.Equal(2000, entity.FindProperty(nameof(NurseProfile.ProfessionalSummary))!.GetMaxLength());
        Assert.Equal(100, entity.FindProperty(nameof(NurseProfile.LicenseNumber))!.GetMaxLength());
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
