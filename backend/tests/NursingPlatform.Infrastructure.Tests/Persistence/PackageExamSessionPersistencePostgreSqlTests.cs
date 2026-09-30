using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.ExamSessions.Exceptions;
using NursingPlatform.Application.PreparationPackages.ExamSessions.StartPackageExamSession;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public sealed class PackageExamSessionPersistencePostgreSqlTests : IAsyncLifetime
{
    private const string ConnectionStringEnvironmentVariable = "NURSING_PLATFORM_TEST_POSTGRES_CONNECTION_STRING";
    private readonly string _databaseName = $"nps_stage3_task5_{Guid.NewGuid():N}";
    private readonly string _connectionString;
    private readonly string _maintenanceConnectionString;

    public PackageExamSessionPersistencePostgreSqlTests()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException($"Set {ConnectionStringEnvironmentVariable} to run PostgreSQL package exam session persistence tests.");
        }

        var testBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = _databaseName
        };
        _connectionString = testBuilder.ConnectionString;

        var maintenanceBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = "postgres"
        };
        _maintenanceConnectionString = maintenanceBuilder.ConnectionString;
    }

    [Fact]
    public async Task StartPackageExamSession_WhenValid_PersistsSessionProvenanceAndConsumedRightAtomically()
    {
        var seed = await SeedPackageAttemptGraphAsync();

        var result = await StartPackageExamSessionAsync(seed.UserId, seed.EntitlementId);

        await using var context = CreateContext();
        var session = await context.ExamSessions.AsNoTracking().SingleAsync(s => s.Id == result.Session.Id);
        var provenance = await context.ExamSessionProvenances.AsNoTracking().SingleAsync(p => p.ExamSessionId == session.Id);
        var attemptRight = await context.PackageBenefitRights.AsNoTracking().SingleAsync(r => r.Id == seed.AttemptRightId);

        Assert.Equal(ExamSessionSource.PackageAttempt, session.Source);
        Assert.Equal(seed.NurseProfileId, session.NurseProfileId);
        Assert.Equal(seed.ExamId, session.ExamId);
        Assert.Equal(seed.ExamVersionId, session.ExamVersionId);
        Assert.Equal(seed.EntitlementId, provenance.PackagePurchaseEntitlementId);
        Assert.Equal(seed.AttemptRightId, provenance.PackageBenefitRightId);
        Assert.Equal(seed.SnapshotId, provenance.PackageOrderItemSnapshotId);
        Assert.Equal(seed.PaymentOrderId, provenance.PaymentOrderId);
        Assert.Equal(seed.PaymentOrderItemId, provenance.PaymentOrderItemId);
        Assert.Equal(seed.PackageDefinitionId, provenance.PreparationPackageDefinitionId);
        Assert.Equal(seed.PackageVersionId, provenance.PreparationPackageVersionId);
        Assert.Equal(seed.PackageOfferId, provenance.PreparationPackageOfferId);
        Assert.Equal(seed.ExamId, provenance.IncludedExamId);
        Assert.Equal(seed.ExamVersionId, provenance.IncludedExamVersionId);
        Assert.Equal(seed.ReportingProfilePublicationId, provenance.ReportingProfilePublicationId);
        Assert.Equal(seed.PracticeCollectionVersionId, provenance.PracticeCollectionVersionId);
        Assert.Equal(PackageBenefitRightStatus.Consumed, attemptRight.Status);
        Assert.NotNull(attemptRight.ConsumedAt);
        Assert.Equal(attemptRight.ConsumedAt, provenance.StartedAt);
        Assert.Single(await context.ExamSessionQuestions.AsNoTracking().Where(q => q.ExamSessionId == session.Id).ToListAsync());
        Assert.Equal(2, await context.ExamSessionAnswerOptions.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task StartPackageExamSession_WhenPersistenceFails_RollsBackSessionProvenanceAndRightConsumption()
    {
        var seed = await SeedPackageAttemptGraphAsync();

        await Assert.ThrowsAsync<DbUpdateException>(() => StartPackageExamSessionAsync(seed.UserId, seed.EntitlementId, throwAfterPackageStartSave: true));

        await using var context = CreateContext();
        var attemptRight = await context.PackageBenefitRights.AsNoTracking().SingleAsync(r => r.Id == seed.AttemptRightId);
        Assert.Equal(PackageBenefitRightStatus.Available, attemptRight.Status);
        Assert.Null(attemptRight.ConsumedAt);
        Assert.Empty(await context.ExamSessions.AsNoTracking().ToListAsync());
        Assert.Empty(await context.ExamSessionProvenances.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task StartPackageExamSession_WhenSameEntitlementRetried_PersistsOnlyOneSessionAndOneProvenance()
    {
        var seed = await SeedPackageAttemptGraphAsync();

        var first = await StartPackageExamSessionAsync(seed.UserId, seed.EntitlementId);
        var second = await StartPackageExamSessionAsync(seed.UserId, seed.EntitlementId);

        await using var context = CreateContext();
        Assert.Equal(first.Session.Id, second.Session.Id);
        Assert.Single(await context.ExamSessions.AsNoTracking().ToListAsync());
        Assert.Single(await context.ExamSessionProvenances.AsNoTracking().ToListAsync());
        var attemptRight = await context.PackageBenefitRights.AsNoTracking().SingleAsync(r => r.Id == seed.AttemptRightId);
        Assert.Equal(PackageBenefitRightStatus.Consumed, attemptRight.Status);
        Assert.NotNull(attemptRight.ConsumedAt);
    }

    [Fact]
    public async Task StartPackageExamSession_WhenConcurrentSameEntitlementStarts_PersistsOnlyOneSessionAndOneProvenance()
    {
        var seed = await SeedPackageAttemptGraphAsync();

        var firstTask = StartPackageExamSessionAsync(seed.UserId, seed.EntitlementId);
        var secondTask = StartPackageExamSessionAsync(seed.UserId, seed.EntitlementId);
        var results = await Task.WhenAll(firstTask, secondTask);

        await using var context = CreateContext();
        Assert.Equal(results[0].Session.Id, results[1].Session.Id);
        Assert.Single(await context.ExamSessions.AsNoTracking().ToListAsync());
        Assert.Single(await context.ExamSessionProvenances.AsNoTracking().ToListAsync());
        var attemptRight = await context.PackageBenefitRights.AsNoTracking().SingleAsync(r => r.Id == seed.AttemptRightId);
        Assert.Equal(PackageBenefitRightStatus.Consumed, attemptRight.Status);
        Assert.NotNull(attemptRight.ConsumedAt);
    }

    [Fact]
    public async Task StartPackageExamSession_WhenExistingFreeSession_ReturnsSourceConflict()
    {
        await AssertExistingSourceConflictAsync(ExamSessionSource.Free);
    }

    [Fact]
    public async Task StartPackageExamSession_WhenExistingStandaloneSession_ReturnsSourceConflict()
    {
        await AssertExistingSourceConflictAsync(ExamSessionSource.StandaloneGrant);
    }

    [Fact]
    public async Task StartPackageExamSession_WhenExistingLegacySession_ReturnsSourceConflict()
    {
        await AssertExistingSourceConflictAsync(ExamSessionSource.Legacy);
    }

    [Fact]
    public async Task ExamSessionPersistence_EnforcesOneInProgressSessionPerNurseAndExamVersionAcrossSources()
    {
        var seed = await SeedPackageAttemptGraphAsync();
        await using var context = CreateContext();
        context.ExamSessions.Add(ExamSession.Create(seed.NurseProfileId, seed.ExamId, seed.ExamVersionId, DateTime.UtcNow.AddMinutes(-2), 60, ExamSessionSource.Free));
        context.ExamSessions.Add(ExamSession.Create(seed.NurseProfileId, seed.ExamId, seed.ExamVersionId, DateTime.UtcNow.AddMinutes(-1), 60, ExamSessionSource.PackageAttempt));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    public async Task InitializeAsync()
    {
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

    private async Task AssertExistingSourceConflictAsync(ExamSessionSource source)
    {
        var seed = await SeedPackageAttemptGraphAsync();
        await using (var setup = CreateContext())
        {
            setup.ExamSessions.Add(ExamSession.Create(seed.NurseProfileId, seed.ExamId, seed.ExamVersionId, DateTime.UtcNow.AddMinutes(-1), 60, source));
            await setup.SaveChangesAsync();
        }

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() => StartPackageExamSessionAsync(seed.UserId, seed.EntitlementId));

        await using var context = CreateContext();
        var attemptRight = await context.PackageBenefitRights.AsNoTracking().SingleAsync(r => r.Id == seed.AttemptRightId);
        Assert.Equal("exam-session-source-conflict", exception.Code);
        Assert.Equal(PackageBenefitRightStatus.Available, attemptRight.Status);
        Assert.Null(attemptRight.ConsumedAt);
        Assert.Empty(await context.ExamSessionProvenances.AsNoTracking().ToListAsync());
        Assert.Single(await context.ExamSessions.AsNoTracking().ToListAsync());
    }

    private async Task<NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs.PackageExamSessionStartDto> StartPackageExamSessionAsync(
        Guid userId,
        Guid entitlementId,
        bool throwAfterPackageStartSave = false)
    {
        await using var context = CreateContext(throwAfterPackageStartSave);
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, userId));
        return await handler.Handle(new StartPackageExamSessionCommand(entitlementId), default);
    }

    private async Task<PackageAttemptSeed> SeedPackageAttemptGraphAsync()
    {
        var now = DateTime.UtcNow.AddDays(-1);
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
        var nurse = AddNurseUser(context);
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
        var order = PaymentOrder.CreatePending(nurse.NurseProfileId, orderItem, now);
        order.MarkPaid(now.AddMinutes(1));
        var entitlement = PackagePurchaseEntitlement.CreateFromSnapshot(nurse.NurseProfileId, order.Id, orderItem.Id, snapshot, now.AddMinutes(1));
        var attemptRight = entitlement.Rights.Single(r => r.RightType == PackageBenefitRightType.PackageExamAttemptEligibility);

        context.Countries.Add(country);
        context.ExamCategories.Add(category);
        context.Exams.Add(exam);
        context.ExamVersions.Add(version);
        context.ExamQuestions.Add(question);
        context.ExamAnswerOptions.AddRange(
            new ExamAnswerOption { Id = Guid.NewGuid(), ExamQuestionId = question.Id, DisplayOrder = 1, OptionText = "A", IsCorrect = true, IsActive = true },
            new ExamAnswerOption { Id = Guid.NewGuid(), ExamQuestionId = question.Id, DisplayOrder = 2, OptionText = "B", IsCorrect = false, IsActive = true });
        context.ReportingTopics.Add(topic);
        context.ReportingProfilePublications.Add(reportingProfile);
        context.PracticeCollections.Add(practiceCollection);
        context.PracticeCollectionVersions.Add(practiceVersion);
        context.StudyMaterials.Add(material);
        context.StudyMaterialVersions.Add(materialVersion);
        context.PreparationPackageDefinitions.Add(definition);
        context.PreparationPackageVersions.Add(packageVersion);
        context.PreparationPackageOffers.Add(offer);
        context.PaymentOrders.Add(order);
        context.PackagePurchaseEntitlements.Add(entitlement);
        await context.SaveChangesAsync();

        return new PackageAttemptSeed(
            nurse.UserId,
            nurse.NurseProfileId,
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
            practiceVersion.Id);
    }

    private static NurseSeed AddNurseUser(ApplicationDbContext context)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var user = new User { Id = Guid.NewGuid(), Email = $"nurse-{Guid.NewGuid():N}@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "Nurse", IsActive = true, EmailVerified = true };
        var userRole = new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role };
        user.UserRoles.Add(userRole);
        role.UserRoles.Add(userRole);
        var nurseProfile = new NurseProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user };
        context.Roles.Add(role);
        context.Users.Add(user);
        context.UserRoles.Add(userRole);
        context.NurseProfiles.Add(nurseProfile);
        return new NurseSeed(user.Id, nurseProfile.Id);
    }

    private ApplicationDbContext CreateContext(bool throwAfterPackageStartSave = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return throwAfterPackageStartSave
            ? new ThrowAfterPackageStartSaveDbContext(options)
            : new ApplicationDbContext(options);
    }

    private static NurseRoleGuard CreateGuard(ApplicationDbContext context, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.UserId).Returns(userId);
        currentUser.SetupGet(u => u.IsAuthenticated).Returns(true);
        return new NurseRoleGuard(context, currentUser.Object);
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private sealed class ThrowAfterPackageStartSaveDbContext : ApplicationDbContext
    {
        public ThrowAfterPackageStartSaveDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var shouldThrow = ChangeTracker.Entries<ExamSession>().Any(e => e.State == EntityState.Added && e.Entity.Source == ExamSessionSource.PackageAttempt)
                && ChangeTracker.Entries<PackageBenefitRight>().Any(e => e.Entity.Status == PackageBenefitRightStatus.Consumed);

            var result = await base.SaveChangesAsync(cancellationToken);
            if (shouldThrow)
            {
                throw new DbUpdateException("Simulated post-save package session persistence failure.", new InvalidOperationException("Post-save failure."));
            }

            return result;
        }
    }

    private sealed record NurseSeed(Guid UserId, Guid NurseProfileId);

    private sealed record PackageAttemptSeed(
        Guid UserId,
        Guid NurseProfileId,
        Guid EntitlementId,
        Guid AttemptRightId,
        Guid SnapshotId,
        Guid PaymentOrderId,
        Guid PaymentOrderItemId,
        Guid PackageDefinitionId,
        Guid PackageVersionId,
        Guid PackageOfferId,
        Guid ExamId,
        Guid ExamVersionId,
        Guid ReportingProfilePublicationId,
        Guid PracticeCollectionVersionId);
}
