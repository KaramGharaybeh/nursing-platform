using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence.Seed;

public static class ReferenceDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedCountriesAsync(context);
        await SeedLanguagesAsync(context);
        await SeedRolesAsync(context);
        await SeedPermissionsAsync(context);
        await SeedRolePermissionsAsync(context);
    }

    private static async Task SeedCountriesAsync(ApplicationDbContext context)
    {
        if (await context.Set<Country>().AnyAsync())
            return;

        context.Set<Country>().AddRange(
        [
            new() { Id = new Guid("A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C5D"), Name = "United States", Code = "US", IsActive = true },
            new() { Id = new Guid("B2C3D4E5-F6A7-4B8C-9D0E-1F2A3B4C5D6E"), Name = "United Kingdom", Code = "GB", IsActive = true },
            new() { Id = new Guid("C3D4E5F6-A7B8-4C9D-0E1F-2A3B4C5D6E7F"), Name = "Canada", Code = "CA", IsActive = true },
            new() { Id = new Guid("D4E5F6A7-B8C9-4D0E-1F2A-3B4C5D6E7F80"), Name = "Australia", Code = "AU", IsActive = true },
            new() { Id = new Guid("E5F6A7B8-C9D0-4E1F-2A3B-4C5D6E7F8091"), Name = "United Arab Emirates", Code = "AE", IsActive = true },
            new() { Id = new Guid("F6A7B8C9-D0E1-4F2A-3B4C-5D6E7F8091A2"), Name = "Saudi Arabia", Code = "SA", IsActive = true },
            new() { Id = new Guid("A7B8C9D0-E1F2-4A3B-4C5D-6E7F8091A2B3"), Name = "Qatar", Code = "QA", IsActive = true },
            new() { Id = new Guid("B8C9D0E1-F2A3-4B4C-5D6E-7F8091A2B3C4"), Name = "Oman", Code = "OM", IsActive = true },
            new() { Id = new Guid("C9D0E1F2-A3B4-4C5D-6E7F-8091A2B3C4D5"), Name = "Kuwait", Code = "KW", IsActive = true },
            new() { Id = new Guid("D0E1F2A3-B4C5-4D6E-7F80-91A2B3C4D5E6"), Name = "Bahrain", Code = "BH", IsActive = true },
        ]);

        await context.SaveChangesAsync();
    }

    private static async Task SeedLanguagesAsync(ApplicationDbContext context)
    {
        if (await context.Set<Language>().AnyAsync())
            return;

        context.Set<Language>().AddRange(
        [
            new() { Id = new Guid("E1F2A3B4-C5D6-4E7F-8091-A2B3C4D5E6F7"), Name = "English", Code = "EN", IsActive = true },
            new() { Id = new Guid("F2A3B4C5-D6E7-4F80-91A2-B3C4D5E6F708"), Name = "Arabic", Code = "AR", IsActive = true },
            new() { Id = new Guid("A3B4C5D6-E7F8-4091-A2B3-C4D5E6F70819"), Name = "French", Code = "FR", IsActive = true },
            new() { Id = new Guid("B4C5D6E7-F809-41A2-B3C4-D5E6F708192A"), Name = "Spanish", Code = "ES", IsActive = true },
            new() { Id = new Guid("C5D6E7F8-091A-42B3-C4D5-E6F708192A3B"), Name = "Hindi", Code = "HI", IsActive = true },
            new() { Id = new Guid("D6E7F809-1A2B-43C4-D5E6-F708192A3B4C"), Name = "Urdu", Code = "UR", IsActive = true },
            new() { Id = new Guid("E7F8091A-2B3C-44D5-E6F7-08192A3B4C5D"), Name = "Tagalog", Code = "TL", IsActive = true },
        ]);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        if (await context.Set<Role>().AnyAsync())
            return;

        context.Set<Role>().AddRange(
        [
            new() { Id = new Guid("F8091A2B-3C4D-45E6-F708-192A3B4C5D6E"), Name = "SuperAdmin", Description = "Full system access" },
            new() { Id = new Guid("091A2B3C-4D5E-46F7-0819-2A3B4C5D6E7F"), Name = "Admin", Description = "Administrative access" },
            new() { Id = new Guid("1A2B3C4D-5E6F-4708-192A-3B4C5D6E7F80"), Name = "Nurse", Description = "Nurse user" },
            new() { Id = new Guid("2B3C4D5E-6F70-4819-2A3B-4C5D6E7F8091"), Name = "Employer", Description = "Employer user" },
        ]);

        await context.SaveChangesAsync();
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        if (await context.Set<Permission>().AnyAsync())
            return;

        context.Set<Permission>().AddRange(
        [
            new() { Id = new Guid("3C4D5E6F-7081-492A-3B4C-5D6E7F8091A2"), Name = "Users.View", Description = "View users" },
            new() { Id = new Guid("4D5E6F70-8192-4A3B-4C5D-6E7F8091A2B3"), Name = "Users.Create", Description = "Create users" },
            new() { Id = new Guid("5E6F7081-92A3-4B4C-5D6E-7F8091A2B3C4"), Name = "Users.Edit", Description = "Edit users" },
            new() { Id = new Guid("6F708192-A3B4-4C5D-6E7F-8091A2B3C4D5"), Name = "Users.Delete", Description = "Delete users" },
            new() { Id = new Guid("708192A3-B4C5-4D6E-7F80-91A2B3C4D5E6"), Name = "Roles.View", Description = "View roles" },
            new() { Id = new Guid("8192A3B4-C5D6-4E7F-8091-A2B3C4D5E6F7"), Name = "Roles.Manage", Description = "Manage roles" },
            new() { Id = new Guid("92A3B4C5-D6E7-4F80-91A2-B3C4D5E6F708"), Name = "Permissions.View", Description = "View permissions" },
            new() { Id = new Guid("A3B4C5D6-E7F8-4091-A2B3-C4D5E6F70819"), Name = "Permissions.Manage", Description = "Manage permissions" },
            new() { Id = new Guid("B4C5D6E7-F809-41A2-B3C4-D5E6F708192A"), Name = "Countries.View", Description = "View countries" },
            new() { Id = new Guid("C5D6E7F8-091A-42B3-C4D5-E6F708192A3B"), Name = "Countries.Manage", Description = "Manage countries" },
            new() { Id = new Guid("D6E7F809-1A2B-43C4-D5E6-F708192A3B4C"), Name = "Languages.View", Description = "View languages" },
            new() { Id = new Guid("E7F8091A-2B3C-44D5-E6F7-08192A3B4C5D"), Name = "Languages.Manage", Description = "Manage languages" },
            new() { Id = new Guid("01ABCDEF-1234-4ABC-8DEF-0123456789AB"), Name = "Exams.View", Description = "View exams" },
            new() { Id = new Guid("02ABCDEF-1234-4ABC-8DEF-0123456789AB"), Name = "Exams.Create", Description = "Create exams" },
            new() { Id = new Guid("03ABCDEF-1234-4ABC-8DEF-0123456789AB"), Name = "Exams.Edit", Description = "Edit exams" },
            new() { Id = new Guid("04ABCDEF-1234-4ABC-8DEF-0123456789AB"), Name = "Exams.Delete", Description = "Delete exams" },
            new() { Id = new Guid("05ABCDEF-1234-4ABC-8DEF-0123456789AB"), Name = "Questions.View", Description = "View questions" },
            new() { Id = new Guid("06ABCDEF-1234-4ABC-8DEF-0123456789AB"), Name = "Questions.Manage", Description = "Manage questions" },
            new() { Id = new Guid("07ABCDEF-1234-4ABC-8DEF-0123456789AB"), Name = "Nurses.View", Description = "View nurses" },
            new() { Id = new Guid("08ABCDEF-1234-4ABC-8DEF-0123456789AB"), Name = "Employers.View", Description = "View employers" },
        ]);

        await context.SaveChangesAsync();
    }

    internal static async Task SeedRolePermissionsAsync(ApplicationDbContext context)
    {
        var roleNames = new[] { "SuperAdmin", "Admin" };

        var roles = await context.Set<Role>()
            .Where(r => roleNames.Contains(r.Name))
            .ToDictionaryAsync(r => r.Name);

        var permissions = await context.Set<Permission>()
            .Where(p => Permissions.All.Contains(p.Name))
            .ToListAsync();

        var existingPairs = await context.Set<RolePermission>()
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync();

        var existingSet = existingPairs
            .Select(p => (p.RoleId, p.PermissionId))
            .ToHashSet();

        var newPairs = new List<RolePermission>();

        foreach (var roleName in roleNames)
        {
            if (!roles.TryGetValue(roleName, out var role))
                continue;

            foreach (var permission in permissions)
            {
                if (existingSet.Contains((role.Id, permission.Id)))
                    continue;

                newPairs.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        if (newPairs.Count > 0)
        {
            context.Set<RolePermission>().AddRange(newPairs);
            await context.SaveChangesAsync();
        }
    }
}
