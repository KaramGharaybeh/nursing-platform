using NursingPlatform.Application.Authorization;

namespace NursingPlatform.Application.Tests.PreparationPackages;

public class PreparationPackagePermissionTests
{
    [Fact]
    public void Stage1Permissions_ShouldExposeDedicatedPreparationPackagePermissionConstants()
    {
        var expected = new[]
        {
            "PreparationPackages.View",
            "PreparationPackages.Manage",
            "PreparationPackages.Publish",
            "PreparationPackageOffers.Manage",
            "StudyMaterials.Manage",
            "PracticeCollections.Manage",
            "ReportingTopics.Manage",
            "ReportingProfiles.Manage"
        };

        Assert.Equal("PreparationPackages.View", Permissions.PreparationPackages.View);
        Assert.Equal("PreparationPackages.Manage", Permissions.PreparationPackages.Manage);
        Assert.Equal("PreparationPackages.Publish", Permissions.PreparationPackages.Publish);
        Assert.Equal("PreparationPackageOffers.Manage", Permissions.PreparationPackageOffers.Manage);
        Assert.Equal("StudyMaterials.Manage", Permissions.StudyMaterials.Manage);
        Assert.Equal("PracticeCollections.Manage", Permissions.PracticeCollections.Manage);
        Assert.Equal("ReportingTopics.Manage", Permissions.ReportingTopics.Manage);
        Assert.Equal("ReportingProfiles.Manage", Permissions.ReportingProfiles.Manage);

        Assert.All(expected, permission => Assert.Contains(permission, Permissions.All));
    }
}
