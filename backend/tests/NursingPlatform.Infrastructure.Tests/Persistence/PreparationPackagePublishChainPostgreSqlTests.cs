using Microsoft.EntityFrameworkCore;
using Npgsql;
using NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;
using NursingPlatform.Application.PreparationPackages.Admin.PackageVersions;
using NursingPlatform.Application.PreparationPackages.Admin.PracticeCollections;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingProfiles;
using NursingPlatform.Application.PreparationPackages.Admin.StudyMaterials;
using NursingPlatform.Application.PreparationPackages.Catalog;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public sealed class PreparationPackagePublishChainPostgreSqlTests : IAsyncLifetime
{
    private const string ConnectionStringEnvironmentVariable = "NURSING_PLATFORM_TEST_POSTGRES_CONNECTION_STRING";
    private readonly string _databaseName = $"nps_publish_chain_{Guid.NewGuid():N}";
    private readonly string _connectionString;
    private readonly string _maintenanceConnectionString;

    public PreparationPackagePublishChainPostgreSqlTests()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException($"Set {ConnectionStringEnvironmentVariable} to run PostgreSQL publish-chain tests.");
        }

        _connectionString = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = _databaseName }.ConnectionString;
        _maintenanceConnectionString = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = "postgres" }.ConnectionString;
    }

    [Fact]
    public async Task PublishStudyMaterialVersion_WithMappedTopic_PublishesAgainstFreshContext()
    {
        var (materialId, versionId, _) = await SeedMaterialDraftAsync();

        await using var context = CreateContext();
        var handler = new PublishAdminStudyMaterialVersionCommandHandler(context);
        var result = await handler.Handle(new PublishAdminStudyMaterialVersionCommand
        {
            StudyMaterialId = materialId,
            VersionId = versionId
        }, CancellationToken.None);

        Assert.Equal(PublicationStatus.Published.ToString(), result.Status);

        await using var verify = CreateContext();
        var stored = await verify.StudyMaterialVersions.AsNoTracking().SingleAsync(v => v.Id == versionId);
        Assert.Equal(PublicationStatus.Published, stored.Status);
    }

    [Fact]
    public async Task PublishPracticeCollectionVersion_WithItems_PublishesAgainstFreshContext()
    {
        var (collectionId, versionId) = await SeedPracticeDraftAsync();

        await using var context = CreateContext();
        var handler = new PublishAdminPracticeCollectionVersionCommandHandler(context);
        var result = await handler.Handle(new PublishAdminPracticeCollectionVersionCommand
        {
            PracticeCollectionId = collectionId,
            VersionId = versionId
        }, CancellationToken.None);

        Assert.Equal(PublicationStatus.Published.ToString(), result.Status);

        await using var verify = CreateContext();
        var stored = await verify.PracticeCollectionVersions.AsNoTracking().SingleAsync(v => v.Id == versionId);
        Assert.Equal(PublicationStatus.Published, stored.Status);
    }

    [Fact]
    public async Task PublishPackageVersion_WithConfirmedSellableGraph_PublishesAgainstFreshContext()
    {
        var graph = await SeedPackageVersionDraftAsync();

        await using var context = CreateContext();
        var handler = new PublishAdminPreparationPackageVersionCommandHandler(context);
        var result = await handler.Handle(new PublishAdminPreparationPackageVersionCommand
        {
            PreparationPackageDefinitionId = graph.DefinitionId,
            VersionId = graph.PackageVersionId
        }, CancellationToken.None);

        Assert.Equal(PreparationPackageVersionStatus.Published.ToString(), result.Status);

        await using var verify = CreateContext();
        var stored = await verify.PreparationPackageVersions.AsNoTracking().SingleAsync(v => v.Id == graph.PackageVersionId);
        Assert.Equal(PreparationPackageVersionStatus.Published, stored.Status);
        Assert.True(stored.ContentIsolationConfirmed);
    }

    [Fact]
    public async Task ListCatalogOffers_WithActiveOfferOnPublishedVersion_IncludesOfferAgainstFreshContext()
    {
        var slug = $"offer-{Guid.NewGuid():N}";
        await SeedActiveCatalogOfferAsync(slug);

        await using var catalog = CreateContext();
        var handler = new ListPreparationPackageOffersQueryHandler(catalog);
        var result = await handler.Handle(new ListPreparationPackageOffersQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(slug, item.Slug);
        Assert.Equal(1, item.MaterialCount);
    }

    [Fact]
    public async Task GetCatalogOffer_WithActiveOfferOnPublishedVersion_ReturnsOfferAgainstFreshContext()
    {
        var slug = $"offer-{Guid.NewGuid():N}";
        await SeedActiveCatalogOfferAsync(slug);

        await using var catalog = CreateContext();
        var handler = new GetPreparationPackageOfferQueryHandler(catalog);
        var detail = await handler.Handle(new GetPreparationPackageOfferQuery { Slug = slug }, CancellationToken.None);

        Assert.Equal(slug, detail.Slug);
        Assert.Equal(1, detail.MaterialCount);
    }

    private async Task SeedActiveCatalogOfferAsync(string slug)
    {
        var graph = await SeedPackageVersionDraftAsync();

        await using (var context = CreateContext())
        {
            var publishHandler = new PublishAdminPreparationPackageVersionCommandHandler(context);
            var published = await publishHandler.Handle(new PublishAdminPreparationPackageVersionCommand
            {
                PreparationPackageDefinitionId = graph.DefinitionId,
                VersionId = graph.PackageVersionId
            }, CancellationToken.None);
            Assert.Equal(PreparationPackageVersionStatus.Published.ToString(), published.Status);
        }

        await using (var context = CreateContext())
        {
            var createHandler = new CreateAdminPreparationPackageOfferCommandHandler(context);
            var created = await createHandler.Handle(new CreateAdminPreparationPackageOfferCommand
            {
                Request = new CreateAdminPreparationPackageOfferRequest
                {
                    PreparationPackageDefinitionId = graph.DefinitionId,
                    PreparationPackageVersionId = graph.PackageVersionId,
                    Title = $"Catalog offer {Guid.NewGuid():N}",
                    Slug = slug,
                    Summary = "Catalog offer summary",
                    PriceAmountMinor = 1000,
                    Currency = "USD",
                    AccessDurationDays = 30
                }
            }, CancellationToken.None);

            var activateHandler = new ActivateAdminPreparationPackageOfferCommandHandler(context);
            var activated = await activateHandler.Handle(new ActivateAdminPreparationPackageOfferCommand { Id = created.Id }, CancellationToken.None);
            Assert.Equal(PreparationPackageOfferStatus.Active.ToString(), activated.Status);
        }
    }

    private async Task<(Guid MaterialId, Guid VersionId, Guid TopicId)> SeedMaterialDraftAsync()
    {
        await using var context = CreateContext();
        var country = new Country { Id = Guid.NewGuid(), Name = $"Country {Guid.NewGuid():N}", Code = "QZ" };
        var category = new ExamCategory { Id = Guid.NewGuid(), CountryId = country.Id, Name = $"Category {Guid.NewGuid():N}", Slug = Guid.NewGuid().ToString("N"), DisplayOrder = 1 };
        var topic = ReportingTopic.Create(category.Id, $"Topic {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var material = StudyMaterial.Create($"Material {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var version = StudyMaterialVersion.CreateDraft(material.Id, StudyMaterialType.FormattedText, "Study content", null, null, null, [topic.Id]);
        context.Countries.Add(country);
        context.ExamCategories.Add(category);
        context.ReportingTopics.Add(topic);
        context.StudyMaterials.Add(material);
        context.StudyMaterialVersions.Add(version);
        await context.SaveChangesAsync();
        return (material.Id, version.Id, topic.Id);
    }

    private async Task<(Guid CollectionId, Guid VersionId)> SeedPracticeDraftAsync()
    {
        await using var context = CreateContext();
        var country = new Country { Id = Guid.NewGuid(), Name = $"Country {Guid.NewGuid():N}", Code = "QZ" };
        var category = new ExamCategory { Id = Guid.NewGuid(), CountryId = country.Id, Name = $"Category {Guid.NewGuid():N}", Slug = Guid.NewGuid().ToString("N"), DisplayOrder = 1 };
        var topic = ReportingTopic.Create(category.Id, $"Topic {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var collection = PracticeCollection.Create($"Collection {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var version = PracticeCollectionVersion.CreateDraft(collection.Id, 1);
        var item = PracticeItem.Create(topic.Id, "Prompt", "Feedback", 1);
        item.AddAnswerOption("A", true, 1);
        item.AddAnswerOption("B", false, 2);
        version.AddPracticeItem(item);
        context.Countries.Add(country);
        context.ExamCategories.Add(category);
        context.ReportingTopics.Add(topic);
        context.PracticeCollections.Add(collection);
        context.PracticeCollectionVersions.Add(version);
        await context.SaveChangesAsync();
        return (collection.Id, version.Id);
    }

    private async Task<(Guid DefinitionId, Guid PackageVersionId)> SeedPackageVersionDraftAsync()
    {
        await using var context = CreateContext();
        var country = new Country { Id = Guid.NewGuid(), Name = $"Country {Guid.NewGuid():N}", Code = "QZ" };
        var category = new ExamCategory { Id = Guid.NewGuid(), CountryId = country.Id, Name = $"Category {Guid.NewGuid():N}", Slug = Guid.NewGuid().ToString("N"), DisplayOrder = 1 };
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            CountryId = country.Id,
            ExamCategoryId = category.Id,
            Title = $"Exam {Guid.NewGuid():N}",
            Slug = Guid.NewGuid().ToString("N"),
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = ExamStatus.Published,
            IsFree = true
        };
        var examVersion = new ExamVersion { Id = Guid.NewGuid(), ExamId = exam.Id, VersionNumber = 1, Status = ExamVersionStatus.Published };
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = examVersion.Id,
            DisplayOrder = 1,
            QuestionText = "Question?",
            QuestionType = ExamQuestionType.SingleBestAnswer,
            Points = 1,
            IsActive = true
        };
        var topic = ReportingTopic.Create(category.Id, $"Topic {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);

        var profile = ReportingProfilePublication.CreateDraft(examVersion.Id, $"Profile {Guid.NewGuid():N}");
        profile.AssignQuestion(question.Id, topic.Id);
        profile.Publish(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));

        var material = StudyMaterial.Create($"Material {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var materialVersion = StudyMaterialVersion.CreateDraft(material.Id, StudyMaterialType.FormattedText, "Study content", null, null, null, [topic.Id]);
        materialVersion.Publish(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));

        var collection = PracticeCollection.Create($"Collection {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var practiceVersion = PracticeCollectionVersion.CreateDraft(collection.Id, 1);
        var practiceItem = PracticeItem.Create(topic.Id, "Prompt", "Feedback", 1);
        practiceItem.AddAnswerOption("A", true, 1);
        practiceItem.AddAnswerOption("B", false, 2);
        practiceVersion.AddPracticeItem(practiceItem);
        practiceVersion.Publish(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));

        var definition = PreparationPackageDefinition.Create(country.Id, category.Id, $"Definition {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), "Description");
        var packageVersion = PreparationPackageVersion.CreateDraft(definition.Id, examVersion.Id, profile.Id, practiceVersion.Id);
        packageVersion.AddMaterialVersion(materialVersion.Id, 1);

        context.Countries.Add(country);
        context.ExamCategories.Add(category);
        context.Exams.Add(exam);
        context.ExamVersions.Add(examVersion);
        context.ExamQuestions.Add(question);
        context.ReportingTopics.Add(topic);
        context.ReportingProfilePublications.Add(profile);
        context.StudyMaterials.Add(material);
        context.StudyMaterialVersions.Add(materialVersion);
        context.PracticeCollections.Add(collection);
        context.PracticeCollectionVersions.Add(practiceVersion);
        context.PreparationPackageDefinitions.Add(definition);
        context.PreparationPackageVersions.Add(packageVersion);
        await context.SaveChangesAsync();

        return (definition.Id, packageVersion.Id);
    }

    public async Task InitializeAsync()
    {
        await using (var connection = new NpgsqlConnection(_maintenanceConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using (var terminateCommand = connection.CreateCommand())
        {
            terminateCommand.CommandText = $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{_databaseName}' AND pid <> pg_backend_pid();";
            await terminateCommand.ExecuteNonQueryAsync();
        }

        await using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"DROP DATABASE \"{_databaseName}\"";
        await dropCommand.ExecuteNonQueryAsync();
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new ApplicationDbContext(options);
    }
}
