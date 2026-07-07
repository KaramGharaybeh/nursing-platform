using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Tests.ReferenceData;

public class ReferenceDataEntitiesTests
{
    [Fact]
    public void Country_Should_Set_Properties()
    {
        var id = Guid.NewGuid();
        var country = new Country
        {
            Id = id,
            Name = "United States",
            Code = "US",
            IsActive = true
        };

        Assert.Equal(id, country.Id);
        Assert.Equal("United States", country.Name);
        Assert.Equal("US", country.Code);
        Assert.True(country.IsActive);
    }

    [Fact]
    public void Language_Should_Set_Properties()
    {
        var language = new Language
        {
            Id = Guid.NewGuid(),
            Name = "English",
            Code = "EN",
            IsActive = true
        };

        Assert.Equal("English", language.Name);
        Assert.Equal("EN", language.Code);
    }

    [Fact]
    public void Role_Should_Set_Properties()
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Description = "System administrator"
        };

        Assert.Equal("Admin", role.Name);
        Assert.Equal("System administrator", role.Description);
    }

    [Fact]
    public void Permission_Should_Set_Properties()
    {
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = "ManageUsers",
            Description = "Can manage users"
        };

        Assert.Equal("ManageUsers", permission.Name);
    }

    [Fact]
    public void RolePermission_Should_Set_Properties()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        Assert.Equal(roleId, rolePermission.RoleId);
        Assert.Equal(permissionId, rolePermission.PermissionId);
    }
}
