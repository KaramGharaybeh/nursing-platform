using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class ExamConfigurationTests
{
    [Theory]
    [InlineData(typeof(ExamCategory), "ExamCategories")]
    [InlineData(typeof(Exam), "Exams")]
    [InlineData(typeof(ExamVersion), "ExamVersions")]
    [InlineData(typeof(ExamQuestion), "ExamQuestions")]
    [InlineData(typeof(ExamAnswerOption), "ExamAnswerOptions")]
    [InlineData(typeof(ExamAccessGrant), "ExamAccessGrants")]
    [InlineData(typeof(ExamSession), "ExamSessions")]
    [InlineData(typeof(ExamSessionQuestion), "ExamSessionQuestions")]
    [InlineData(typeof(ExamSessionAnswerOption), "ExamSessionAnswerOptions")]
    [InlineData(typeof(ExamSessionAnswer), "ExamSessionAnswers")]
    public void ExamConfiguration_UsesExpectedTableNamesAndPrimaryKeys(Type entityType, string tableName)
    {
        var entity = CreateDbContext().Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(tableName, entity.GetTableName());
        Assert.Equal("Id", Assert.Single(entity.FindPrimaryKey()!.Properties).Name);
    }

    [Theory]
    [InlineData(typeof(Exam), nameof(Exam.Status))]
    [InlineData(typeof(ExamVersion), nameof(ExamVersion.Status))]
    [InlineData(typeof(ExamQuestion), nameof(ExamQuestion.QuestionType))]
    [InlineData(typeof(ExamSession), nameof(ExamSession.Status))]
    public void ExamConfiguration_StoresStatusesAsStringsWithMaxLength(Type entityType, string propertyName)
    {
        var property = CreateDbContext().Model.FindEntityType(entityType)!.FindProperty(propertyName)!;

        Assert.Equal(32, property.GetMaxLength());
        Assert.NotNull(property.GetTypeMapping().Converter);
    }

    [Fact]
    public void ExamConfiguration_ConfiguresCatalogIndexes()
    {
        var context = CreateDbContext();
        var categoryIndexes = context.Model.FindEntityType(typeof(ExamCategory))!.GetIndexes().ToList();
        var examIndexes = context.Model.FindEntityType(typeof(Exam))!.GetIndexes().ToList();

        Assert.Contains(categoryIndexes, i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ExamCategory.CountryId), nameof(ExamCategory.Slug)]));
        Assert.Contains(categoryIndexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ExamCategory.CountryId), nameof(ExamCategory.DisplayOrder), nameof(ExamCategory.Id)]));
        Assert.Contains(examIndexes, i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual([nameof(Exam.Slug)]));
        Assert.Contains(examIndexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(Exam.Status), nameof(Exam.CountryId), nameof(Exam.ExamCategoryId), nameof(Exam.Title), nameof(Exam.Id)]));
    }

    [Fact]
    public void ExamConfiguration_ConfiguresVersionAndQuestionOrderIndexes()
    {
        var context = CreateDbContext();
        var versionIndexes = context.Model.FindEntityType(typeof(ExamVersion))!.GetIndexes().ToList();
        var questionIndexes = context.Model.FindEntityType(typeof(ExamQuestion))!.GetIndexes().ToList();
        var optionIndexes = context.Model.FindEntityType(typeof(ExamAnswerOption))!.GetIndexes().ToList();

        Assert.Contains(versionIndexes, i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ExamVersion.ExamId), nameof(ExamVersion.VersionNumber)]));
        Assert.Contains(versionIndexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ExamVersion.ExamId), nameof(ExamVersion.Status), nameof(ExamVersion.VersionNumber)]));
        Assert.Contains(questionIndexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ExamQuestion.ExamVersionId), nameof(ExamQuestion.DisplayOrder), nameof(ExamQuestion.Id)]));
        Assert.Contains(optionIndexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ExamAnswerOption.ExamQuestionId), nameof(ExamAnswerOption.DisplayOrder), nameof(ExamAnswerOption.Id)]));
    }

    [Fact]
    public void ExamConfiguration_ConfiguresSessionOwnershipAndAttemptIndexes()
    {
        var indexes = CreateDbContext().Model.FindEntityType(typeof(ExamSession))!.GetIndexes().ToList();

        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ExamSession.NurseProfileId), nameof(ExamSession.StartedAt), nameof(ExamSession.Id)]));
        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(ExamSession.NurseProfileId), nameof(ExamSession.ExamVersionId)]));
    }

    [Fact]
    public void ExamConfiguration_ConfiguresAnswerUniqueness()
    {
        var index = CreateDbContext().Model.FindEntityType(typeof(ExamSessionAnswer))!.GetIndexes()
            .Single(i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(ExamSessionAnswer.ExamSessionQuestionId)]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void ExamConfiguration_ConfiguresUniqueFilteredInProgressSessionIndex()
    {
        var index = CreateDbContext().Model.FindEntityType(typeof(ExamSession))!.GetIndexes()
            .Single(i => i.Properties.Select(p => p.Name).SequenceEqual(
                [nameof(ExamSession.NurseProfileId), nameof(ExamSession.ExamVersionId)]));

        Assert.True(index.IsUnique);
        Assert.Equal("\"Status\" = 'InProgress'", index.GetFilter());
    }

    [Fact]
    public void ExamConfiguration_ConfiguresRestrictDeleteForHistoricalIntegrity()
    {
        var entityTypes = new[]
        {
            typeof(Exam),
            typeof(ExamVersion),
            typeof(ExamQuestion),
            typeof(ExamAnswerOption),
            typeof(ExamAccessGrant),
            typeof(ExamSession),
            typeof(ExamSessionQuestion),
            typeof(ExamSessionAnswerOption),
            typeof(ExamSessionAnswer)
        };

        var foreignKeys = entityTypes
            .SelectMany(t => CreateDbContext().Model.FindEntityType(t)!.GetForeignKeys())
            .ToList();

        Assert.NotEmpty(foreignKeys);
        Assert.All(foreignKeys, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
