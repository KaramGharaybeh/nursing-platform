using NursingPlatform.Application.Authorization;

namespace NursingPlatform.Application.Tests.Authorization;

public class PermissionRequirementTests
{
    [Fact]
    public void Constructor_ShouldSetPermission()
    {
        var requirement = new PermissionRequirement("Users.Create");
        Assert.Equal("Users.Create", requirement.Permission);
    }

    [Fact]
    public void Constructor_NullPermission_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new PermissionRequirement(null!));
    }
}
