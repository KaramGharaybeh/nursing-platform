using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.ExamSessions.Exceptions;
using NursingPlatform.Application.PreparationPackages.ExamSessions.StartPackageExamSession;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.PreparationPackages.PackageExamSessions;

public class StartPackageExamSessionHandlerTests
{
    [Fact]
    public async Task Handle_StartPackageAttempt_WithActiveEntitlementAndAvailableRight_CreatesSessionProvenanceAndConsumesRight()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var beforeExecution = DateTime.UtcNow;
        var result = await handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default);
        var afterExecution = DateTime.UtcNow;

        var session = Assert.Single(context.ExamSessions);
        Assert.Equal(ExamSessionSource.PackageAttempt, session.Source);
        Assert.Equal(fixture.Entitlement.IncludedExamId, session.ExamId);
        Assert.Equal(fixture.Entitlement.IncludedExamVersionId, session.ExamVersionId);
        Assert.NotEqual(fixture.LatestVersion.Id, session.ExamVersionId);

        var provenance = Assert.Single(context.ExamSessionProvenances);
        Assert.Equal(session.Id, provenance.ExamSessionId);
        Assert.Equal(fixture.Entitlement.Id, provenance.PackagePurchaseEntitlementId);
        Assert.Equal(fixture.AttemptRight.Id, provenance.PackageBenefitRightId);
        Assert.Equal(fixture.Entitlement.PurchasedOfferSnapshotId, provenance.PackageOrderItemSnapshotId);
        Assert.Equal(fixture.Entitlement.PaymentOrderId, provenance.PaymentOrderId);
        Assert.Equal(fixture.Entitlement.PaymentOrderItemId, provenance.PaymentOrderItemId);
        Assert.Equal(fixture.Entitlement.PreparationPackageDefinitionId, provenance.PreparationPackageDefinitionId);
        Assert.Equal(fixture.Entitlement.PreparationPackageVersionId, provenance.PreparationPackageVersionId);
        Assert.Equal(fixture.Entitlement.PreparationPackageOfferId, provenance.PreparationPackageOfferId);
        Assert.Equal(fixture.Entitlement.IncludedExamId, provenance.IncludedExamId);
        Assert.Equal(fixture.Entitlement.IncludedExamVersionId, provenance.IncludedExamVersionId);
        Assert.Equal(fixture.Entitlement.ReportingProfilePublicationId, provenance.ReportingProfilePublicationId);
        Assert.Equal(fixture.Entitlement.PracticeCollectionVersionId, provenance.PracticeCollectionVersionId);

        Assert.Equal(PackageBenefitRightStatus.Consumed, fixture.AttemptRight.Status);
        Assert.NotNull(fixture.AttemptRight.ConsumedAt);
        Assert.InRange(fixture.AttemptRight.ConsumedAt.Value, beforeExecution, afterExecution);
        Assert.Equal(fixture.AttemptRight.ConsumedAt, provenance.StartedAt);

        Assert.Single(context.ExamSessionQuestions);
        Assert.Equal(2, context.ExamSessionAnswerOptions.Count());
        Assert.Equal(session.Id, result.Session.Id);
        Assert.Equal("PackageAttempt", result.Source);
        Assert.DoesNotContain(result.GetType().GetProperties(), property => property.Name.Contains("Right", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.GetType().GetProperties(), property => property.Name.Contains("Provenance", StringComparison.OrdinalIgnoreCase));
        var json = JsonSerializer.Serialize(result);
        Assert.DoesNotContain("packageBenefitRightId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("benefitRightId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("examSessionProvenance", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("correct", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("answerKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rationale", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(context.ExamAccessGrants);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithExpiredEntitlement_DeniesWithoutConsumingRight()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-30), now.AddDays(-1));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal("package-entitlement-inactive", exception.Code);
        Assert.Equal(PackageBenefitRightStatus.Available, fixture.AttemptRight.Status);
        Assert.Null(fixture.AttemptRight.ConsumedAt);
        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithAttemptRightOutsideAccessWindow_DeniesWithoutConsumingRight()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        SetPrivateProperty(fixture.AttemptRight, nameof(PackageBenefitRight.AccessStartsAt), now.AddDays(1));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal("package-entitlement-inactive", exception.Code);
        Assert.Equal(PackageBenefitRightStatus.Available, fixture.AttemptRight.Status);
        Assert.Null(fixture.AttemptRight.ConsumedAt);
        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithForeignEntitlement_DeniesWithoutExposingEntitlementFacts()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var ownerFixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        var otherNurse = AddNurseUser(context);
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, otherNurse.UserId));

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(ownerFixture.Entitlement.Id), default));

        Assert.DoesNotContain(ownerFixture.Entitlement.Id.ToString(), exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(PackageBenefitRightStatus.Available, ownerFixture.AttemptRight.Status);
        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithMissingAttemptRight_DeniesWithoutCreatingSession()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        context.PackageBenefitRights.Remove(fixture.AttemptRight);
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal("package-attempt-right-missing", exception.Code);
        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_UsesEntitlementIncludedExamVersion_NotLatestPublishedVersion()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var result = await handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default);

        Assert.Equal(fixture.IncludedVersion.Id, result.IncludedExamVersionId);
        Assert.Equal(fixture.IncludedVersion.Id, Assert.Single(context.ExamSessions).ExamVersionId);
        Assert.NotEqual(fixture.LatestVersion.Id, Assert.Single(context.ExamSessions).ExamVersionId);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_DoesNotReadOrCreateExamAccessGrant()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext(throwOnExamAccessGrantRead: true);
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        await handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default);

        Assert.False(context.ExamAccessGrantsRead);
        Assert.Empty(context.Set<ExamAccessGrant>());
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithConsumedRightAndInProgressMatchingSession_ReturnsSameSession()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-10), now.AddDays(1));
        await context.SaveChangesAsync();
        var existing = SeedPackageSession(context, fixture, now.AddMinutes(-1), ExamSessionStatus.InProgress);
        fixture.AttemptRight.ConsumePackageExamAttempt(now.AddMinutes(-1));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var result = await handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default);

        Assert.Equal(existing.Id, result.Session.Id);
        Assert.Single(context.ExamSessions);
        Assert.Single(context.ExamSessionProvenances);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithConsumedRightAndTerminalSession_ReturnsConsumedOutcome()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-10), now.AddDays(1));
        await context.SaveChangesAsync();
        SeedPackageSession(context, fixture, now.AddMinutes(-30), ExamSessionStatus.Submitted);
        fixture.AttemptRight.ConsumePackageExamAttempt(now.AddMinutes(-30));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal("package-attempt-consumed", exception.Code);
        Assert.Single(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithDifferentSourceInProgress_ReturnsSourceConflictWithoutConsumingRight()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        context.ExamSessions.Add(ExamSession.Create(fixture.NurseProfileId, fixture.Entitlement.IncludedExamId, fixture.Entitlement.IncludedExamVersionId, now.AddMinutes(-1), 60, ExamSessionSource.Free));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal("exam-session-source-conflict", exception.Code);
        Assert.Equal(PackageBenefitRightStatus.Available, fixture.AttemptRight.Status);
        Assert.Null(fixture.AttemptRight.ConsumedAt);
        Assert.Single(context.ExamSessions);
        Assert.Empty(context.ExamSessionProvenances);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WhenRightConsumptionFails_DoesNotPersistSession()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        context.ThrowOnConsumedRightSave = true;
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Empty(context.ExamSessions);
        Assert.Empty(context.ExamSessionProvenances);
        Assert.Equal(PackageBenefitRightStatus.Available, fixture.AttemptRight.Status);
        Assert.Null(fixture.AttemptRight.ConsumedAt);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithLegacySessionSource_DoesNotSatisfyPackageStart()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        context.ExamSessions.Add(ExamSession.Create(fixture.NurseProfileId, fixture.Entitlement.IncludedExamId, fixture.Entitlement.IncludedExamVersionId, now.AddMinutes(-1), 60, ExamSessionSource.Legacy));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal("exam-session-source-conflict", exception.Code);
        Assert.Empty(context.ExamSessionProvenances);
        Assert.Equal(PackageBenefitRightStatus.Available, fixture.AttemptRight.Status);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithDifferentPackageEntitlementInProgressForSameExamVersion_ReturnsSourceConflict()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var first = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        var second = SeedAdditionalEntitlementForSamePackageExam(context, first, now.AddDays(-1), now.AddDays(30));
        SeedPackageSession(context, first, now.AddMinutes(-1), ExamSessionStatus.InProgress);
        first.AttemptRight.ConsumePackageExamAttempt(now.AddMinutes(-1));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, second.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(second.Entitlement.Id), default));

        Assert.Equal("exam-session-source-conflict", exception.Code);
        Assert.Equal(PackageBenefitRightStatus.Available, second.AttemptRight.Status);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_AfterEntitlementExpiryButSessionInProgress_ReturnsExistingSessionWithoutRecheckingWindow()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-10), now.AddDays(-1));
        await context.SaveChangesAsync();
        var existing = SeedPackageSession(context, fixture, now.AddMinutes(-1), ExamSessionStatus.InProgress);
        fixture.AttemptRight.ConsumePackageExamAttempt(now.AddDays(-2));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var result = await handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default);

        Assert.Equal(existing.Id, result.Session.Id);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithExpiredInProgressMatchingSession_FinalizesAndReturnsConsumedOutcomeWithoutReplacement()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-10), now.AddDays(1));
        await context.SaveChangesAsync();
        var existing = SeedPackageSession(context, fixture, now.AddMinutes(-90), ExamSessionStatus.InProgress);
        fixture.AttemptRight.ConsumePackageExamAttempt(now.AddMinutes(-90));
        await context.SaveChangesAsync();
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal("package-attempt-consumed", exception.Code);
        Assert.Equal(ExamSessionStatus.Expired, existing.Status);
        Assert.NotNull(existing.FinalizedAt);
        Assert.Single(context.ExamSessions);
        Assert.Single(context.ExamSessionProvenances);
        Assert.Equal(PackageBenefitRightStatus.Consumed, fixture.AttemptRight.Status);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WithExpiredInProgressMatchingSessionAlreadyFinalized_ReturnsConsumedOutcomeWithoutReplacement()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-10), now.AddDays(1));
        await context.SaveChangesAsync();
        var existing = SeedPackageSession(context, fixture, now.AddMinutes(-90), ExamSessionStatus.InProgress);
        fixture.AttemptRight.ConsumePackageExamAttempt(now.AddMinutes(-90));
        await context.SaveChangesAsync();
        context.ReturnZeroRowsOnFinalization = true;
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        var exception = await Assert.ThrowsAsync<PackageExamSessionConflictException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal("package-attempt-consumed", exception.Code);
        Assert.NotEqual(ExamSessionStatus.Submitted, existing.Status);
        Assert.Single(context.ExamSessions);
        Assert.Single(context.ExamSessionProvenances);
        Assert.Equal(PackageBenefitRightStatus.Consumed, fixture.AttemptRight.Status);
    }

    [Fact]
    public async Task Handle_StartPackageAttempt_WhenSessionCreationFails_DoesNotConsumeRight()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext();
        var fixture = SeedPackageAttemptFixture(context, now.AddDays(-1), now.AddDays(30));
        await context.SaveChangesAsync();
        context.ThrowOnPackageSessionSave = true;
        var handler = new StartPackageExamSessionCommandHandler(context, CreateGuard(context, fixture.UserId));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            handler.Handle(new StartPackageExamSessionCommand(fixture.Entitlement.Id), default));

        Assert.Equal(PackageBenefitRightStatus.Available, fixture.AttemptRight.Status);
        Assert.Null(fixture.AttemptRight.ConsumedAt);
    }

    private static TestPackageExamSessionDbContext CreateContext(bool throwOnExamAccessGrantRead = false)
    {
        var options = new DbContextOptionsBuilder<TestPackageExamSessionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestPackageExamSessionDbContext(options) { ThrowOnExamAccessGrantRead = throwOnExamAccessGrantRead };
    }

    private static PackageAttemptFixture SeedAdditionalEntitlementForSamePackageExam(
        TestPackageExamSessionDbContext context,
        PackageAttemptFixture source,
        DateTime accessStartsAt,
        DateTime accessEndsAt)
    {
        var snapshot = PackageOrderItemSnapshot.Create(
            Guid.NewGuid(),
            "Package 2",
            "package-2",
            null,
            Guid.NewGuid(),
            "Package 2",
            "package-2",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            source.Entitlement.IncludedExamId,
            source.Entitlement.IncludedExamVersionId,
            "NCLEX RN",
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()],
            9900,
            "USD",
            Math.Max(1, (int)Math.Ceiling((accessEndsAt - accessStartsAt).TotalDays)),
            accessStartsAt);
        var paymentOrderItemId = Guid.NewGuid();
        snapshot.AssignPaymentOrderItem(paymentOrderItemId);
        var entitlement = PackagePurchaseEntitlement.CreateFromSnapshot(source.NurseProfileId, Guid.NewGuid(), paymentOrderItemId, snapshot, accessStartsAt);
        var attemptRight = entitlement.Rights.Single(r => r.RightType == PackageBenefitRightType.PackageExamAttemptEligibility);
        context.PackageOrderItemSnapshots.Add(snapshot);
        context.PackagePurchaseEntitlements.Add(entitlement);
        return new PackageAttemptFixture(source.UserId, source.NurseProfileId, entitlement, attemptRight, source.IncludedVersion, source.LatestVersion);
    }

    private static ExamSession SeedPackageSession(
        TestPackageExamSessionDbContext context,
        PackageAttemptFixture fixture,
        DateTime startedAt,
        ExamSessionStatus status)
    {
        var session = ExamSession.Create(
            fixture.NurseProfileId,
            fixture.Entitlement.IncludedExamId,
            fixture.Entitlement.IncludedExamVersionId,
            startedAt,
            60,
            ExamSessionSource.PackageAttempt);
        session.Status = status;
        context.ExamSessions.Add(session);
        context.ExamSessionProvenances.Add(ExamSessionProvenance.CreateForPackageAttempt(
            session.Id,
            fixture.Entitlement.Id,
            fixture.AttemptRight.Id,
            fixture.Entitlement.PurchasedOfferSnapshotId,
            fixture.Entitlement.PaymentOrderId,
            fixture.Entitlement.PaymentOrderItemId,
            fixture.Entitlement.PreparationPackageDefinitionId,
            fixture.Entitlement.PreparationPackageVersionId,
            fixture.Entitlement.PreparationPackageOfferId,
            fixture.Entitlement.IncludedExamId,
            fixture.Entitlement.IncludedExamVersionId,
            fixture.Entitlement.ReportingProfilePublicationId,
            fixture.Entitlement.PracticeCollectionVersionId,
            fixture.Entitlement.AccessStartsAt,
            fixture.Entitlement.AccessEndsAt,
            startedAt));
        return session;
    }

    private static PackageAttemptFixture SeedPackageAttemptFixture(
        TestPackageExamSessionDbContext context,
        DateTime accessStartsAt,
        DateTime accessEndsAt)
    {
        var nurse = AddNurseUser(context);
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            CountryId = Guid.NewGuid(),
            ExamCategoryId = Guid.NewGuid(),
            Title = "NCLEX RN",
            Slug = "nclex-rn",
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = ExamStatus.Published,
            IsFree = false
        };
        var includedVersion = new ExamVersion
        {
            Id = Guid.NewGuid(),
            ExamId = exam.Id,
            VersionNumber = 1,
            Status = ExamVersionStatus.Published
        };
        var latestVersion = new ExamVersion
        {
            Id = Guid.NewGuid(),
            ExamId = exam.Id,
            VersionNumber = 2,
            Status = ExamVersionStatus.Published
        };
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = includedVersion.Id,
            DisplayOrder = 1,
            QuestionText = "Question?",
            Explanation = "Explanation",
            QuestionType = ExamQuestionType.SingleBestAnswer,
            Points = 1,
            IsActive = true
        };
        context.Exams.Add(exam);
        context.ExamVersions.AddRange(includedVersion, latestVersion);
        context.ExamQuestions.Add(question);
        context.ExamAnswerOptions.AddRange(
            new ExamAnswerOption
            {
                Id = Guid.NewGuid(),
                ExamQuestionId = question.Id,
                DisplayOrder = 1,
                OptionText = "A",
                IsCorrect = true,
                IsActive = true
            },
            new ExamAnswerOption
            {
                Id = Guid.NewGuid(),
                ExamQuestionId = question.Id,
                DisplayOrder = 2,
                OptionText = "B",
                IsCorrect = false,
                IsActive = true
            });

        var snapshot = PackageOrderItemSnapshot.Create(
            Guid.NewGuid(),
            "Package",
            "package",
            null,
            Guid.NewGuid(),
            "Package",
            "package",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            exam.Id,
            includedVersion.Id,
            exam.Title,
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()],
            9900,
            "USD",
            Math.Max(1, (int)Math.Ceiling((accessEndsAt - accessStartsAt).TotalDays)),
            accessStartsAt);
        var paymentOrderItemId = Guid.NewGuid();
        snapshot.AssignPaymentOrderItem(paymentOrderItemId);
        var entitlement = PackagePurchaseEntitlement.CreateFromSnapshot(nurse.NurseProfileId, Guid.NewGuid(), paymentOrderItemId, snapshot, accessStartsAt);
        var attemptRight = entitlement.Rights.Single(r => r.RightType == PackageBenefitRightType.PackageExamAttemptEligibility);
        context.PackageOrderItemSnapshots.Add(snapshot);
        context.PackagePurchaseEntitlements.Add(entitlement);

        return new PackageAttemptFixture(nurse.UserId, nurse.NurseProfileId, entitlement, attemptRight, includedVersion, latestVersion);
    }

    private static NurseFixture AddNurseUser(TestPackageExamSessionDbContext context)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var user = new User { Id = Guid.NewGuid(), Email = $"nurse-{Guid.NewGuid():N}@example.com", PasswordHash = "hash", IsActive = true };
        var userRole = new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role };
        user.UserRoles.Add(userRole);
        role.UserRoles.Add(userRole);
        var nurseProfile = new NurseProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user };
        context.Roles.Add(role);
        context.Users.Add(user);
        context.UserRoles.Add(userRole);
        context.NurseProfiles.Add(nurseProfile);
        return new NurseFixture(user.Id, nurseProfile.Id);
    }

    private static NurseRoleGuard CreateGuard(TestPackageExamSessionDbContext context, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.UserId).Returns(userId);
        currentUser.SetupGet(u => u.IsAuthenticated).Returns(true);
        return new NurseRoleGuard(context, currentUser.Object);
    }

    private static void SetPrivateProperty<T>(object instance, string propertyName, T value)
    {
        instance.GetType().GetProperty(propertyName)!.SetValue(instance, value);
    }

    private sealed record NurseFixture(Guid UserId, Guid NurseProfileId);

    private sealed record PackageAttemptFixture(
        Guid UserId,
        Guid NurseProfileId,
        PackagePurchaseEntitlement Entitlement,
        PackageBenefitRight AttemptRight,
        ExamVersion IncludedVersion,
        ExamVersion LatestVersion);

    private sealed class TestPackageExamSessionDbContext : DbContext, IApplicationDbContext
    {
        public TestPackageExamSessionDbContext(DbContextOptions<TestPackageExamSessionDbContext> options)
            : base(options)
        {
        }

        public bool ThrowOnExamAccessGrantRead { get; init; }
        public bool ExamAccessGrantsRead { get; private set; }
        public bool ThrowOnPackageSessionSave { get; set; }
        public bool ThrowOnConsumedRightSave { get; set; }
        public bool ReturnZeroRowsOnFinalization { get; set; }

        public DbSet<Country> Countries => Set<Country>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<NurseProfile> NurseProfiles => Set<NurseProfile>();
        public DbSet<NurseExperience> NurseExperiences => Set<NurseExperience>();
        public DbSet<NurseEducation> NurseEducation => Set<NurseEducation>();
        public DbSet<NurseCertificate> NurseCertificates => Set<NurseCertificate>();
        public DbSet<NurseLanguage> NurseLanguages => Set<NurseLanguage>();
        public DbSet<NurseSkill> NurseSkills => Set<NurseSkill>();
        public DbSet<NurseCvDocument> NurseCvDocuments => Set<NurseCvDocument>();
        public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
        public DbSet<EmployerOrganization> EmployerOrganizations => Set<EmployerOrganization>();
        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<ExamCategory> ExamCategories => Set<ExamCategory>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<ExamVersion> ExamVersions => Set<ExamVersion>();
        public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
        public DbSet<ExamAnswerOption> ExamAnswerOptions => Set<ExamAnswerOption>();
        public DbSet<ExamAccessGrant> ExamAccessGrants
        {
            get
            {
                ExamAccessGrantsRead = true;
                if (ThrowOnExamAccessGrantRead)
                {
                    throw new InvalidOperationException("Package exam start must not read standalone exam access grants.");
                }

                return Set<ExamAccessGrant>();
            }
        }
        public DbSet<ExamSession> ExamSessions => Set<ExamSession>();
        public DbSet<ExamSessionProvenance> ExamSessionProvenances => Set<ExamSessionProvenance>();
        public DbSet<ExamSessionQuestion> ExamSessionQuestions => Set<ExamSessionQuestion>();
        public DbSet<ExamSessionAnswerOption> ExamSessionAnswerOptions => Set<ExamSessionAnswerOption>();
        public DbSet<ExamSessionAnswer> ExamSessionAnswers => Set<ExamSessionAnswer>();
        public DbSet<PaymentProduct> PaymentProducts => Set<PaymentProduct>();
        public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
        public DbSet<PaymentOrderItem> PaymentOrderItems => Set<PaymentOrderItem>();
        public DbSet<PaymentCheckoutSession> PaymentCheckoutSessions => Set<PaymentCheckoutSession>();
        public DbSet<PackageOrderItemSnapshot> PackageOrderItemSnapshots => Set<PackageOrderItemSnapshot>();
        public DbSet<PackagePurchaseEntitlement> PackagePurchaseEntitlements => Set<PackagePurchaseEntitlement>();
        public DbSet<PackageBenefitRight> PackageBenefitRights => Set<PackageBenefitRight>();
        public DbSet<PreparationPackageDefinition> PreparationPackageDefinitions => Set<PreparationPackageDefinition>();
        public DbSet<PreparationPackageVersion> PreparationPackageVersions => Set<PreparationPackageVersion>();
        public DbSet<PreparationPackageVersionMaterial> PreparationPackageVersionMaterials => Set<PreparationPackageVersionMaterial>();
        public DbSet<PreparationPackageOffer> PreparationPackageOffers => Set<PreparationPackageOffer>();
        public DbSet<StudyMaterial> StudyMaterials => Set<StudyMaterial>();
        public DbSet<StudyMaterialVersion> StudyMaterialVersions => Set<StudyMaterialVersion>();
        public DbSet<StudyMaterialVersionTopic> StudyMaterialVersionTopics => Set<StudyMaterialVersionTopic>();
        public DbSet<PracticeCollection> PracticeCollections => Set<PracticeCollection>();
        public DbSet<PracticeCollectionVersion> PracticeCollectionVersions => Set<PracticeCollectionVersion>();
        public DbSet<PracticeItem> PracticeItems => Set<PracticeItem>();
        public DbSet<PracticeAnswerOption> PracticeAnswerOptions => Set<PracticeAnswerOption>();
        public DbSet<ReportingTopic> ReportingTopics => Set<ReportingTopic>();
        public DbSet<ReportingProfilePublication> ReportingProfilePublications => Set<ReportingProfilePublication>();
        public DbSet<ReportingProfileQuestionAssignment> ReportingProfileQuestionAssignments => Set<ReportingProfileQuestionAssignment>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (ThrowOnPackageSessionSave && ChangeTracker.Entries<ExamSession>().Any(e => e.State == EntityState.Added && e.Entity.Source == ExamSessionSource.PackageAttempt))
            {
                foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList())
                {
                    entry.State = EntityState.Detached;
                }

                throw new DbUpdateException("Simulated package session persistence failure.", new InvalidOperationException("Session insert failed."));
            }

            if (ThrowOnConsumedRightSave && ChangeTracker.Entries<PackageBenefitRight>().Any(e => e.Entity.Status == PackageBenefitRightStatus.Consumed))
            {
                foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList())
                {
                    entry.State = EntityState.Detached;
                }

                throw new DbUpdateException("Simulated package attempt right consumption persistence failure.", new InvalidOperationException("Right update failed."));
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public Task<IApplicationDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IApplicationDbTransaction>(new SnapshotApplicationDbTransaction(this));
        }

        public Task<int> AcquirePaymentCheckoutProviderLeaseAsync(Guid checkoutSessionId, Guid leaseId, DateTime leaseExpiresAt, DateTime timestamp, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<int> ExecutePaymentOrderPaidTransitionAsync(Guid orderId, Guid nurseProfileId, DateTime paidAt, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public bool IsUniqueEffectiveExamAccessGrantViolation(DbUpdateException exception) => false;

        public bool IsUniqueInProgressExamSessionViolation(DbUpdateException exception) => false;

        public Task<int> ExecuteContactRequestTransitionAsync(Guid id, Guid ownerProfileId, bool isEmployerOwner, ContactRequestStatus status, DateTime timestamp, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<int> ExecuteExamSessionFinalizationAsync(
            Guid id,
            Guid nurseProfileId,
            ExamSessionStatus status,
            int score,
            int maxScore,
            decimal percentage,
            bool passed,
            int correctCount,
            int questionCount,
            DateTime timestamp,
            CancellationToken cancellationToken = default)
        {
            if (ReturnZeroRowsOnFinalization)
            {
                return Task.FromResult(0);
            }

            var session = ExamSessions.SingleOrDefault(s => s.Id == id
                && s.NurseProfileId == nurseProfileId
                && s.Status == ExamSessionStatus.InProgress);

            if (session is null)
            {
                return Task.FromResult(0);
            }

            session.Status = status;
            session.Score = score;
            session.MaxScore = maxScore;
            session.Percentage = percentage;
            session.Passed = passed;
            session.CorrectCount = correctCount;
            session.QuestionCount = questionCount;
            session.FinalizedAt = timestamp;
            session.UpdatedAt = timestamp;

            return Task.FromResult(1);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            modelBuilder.Entity<PackagePurchaseEntitlement>().HasMany(e => e.Rights).WithOne().HasForeignKey(r => r.PackagePurchaseEntitlementId);
            modelBuilder.Entity<PackagePurchaseEntitlement>().Property<IReadOnlyList<Guid>>(nameof(PackagePurchaseEntitlement.StudyMaterialVersionIds));
            modelBuilder.Entity<PackageOrderItemSnapshot>().Property<IReadOnlyList<Guid>>(nameof(PackageOrderItemSnapshot.StudyMaterialVersionIds));
        }
    }

    private sealed class SnapshotApplicationDbTransaction : IApplicationDbTransaction
    {
        private readonly TestPackageExamSessionDbContext _context;
        private readonly Dictionary<Guid, (PackageBenefitRightStatus Status, DateTime? ConsumedAt)> _rightSnapshots;

        public SnapshotApplicationDbTransaction(TestPackageExamSessionDbContext context)
        {
            _context = context;
            _rightSnapshots = context.PackageBenefitRights
                .ToDictionary(r => r.Id, r => (r.Status, r.ConsumedAt));
        }

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            foreach (var right in _context.PackageBenefitRights)
            {
                if (_rightSnapshots.TryGetValue(right.Id, out var snapshot))
                {
                    typeof(PackageBenefitRight).GetProperty(nameof(PackageBenefitRight.Status))!.SetValue(right, snapshot.Status);
                    typeof(PackageBenefitRight).GetProperty(nameof(PackageBenefitRight.ConsumedAt))!.SetValue(right, snapshot.ConsumedAt);
                }
            }

            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
