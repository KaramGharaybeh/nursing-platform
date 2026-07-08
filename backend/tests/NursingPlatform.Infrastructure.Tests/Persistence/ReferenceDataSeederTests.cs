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
    public async Task SeedAsync_SuperAdmin_GetsAllTwentyPermissions()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var pairs = await context.Set<RolePermission>()
            .Where(rp => rp.Role.Name == "SuperAdmin")
            .CountAsync();

        Assert.Equal(20, pairs);
    }

    [Fact]
    public async Task SeedAsync_Admin_GetsAllTwentyPermissions()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);

        var pairs = await context.Set<RolePermission>()
            .Where(rp => rp.Role.Name == "Admin")
            .CountAsync();

        Assert.Equal(20, pairs);
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
    public async Task SeedAsync_WhenCalledTwice_DoesNotDuplicateRolePermissions()
    {
        var context = CreateDbContext();

        await ReferenceDataSeeder.SeedAsync(context);
        await ReferenceDataSeeder.SeedAsync(context);

        var pairCount = await context.Set<RolePermission>().CountAsync();
        Assert.Equal(40, pairCount);
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
        Assert.Equal(40, pairCount);
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
        Assert.Equal(40, allPairs.Count);
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
        Assert.Equal(40, allPairs.Count);

        var experimentalPermission = await context.Set<Permission>()
            .FirstAsync(p => p.Name == "Experimental.Manage");
        var extraGranted = allPairs.Count(rp =>
            rp.PermissionId == experimentalPermission.Id);
        Assert.Equal(0, extraGranted);
    }
}
