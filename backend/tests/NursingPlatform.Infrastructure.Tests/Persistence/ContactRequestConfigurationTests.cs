using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class ContactRequestConfigurationTests
{
    [Fact]
    public void ContactRequestConfiguration_UsesExpectedTableNameAndPrimaryKey()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(ContactRequest));

        Assert.NotNull(entity);
        Assert.Equal("ContactRequests", entity.GetTableName());
        Assert.Equal(nameof(ContactRequest.Id), Assert.Single(entity.FindPrimaryKey()!.Properties).Name);
    }

    [Fact]
    public void ContactRequestConfiguration_StoresStatusAsStringWithMaxLength()
    {
        var property = CreateDbContext().Model.FindEntityType(typeof(ContactRequest))!
            .FindProperty(nameof(ContactRequest.Status))!;

        Assert.Equal(32, property.GetMaxLength());
        Assert.NotNull(property.GetTypeMapping().Converter);
    }

    [Fact]
    public void ContactRequestConfiguration_ConfiguresSnapshotMaxLengths()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(ContactRequest))!;

        Assert.Equal(160, entity.FindProperty(nameof(ContactRequest.CandidateHeadlineSnapshot))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(ContactRequest.CandidateLicenseCountryNameSnapshot))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(ContactRequest.CandidateCurrentCountryNameSnapshot))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(ContactRequest.EmployerOrganizationNameSnapshot))!.GetMaxLength());
        Assert.Equal(160, entity.FindProperty(nameof(ContactRequest.JobTitleSnapshot))!.GetMaxLength());
        Assert.Equal(160, entity.FindProperty(nameof(ContactRequest.DepartmentSnapshot))!.GetMaxLength());
    }

    [Fact]
    public void ContactRequestConfiguration_ConfiguresRestrictDeleteRelationships()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(ContactRequest))!;
        var foreignKeys = entity.GetForeignKeys().ToList();

        Assert.All(foreignKeys, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        Assert.Contains(foreignKeys, fk => fk.Properties.Any(p => p.Name == nameof(ContactRequest.EmployerProfileId)));
        Assert.Contains(foreignKeys, fk => fk.Properties.Any(p => p.Name == nameof(ContactRequest.EmployerOrganizationId)));
        Assert.Contains(foreignKeys, fk => fk.Properties.Any(p => p.Name == nameof(ContactRequest.NurseProfileId)));
    }

    [Fact]
    public void ContactRequestConfiguration_ConfiguresEmployerAndNurseListIndexes()
    {
        var indexes = CreateDbContext().Model.FindEntityType(typeof(ContactRequest))!.GetIndexes().ToList();

        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ContactRequest.EmployerProfileId), nameof(ContactRequest.CreatedAt), nameof(ContactRequest.Id)]));
        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ContactRequest.NurseProfileId), nameof(ContactRequest.CreatedAt), nameof(ContactRequest.Id)]));
    }

    [Fact]
    public void ContactRequestConfiguration_ConfiguresActiveDuplicateFilteredUniqueIndex()
    {
        var index = CreateDbContext().Model.FindEntityType(typeof(ContactRequest))!.GetIndexes()
            .Single(i => i.Properties.Select(p => p.Name).SequenceEqual(
                [nameof(ContactRequest.EmployerProfileId), nameof(ContactRequest.NurseProfileId)]));

        Assert.True(index.IsUnique);
        Assert.Equal("\"Status\" IN ('Pending', 'Approved')", index.GetFilter());
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
