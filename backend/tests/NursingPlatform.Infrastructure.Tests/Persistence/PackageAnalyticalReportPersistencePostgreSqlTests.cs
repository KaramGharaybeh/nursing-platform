using Microsoft.EntityFrameworkCore;
using Npgsql;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public sealed class PackageAnalyticalReportPersistencePostgreSqlTests : IAsyncLifetime
{
    private const string ConnectionStringEnvironmentVariable = "NURSING_PLATFORM_TEST_POSTGRES_CONNECTION_STRING";
    private readonly string _databaseName = $"nps_stage4_report_{Guid.NewGuid():N}";
    private readonly string? _connectionString;
    private readonly string? _maintenanceConnectionString;
    private readonly bool _skip;

    public PackageAnalyticalReportPersistencePostgreSqlTests()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            _skip = true;
            return;
        }

        _connectionString = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = _databaseName
        }.ConnectionString;
        _maintenanceConnectionString = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = "postgres"
        }.ConnectionString;
    }

    [Fact]
    public async Task PackageAnalyticalReportPersistence_SavesReportWithTopicAndGuidanceChildren()
    {
        if (ShouldSkipPostgreSqlTest())
        {
            return;
        }

        var seed = await SeedReportPrerequisitesAsync();
        var generatedAt = DateTime.UtcNow;
        await using var context = CreateContext();
        var report = CreateReport(seed, generatedAt);

        context.PackageAnalyticalReports.Add(report);
        await context.SaveChangesAsync();

        var persisted = await context.PackageAnalyticalReports
            .AsNoTracking()
            .Include(item => item.TopicResults)
            .Include(item => item.GuidanceItems)
            .SingleAsync(item => item.ExamSessionId == seed.SessionId);

        Assert.Equal(seed.NurseProfileId, persisted.NurseProfileId);
        Assert.Equal(seed.ProvenanceId, persisted.ExamSessionProvenanceId);
        Assert.Equal(seed.EntitlementId, persisted.PackagePurchaseEntitlementId);
        Assert.Single(persisted.TopicResults);
        Assert.Single(persisted.GuidanceItems);
        Assert.Equal(PackageReportGuidanceSourceType.StudyMaterialVersion, Assert.Single(persisted.GuidanceItems).SourceType);
    }

    [Fact]
    public async Task PackageAnalyticalReportPersistence_BlocksDuplicateReportForSameExamSessionAndHelperRecognizesViolation()
    {
        if (ShouldSkipPostgreSqlTest())
        {
            return;
        }

        var seed = await SeedReportPrerequisitesAsync();
        await using var firstContext = CreateContext();
        firstContext.PackageAnalyticalReports.Add(CreateReport(seed, DateTime.UtcNow));
        await firstContext.SaveChangesAsync();

        await using var duplicateContext = CreateContext();
        duplicateContext.PackageAnalyticalReports.Add(CreateReport(seed, DateTime.UtcNow.AddSeconds(1)));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => duplicateContext.SaveChangesAsync());

        Assert.True(duplicateContext.IsUniquePackageAnalyticalReportSessionViolation(exception));
    }

    [Fact]
    public async Task PackageAnalyticalReportPersistence_WhenConcurrentFirstRequests_RecoversToOneExpiredPackageReportWithoutConsumingRight()
    {
        if (ShouldSkipPostgreSqlTest())
        {
            return;
        }

        var seed = await SeedReportPrerequisitesAsync(expiredEntitlement: true);

        var firstRequest = PersistOrRecoverReportAsync(seed, DateTime.UtcNow);
        var secondRequest = PersistOrRecoverReportAsync(seed, DateTime.UtcNow.AddMilliseconds(1));
        var reports = await Task.WhenAll(firstRequest, secondRequest);

        await using var context = CreateContext();
        var persisted = await context.PackageAnalyticalReports
            .AsNoTracking()
            .Include(report => report.TopicResults)
            .Include(report => report.GuidanceItems)
            .SingleAsync(report => report.ExamSessionId == seed.SessionId);
        var reportRight = await context.PackageBenefitRights.AsNoTracking()
            .SingleAsync(right => right.PackagePurchaseEntitlementId == seed.EntitlementId
                && right.RightType == PackageBenefitRightType.ReportEligibility);

        Assert.Equal(reports[0], reports[1]);
        Assert.Equal(persisted.Id, reports[0]);
        Assert.True(seed.AccessEndsAt < DateTime.UtcNow);
        Assert.Equal(1, await context.PackageAnalyticalReports.CountAsync(report => report.ExamSessionId == seed.SessionId));
        Assert.Equal(1, await context.PackageAnalyticalReportTopicResults.CountAsync(topic => topic.PackageAnalyticalReportId == persisted.Id));
        Assert.Equal(2, await context.PackageAnalyticalReportGuidanceItems.CountAsync(item => item.PackageAnalyticalReportId == persisted.Id));
        var topicResult = Assert.Single(persisted.TopicResults);
        Assert.Equal(seed.TopicId, topicResult.ReportingTopicId);
        Assert.Equal(1, topicResult.ScoredQuestionCount);
        Assert.Equal(1, topicResult.CorrectCount);
        Assert.Equal(1, topicResult.EarnedPoints);
        Assert.Equal(1, topicResult.AvailablePoints);
        Assert.Equal(100m, topicResult.Percentage);
        Assert.Contains(persisted.GuidanceItems, item => item.SourceType == PackageReportGuidanceSourceType.StudyMaterialVersion && item.SourceVersionId == seed.MaterialVersionId);
        Assert.Contains(persisted.GuidanceItems, item => item.SourceType == PackageReportGuidanceSourceType.PracticeCollectionVersion && item.SourceVersionId == seed.PracticeCollectionVersionId);
        Assert.Equal(PackageBenefitRightStatus.Dormant, reportRight.Status);
        Assert.Null(reportRight.ConsumedAt);
    }

    public async Task InitializeAsync()
    {
        if (_skip)
        {
            return;
        }

        await using var maintenanceConnection = new NpgsqlConnection(_maintenanceConnectionString);
        await maintenanceConnection.OpenAsync();
        await using (var createCommand = maintenanceConnection.CreateCommand())
        {
            createCommand.CommandText = $"CREATE DATABASE {QuoteIdentifier(_databaseName)}";
            await createCommand.ExecuteNonQueryAsync();
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_skip)
        {
            return;
        }

        await using var maintenanceConnection = new NpgsqlConnection(_maintenanceConnectionString);
        await maintenanceConnection.OpenAsync();
        await using (var terminateCommand = maintenanceConnection.CreateCommand())
        {
            terminateCommand.CommandText = "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @databaseName";
            terminateCommand.Parameters.AddWithValue("databaseName", _databaseName);
            await terminateCommand.ExecuteNonQueryAsync();
        }

        await using (var dropCommand = maintenanceConnection.CreateCommand())
        {
            dropCommand.CommandText = $"DROP DATABASE IF EXISTS {QuoteIdentifier(_databaseName)}";
            await dropCommand.ExecuteNonQueryAsync();
        }
    }

    private async Task<ReportSeed> SeedReportPrerequisitesAsync(bool expiredEntitlement = false)
    {
        if (_skip)
        {
            throw new InvalidOperationException($"Set {ConnectionStringEnvironmentVariable} to run PostgreSQL package report persistence tests.");
        }

        var now = DateTime.UtcNow.AddDays(expiredEntitlement ? -31 : -2);
        await using var context = CreateContext();
        var country = new Country { Id = Guid.NewGuid(), Name = $"Country {Guid.NewGuid():N}", Code = Guid.NewGuid().ToString("N")[..2].ToUpperInvariant() };
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
            IsFree = false
        };
        var version = new ExamVersion { Id = Guid.NewGuid(), ExamId = exam.Id, VersionNumber = 1, Status = ExamVersionStatus.Published };
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = version.Id,
            DisplayOrder = 1,
            QuestionText = "Question?",
            Explanation = "Explanation",
            QuestionType = ExamQuestionType.SingleBestAnswer,
            Points = 1,
            IsActive = true
        };
        var topic = ReportingTopic.Create(category.Id, $"Topic {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var reportingProfile = ReportingProfilePublication.CreateDraft(version.Id, $"Profile {Guid.NewGuid():N}");
        reportingProfile.AssignQuestion(question.Id, topic.Id);
        reportingProfile.Publish(now);
        var practiceCollection = PracticeCollection.Create($"Practice {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var practiceVersion = PracticeCollectionVersion.CreateDraft(practiceCollection.Id, 1);
        var practiceItem = PracticeItem.Create(topic.Id, "Prompt", "Feedback", 1);
        practiceItem.AddAnswerOption("A", true, 1);
        practiceItem.AddAnswerOption("B", false, 2);
        practiceVersion.AddPracticeItem(practiceItem);
        practiceVersion.Publish(now);
        var material = StudyMaterial.Create($"Material {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var materialVersion = StudyMaterialVersion.CreateDraft(material.Id, StudyMaterialType.FormattedText, "Content", null, null, null, [topic.Id]);
        materialVersion.Publish(now);
        var definition = PreparationPackageDefinition.Create(country.Id, category.Id, $"Package {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var packageVersion = PreparationPackageVersion.CreateDraft(definition.Id, version.Id, reportingProfile.Id, practiceVersion.Id);
        packageVersion.AddMaterialVersion(materialVersion.Id, 1);
        packageVersion.ConfirmContentIsolation();
        packageVersion.Publish(now);
        var offer = PreparationPackageOffer.CreateDraft(definition.Id, packageVersion.Id, $"Offer {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null, 9900, "USD", 30);
        offer.Activate(now);
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var user = new User { Id = Guid.NewGuid(), Email = $"nurse-{Guid.NewGuid():N}@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "Nurse", IsActive = true, EmailVerified = true };
        var userRole = new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role };
        user.UserRoles.Add(userRole);
        role.UserRoles.Add(userRole);
        var nurseProfile = new NurseProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user };
        var snapshot = PackageOrderItemSnapshot.Create(
            offer.Id,
            offer.Title,
            offer.Slug,
            offer.Summary,
            definition.Id,
            definition.Title,
            definition.Slug,
            country.Id,
            category.Id,
            packageVersion.Id,
            1,
            exam.Id,
            version.Id,
            exam.Title,
            reportingProfile.Id,
            practiceVersion.Id,
            [materialVersion.Id],
            offer.PriceAmountMinor,
            offer.Currency,
            offer.AccessDurationDays,
            now);
        var orderItem = PaymentOrderItem.CreatePackageOfferSnapshot(snapshot);
        var order = PaymentOrder.CreatePending(nurseProfile.Id, orderItem, now);
        order.MarkPaid(now.AddMinutes(1));
        var entitlement = PackagePurchaseEntitlement.CreateFromSnapshot(nurseProfile.Id, order.Id, orderItem.Id, snapshot, now.AddMinutes(1));
        var attemptRight = entitlement.Rights.Single(right => right.RightType == PackageBenefitRightType.PackageExamAttemptEligibility);
        var session = ExamSession.Create(nurseProfile.Id, exam.Id, version.Id, now.AddMinutes(2), 60, ExamSessionSource.PackageAttempt);
        session.Status = ExamSessionStatus.Submitted;
        session.SubmittedAt = now.AddMinutes(10);
        session.FinalizedAt = now.AddMinutes(10);
        session.Score = 1;
        session.MaxScore = 1;
        session.Percentage = 100m;
        session.Passed = true;
        session.CorrectCount = 1;
        session.QuestionCount = 1;
        var provenance = ExamSessionProvenance.CreateForPackageAttempt(
            session.Id,
            entitlement.Id,
            attemptRight.Id,
            snapshot.Id,
            order.Id,
            orderItem.Id,
            definition.Id,
            packageVersion.Id,
            offer.Id,
            exam.Id,
            version.Id,
            reportingProfile.Id,
            practiceVersion.Id,
            entitlement.AccessStartsAt,
            entitlement.AccessEndsAt,
            session.StartedAt);

        context.AddRange(country, category, exam, version, question, topic, reportingProfile, practiceCollection, practiceVersion, material, materialVersion, definition, packageVersion, offer, role, user, userRole, nurseProfile, order, entitlement, session, provenance);
        await context.SaveChangesAsync();

        return new ReportSeed(
            nurseProfile.Id,
            session.Id,
            provenance.Id,
            entitlement.Id,
            snapshot.Id,
            order.Id,
            orderItem.Id,
            definition.Id,
            packageVersion.Id,
            offer.Id,
            exam.Id,
            version.Id,
            reportingProfile.Id,
            practiceVersion.Id,
            topic.Id,
            materialVersion.Id,
            entitlement.AccessStartsAt,
            entitlement.AccessEndsAt,
            session.SubmittedAt!.Value,
            session.FinalizedAt!.Value);
    }

    private static PackageAnalyticalReport CreateReport(ReportSeed seed, DateTime generatedAt)
    {
        var report = PackageAnalyticalReport.Create(
            seed.NurseProfileId,
            seed.SessionId,
            seed.ProvenanceId,
            seed.EntitlementId,
            seed.SnapshotId,
            seed.PaymentOrderId,
            seed.PaymentOrderItemId,
            seed.PackageDefinitionId,
            seed.PackageVersionId,
            seed.PackageOfferId,
            seed.ExamId,
            seed.ExamVersionId,
            seed.ReportingProfilePublicationId,
            seed.PracticeCollectionVersionId,
            generatedAt,
            ExamSessionStatus.Submitted,
            seed.SubmittedAt,
            seed.FinalizedAt,
            1,
            1,
            100m,
            true,
            1,
            1,
            seed.AccessStartsAt,
            seed.AccessEndsAt);
        report.AddTopicResult(seed.TopicId, "Topic", null, 1, 1, 1, 1, 100m, 1);
        report.AddGuidanceItem(seed.TopicId, PackageReportGuidanceSourceType.StudyMaterialVersion, seed.MaterialVersionId, "Material", "FormattedText", 2);
        return report;
    }

    private static PackageAnalyticalReport CreateReportWithPracticeGuidance(ReportSeed seed, DateTime generatedAt)
    {
        var report = CreateReport(seed, generatedAt);
        report.AddGuidanceItem(seed.TopicId, PackageReportGuidanceSourceType.PracticeCollectionVersion, seed.PracticeCollectionVersionId, "Practice", "PracticeCollectionVersion", 3);
        return report;
    }

    private async Task<Guid> PersistOrRecoverReportAsync(ReportSeed seed, DateTime generatedAt)
    {
        await using var context = CreateContext();
        var report = CreateReportWithPracticeGuidance(seed, generatedAt);
        context.PackageAnalyticalReports.Add(report);

        try
        {
            await context.SaveChangesAsync();
            return report.Id;
        }
        catch (DbUpdateException exception) when (context.IsUniquePackageAnalyticalReportSessionViolation(exception))
        {
            return await context.PackageAnalyticalReports
                .AsNoTracking()
                .Where(existing => existing.ExamSessionId == seed.SessionId)
                .Select(existing => existing.Id)
                .SingleAsync();
        }
    }

    private ApplicationDbContext CreateContext()
    {
        if (_skip)
        {
            throw new InvalidOperationException($"Set {ConnectionStringEnvironmentVariable} to run PostgreSQL package report persistence tests.");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    private bool ShouldSkipPostgreSqlTest()
    {
        return _skip;
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private sealed record ReportSeed(
        Guid NurseProfileId,
        Guid SessionId,
        Guid ProvenanceId,
        Guid EntitlementId,
        Guid SnapshotId,
        Guid PaymentOrderId,
        Guid PaymentOrderItemId,
        Guid PackageDefinitionId,
        Guid PackageVersionId,
        Guid PackageOfferId,
        Guid ExamId,
        Guid ExamVersionId,
        Guid ReportingProfilePublicationId,
        Guid PracticeCollectionVersionId,
        Guid TopicId,
        Guid MaterialVersionId,
        DateTime AccessStartsAt,
        DateTime AccessEndsAt,
        DateTime SubmittedAt,
        DateTime FinalizedAt);
}
