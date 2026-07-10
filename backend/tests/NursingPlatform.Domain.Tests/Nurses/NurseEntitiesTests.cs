using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Domain.Tests.Nurses;

public class NurseEntitiesTests
{
    [Fact]
    public void NurseProfile_DefaultRecruitmentVisibility_IsFalse()
    {
        var profile = new NurseProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        Assert.False(profile.IsAvailableForRecruitment);
        Assert.Equal(0, profile.YearsOfExperience);
    }

    [Fact]
    public void NurseSkill_NormalizedName_IsStoredSeparatelyFromName()
    {
        var skill = new NurseSkill
        {
            Id = Guid.NewGuid(),
            NurseProfileId = Guid.NewGuid(),
            Name = "Critical Care",
            NormalizedName = "CRITICAL CARE"
        };

        Assert.Equal("Critical Care", skill.Name);
        Assert.Equal("CRITICAL CARE", skill.NormalizedName);
        Assert.NotEqual(skill.Name, skill.NormalizedName);
    }

    [Fact]
    public void NurseCvDocument_StorageKey_IsSeparateFromOriginalFileName()
    {
        var document = new NurseCvDocument
        {
            Id = Guid.NewGuid(),
            NurseProfileId = Guid.NewGuid(),
            OriginalFileName = "resume.pdf",
            StorageKey = "generated/internal/key.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024,
            UploadedAt = DateTime.UtcNow
        };

        Assert.Equal("resume.pdf", document.OriginalFileName);
        Assert.Equal("generated/internal/key.pdf", document.StorageKey);
        Assert.NotEqual(document.OriginalFileName, document.StorageKey);
    }
}
