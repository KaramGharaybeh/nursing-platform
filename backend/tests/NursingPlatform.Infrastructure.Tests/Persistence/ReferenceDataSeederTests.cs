using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Persistence;
using NursingPlatform.Infrastructure.Persistence.Seed;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class ReferenceDataSeederTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task PermissionSeedIds_AreUnique()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var permissions = await context.Set<Permission>().ToListAsync();
        Assert.Equal(permissions.Count, permissions.Select(p => p.Id).Distinct().Count());
    }

    [Fact]
    public async Task SeedAsync_SuperAdmin_GetsAllPermissions()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var pairs = await context.Set<RolePermission>()
            .Where(rp => rp.Role.Name == "SuperAdmin")
            .CountAsync();

        Assert.Equal(28, pairs);
    }

    [Fact]
    public async Task SeedAsync_Admin_GetsAllPermissions()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var pairs = await context.Set<RolePermission>()
            .Where(rp => rp.Role.Name == "Admin")
            .CountAsync();

        Assert.Equal(28, pairs);
    }

    [Fact]
    public async Task SeedAsync_Nurse_GetsZeroPermissions()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var pairs = await context.Set<RolePermission>()
            .Where(rp => rp.Role.Name == "Nurse")
            .CountAsync();

        Assert.Equal(0, pairs);
    }

    [Fact]
    public async Task SeedAsync_Employer_GetsZeroPermissions()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var pairs = await context.Set<RolePermission>()
            .Where(rp => rp.Role.Name == "Employer")
            .CountAsync();

        Assert.Equal(0, pairs);
    }

    [Fact]
    public async Task SeedAsync_WhenExistingRolesAreMissingExpert_AddsExpertWithoutDuplicatingRoles()
    {
        var context = CreateDbContext();
        context.Set<Role>().AddRange(
        [
            new() { Id = new Guid("F8091A2B-3C4D-45E6-F708-192A3B4C5D6E"), Name = "SuperAdmin", Description = "Full system access" },
            new() { Id = new Guid("091A2B3C-4D5E-46F7-0819-2A3B4C5D6E7F"), Name = "Admin", Description = "Administrative access" },
            new() { Id = new Guid("1A2B3C4D-5E6F-4708-192A-3B4C5D6E7F80"), Name = "Nurse", Description = "Nurse user" },
            new() { Id = new Guid("2B3C4D5E-6F70-4819-2A3B-4C5D6E7F8091"), Name = "Employer", Description = "Employer user" }
        ]);
        await context.SaveChangesAsync();

        await ReferenceDataSeeder.SeedAsync(context);

        var roles = await context.Set<Role>().ToListAsync();
        Assert.Equal(5, roles.Count);
        Assert.Single(roles, role =>
            role.Id == new Guid("3C4D5E6F-7081-492A-3B4C-5D6E7F8091A2") &&
            role.Name == "Expert" &&
            role.Description == "Expert user");
        Assert.Equal(roles.Count, roles.Select(role => role.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(roles.Count, roles.Select(role => role.Id).Distinct().Count());
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_DoesNotDuplicateRolePermissions()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);
        await ReferenceDataSeeder.SeedAsync(context);

        var pairCount = await context.Set<RolePermission>().CountAsync();
        Assert.Equal(56, pairCount);
    }

    [Fact]
    public async Task SeedAsync_WhenCountriesAlreadyExist_StillSeedsMissingRolePermissions()
    {
        var context = CreateDbContext();

        context.Set<Country>().Add(new Country
        {
            Id = Guid.NewGuid(),
            Name = "TestCountry",
            Code = "TC",
            IsActive = true
        });
        await context.SaveChangesAsync();

        await ReferenceDataSeeder.SeedAsync(context);

        var pairCount = await context.Set<RolePermission>().CountAsync();
        Assert.Equal(56, pairCount);
    }

    [Fact]
    public async Task SeedAsync_WhenOnePairExists_AddsOnlyMissingPairs()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var superAdmin = await context.Set<Role>().FirstAsync(r => r.Name == "SuperAdmin");
        var usersView = await context.Set<Permission>().FirstAsync(p => p.Name == "Users.View");

        var existing = await context.Set<RolePermission>().ToListAsync();
        context.Set<RolePermission>().RemoveRange(existing);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission
        {
            RoleId = superAdmin.Id,
            PermissionId = usersView.Id
        });
        await context.SaveChangesAsync();

        await ReferenceDataSeeder.SeedAsync(context);

        var allPairs = await context.Set<RolePermission>().ToListAsync();
        Assert.Equal(56, allPairs.Count);
        Assert.Single(allPairs, rp =>
            rp.RoleId == superAdmin.Id && rp.PermissionId == usersView.Id);
    }

    [Fact]
    public async Task SeedAsync_ExtraPermissionNotInAll_IsNotGrantedToAdminRoles()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        context.Set<Permission>().Add(new Permission
        {
            Id = Guid.NewGuid(),
            Name = "Experimental.Manage",
            Description = "Not in Permissions.All"
        });
        await context.SaveChangesAsync();

        var existingPairs = await context.Set<RolePermission>().ToListAsync();
        context.Set<RolePermission>().RemoveRange(existingPairs);
        await context.SaveChangesAsync();

        await ReferenceDataSeeder.SeedAsync(context);

        var allPairs = await context.Set<RolePermission>().ToListAsync();
        Assert.Equal(56, allPairs.Count);

        var experimentalPermission = await context.Set<Permission>()
            .FirstAsync(p => p.Name == "Experimental.Manage");
        var extraGranted = allPairs.Count(rp =>
            rp.PermissionId == experimentalPermission.Id);
        Assert.Equal(0, extraGranted);
    }

    [Fact]
    public async Task PreparationPackagePermissionSeedData_IncludesDedicatedPermissions()
    {
        var context = CreateDbContext();
        var expectedPermissions = new[]
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

        await ReferenceDataSeeder.SeedAsync(context);

        var permissionNames = await context.Set<Permission>()
            .Select(permission => permission.Name)
            .ToListAsync();

        Assert.All(expectedPermissions, permission => Assert.Contains(permission, permissionNames));
    }

    [Fact]
    public async Task PreparationPackagePermissionSeedData_HasNoDuplicateNamesOrIds()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var permissions = await context.Set<Permission>().ToListAsync();

        Assert.Equal(permissions.Count, permissions.Select(permission => permission.Id).Distinct().Count());
        Assert.Equal(permissions.Count, permissions.Select(permission => permission.Name).Distinct(StringComparer.Ordinal).Count());
    }
}
