using NursingPlatform.Application.Authorization;

namespace NursingPlatform.Application.Tests.Authorization;

public class PermissionsTests
{
    private static readonly HashSet<string> ExpectedSeedPermissions =
    [
        "Users.Create", "Users.View", "Users.Edit", "Users.Delete",
        "Roles.View", "Roles.Manage",
        "Permissions.View", "Permissions.Manage",
        "Countries.View", "Countries.Manage",
        "Languages.View", "Languages.Manage",
        "Exams.View", "Exams.Create", "Exams.Edit", "Exams.Delete",
        "Questions.View", "Questions.Manage",
        "Nurses.View",
        "Employers.View",
        "PreparationPackages.View", "PreparationPackages.Manage", "PreparationPackages.Publish",
        "PreparationPackageOffers.Manage",
        "StudyMaterials.Manage",
        "PracticeCollections.Manage",
        "ReportingTopics.Manage",
        "ReportingProfiles.Manage"
    ];

    [Fact]
    public void All_ContainsExactlySeedPermissions()
    {
        Assert.Equal(ExpectedSeedPermissions.Count, Permissions.All.Length);
        Assert.True(ExpectedSeedPermissions.SetEquals(Permissions.All));
    }

    [Fact]
    public void All_HasNoDuplicates()
    {
        Assert.Equal(Permissions.All.Length, new HashSet<string>(Permissions.All).Count);
    }

    [Fact]
    public void Admin_ContainsSamePermissionsAsAll()
    {
        Assert.Equal(Permissions.All, Permissions.Admin);
    }

    [Fact]
    public void AllNames_MatchExpectedFormat()
    {
        foreach (var name in Permissions.All)
        {
            Assert.Matches(@"^[A-Z][A-Za-z]+\.[A-Z][A-Za-z]+$", name);
        }
    }
}
