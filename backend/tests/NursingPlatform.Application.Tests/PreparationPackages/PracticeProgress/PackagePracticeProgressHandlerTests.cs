using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.PracticeProgress;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.PreparationPackages.PracticeProgress;

public class PackagePracticeProgressHandlerTests
{
    [Fact]
    public async Task Handle_GetPackagePracticeProgress_ForOwnerAfterExpiry_DerivesCountersAndMissingRowsAsUnanswered()
    {
        var now = new DateTime(2026, 8, 2, 12, 0, 0, DateTimeKind.Utc);
        var fixture = CreatePackagePractice(now.AddDays(-30), accessDurationDays: 7, itemCount: 3);
        var first = fixture.Items[0];
        var second = fixture.Items[1];
        var firstAnswer = first.AnswerOptions.Single(option => option.IsCorrect);
        var secondAnswer = second.AnswerOptions.Single(option => !option.IsCorrect);
        var progressRows = new List<PackagePracticeProgress>
        {
            PackagePracticeProgress.Create(
                fixture.NurseProfileId,
                fixture.Entitlement.Id,
                fixture.Entitlement.PracticeCollectionVersionId,
                first.Id,
                firstAnswer.Id,
                isCorrect: true,
                now.AddDays(-20)),
            PackagePracticeProgress.Create(
                fixture.NurseProfileId,
                fixture.Entitlement.Id,
                fixture.Entitlement.PracticeCollectionVersionId,
                second.Id,
                secondAnswer.Id,
                isCorrect: false,
                now.AddDays(-19))
        };
        var context = CreateContext(fixture, progressRows);
        var handler = new GetPackagePracticeProgressQueryHandler(context.Object, CreateGuard(context.Object, fixture.UserId));

        var result = await handler.Handle(new GetPackagePracticeProgressQuery(fixture.Entitlement.Id), default);

        Assert.Equal(fixture.Entitlement.Id, result.PackagePurchaseEntitlementId);
        Assert.Equal(fixture.Entitlement.PracticeCollectionVersionId, result.PracticeCollectionVersionId);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.AnsweredCount);
        Assert.Equal(1, result.UnansweredCount);
        Assert.Equal(1, result.CorrectCount);
        Assert.Equal(1, result.IncorrectCount);
        Assert.Collection(result.ItemStates,
            item =>
            {
                Assert.Equal(first.Id, item.PracticeItemId);
                Assert.Equal(PackagePracticeProgressItemState.AnsweredCorrect, item.State);
                Assert.Equal(firstAnswer.Id, item.SelectedPracticeAnswerOptionId);
                Assert.Equal(now.AddDays(-20), item.LastAnsweredAt);
            },
            item =>
            {
                Assert.Equal(second.Id, item.PracticeItemId);
                Assert.Equal(PackagePracticeProgressItemState.AnsweredIncorrect, item.State);
                Assert.Equal(secondAnswer.Id, item.SelectedPracticeAnswerOptionId);
                Assert.Equal(now.AddDays(-19), item.LastAnsweredAt);
            },
            item =>
            {
                Assert.Equal(fixture.Items[2].Id, item.PracticeItemId);
                Assert.Equal(PackagePracticeProgressItemState.Unanswered, item.State);
                Assert.Null(item.SelectedPracticeAnswerOptionId);
                Assert.Null(item.LastAnsweredAt);
            });
    }

    [Fact]
    public async Task Handle_GetPackagePracticeProgress_ForDifferentNurse_DoesNotRevealData()
    {
        var fixture = CreatePackagePractice(DateTime.UtcNow.AddDays(-1), accessDurationDays: 30, itemCount: 1);
        var other = CreateNurse("other@nurse.test");
        var context = CreateContext(fixture, [], [fixture.Nurse, other]);
        var handler = new GetPackagePracticeProgressQueryHandler(context.Object, CreateGuard(context.Object, other.UserId));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetPackagePracticeProgressQuery(fixture.Entitlement.Id), default));
    }

    [Fact]
    public async Task Handle_SubmitPackagePracticeAnswer_WithActiveAccess_CreatesProgressFromPracticeOptionAccuracyOnly()
    {
        var now = DateTime.UtcNow;
        var fixture = CreatePackagePractice(now.AddDays(-1), accessDurationDays: 30, itemCount: 1);
        var progressRows = new List<PackagePracticeProgress>();
        var context = CreateContext(fixture, progressRows);
        var item = fixture.Items[0];
        var selectedAnswer = item.AnswerOptions.Single(option => option.IsCorrect);
        var attemptRight = fixture.Entitlement.Rights.Single(right => right.RightType == PackageBenefitRightType.PackageExamAttemptEligibility);
        var handler = new SubmitPackagePracticeAnswerCommandHandler(context.Object, CreateGuard(context.Object, fixture.UserId));

        var result = await handler.Handle(new SubmitPackagePracticeAnswerCommand(
            fixture.Entitlement.Id,
            item.Id,
            new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = selectedAnswer.Id }), default);

        Assert.Equal(item.Id, result.PracticeItemId);
        Assert.Equal(PackagePracticeProgressItemState.AnsweredCorrect, result.State);
        Assert.Equal(selectedAnswer.Id, result.SelectedPracticeAnswerOptionId);
        Assert.NotEqual(default, result.LastAnsweredAt);
        var progress = Assert.Single(progressRows);
        Assert.Equal(fixture.NurseProfileId, progress.NurseProfileId);
        Assert.Equal(fixture.Entitlement.Id, progress.PackagePurchaseEntitlementId);
        Assert.Equal(fixture.Entitlement.PracticeCollectionVersionId, progress.PracticeCollectionVersionId);
        Assert.Equal(item.Id, progress.PracticeItemId);
        Assert.True(progress.IsCorrect);
        Assert.Equal(PackageBenefitRightStatus.Available, attemptRight.Status);
        Assert.Null(attemptRight.ConsumedAt);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SubmitPackagePracticeAnswer_AfterExpiry_IsRejectedAndDoesNotWrite()
    {
        var fixture = CreatePackagePractice(DateTime.UtcNow.AddDays(-30), accessDurationDays: 7, itemCount: 1);
        var progressRows = new List<PackagePracticeProgress>();
        var context = CreateContext(fixture, progressRows);
        var item = fixture.Items[0];
        var selectedAnswer = item.AnswerOptions.Single(option => option.IsCorrect);
        var handler = new SubmitPackagePracticeAnswerCommandHandler(context.Object, CreateGuard(context.Object, fixture.UserId));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new SubmitPackagePracticeAnswerCommand(
            fixture.Entitlement.Id,
            item.Id,
            new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = selectedAnswer.Id }), default));

        Assert.Empty(progressRows);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SubmitPackagePracticeAnswer_WhenItemIsOutsidePurchasedCollection_IsRejected()
    {
        var fixture = CreatePackagePractice(DateTime.UtcNow.AddDays(-1), accessDurationDays: 30, itemCount: 1);
        var otherVersion = PracticeCollectionVersion.CreateDraft(Guid.NewGuid(), 1);
        var otherItem = CreatePracticeItem(1);
        otherVersion.AddPracticeItem(otherItem);
        var context = CreateContext(
            fixture,
            [],
            extraItems: [otherItem],
            extraAnswers: otherItem.AnswerOptions.ToList());
        var selectedAnswer = otherItem.AnswerOptions.Single(option => option.IsCorrect);
        var handler = new SubmitPackagePracticeAnswerCommandHandler(context.Object, CreateGuard(context.Object, fixture.UserId));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new SubmitPackagePracticeAnswerCommand(
            fixture.Entitlement.Id,
            otherItem.Id,
            new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = selectedAnswer.Id }), default));
    }

    [Fact]
    public async Task Handle_SubmitPackagePracticeAnswer_WhenOptionBelongsToDifferentPracticeItem_IsRejected()
    {
        var fixture = CreatePackagePractice(DateTime.UtcNow.AddDays(-1), accessDurationDays: 30, itemCount: 2);
        var context = CreateContext(fixture, []);
        var first = fixture.Items[0];
        var second = fixture.Items[1];
        var secondAnswer = second.AnswerOptions.Single(option => option.IsCorrect);
        var handler = new SubmitPackagePracticeAnswerCommandHandler(context.Object, CreateGuard(context.Object, fixture.UserId));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new SubmitPackagePracticeAnswerCommand(
            fixture.Entitlement.Id,
            first.Id,
            new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = secondAnswer.Id }), default));
    }

    [Fact]
    public async Task Handle_SubmitPackagePracticeAnswer_ReanswerOverwritesLatestProgress()
    {
        var fixture = CreatePackagePractice(DateTime.UtcNow.AddDays(-1), accessDurationDays: 30, itemCount: 1);
        var item = fixture.Items[0];
        var oldAnswer = item.AnswerOptions.Single(option => !option.IsCorrect);
        var newAnswer = item.AnswerOptions.Single(option => option.IsCorrect);
        var existing = PackagePracticeProgress.Create(
            fixture.NurseProfileId,
            fixture.Entitlement.Id,
            fixture.Entitlement.PracticeCollectionVersionId,
            item.Id,
            oldAnswer.Id,
            isCorrect: false,
            DateTime.UtcNow.AddMinutes(-5));
        var firstAnsweredAt = existing.LastAnsweredAt;
        var progressRows = new List<PackagePracticeProgress> { existing };
        var context = CreateContext(fixture, progressRows);
        var handler = new SubmitPackagePracticeAnswerCommandHandler(context.Object, CreateGuard(context.Object, fixture.UserId));

        await handler.Handle(new SubmitPackagePracticeAnswerCommand(
            fixture.Entitlement.Id,
            item.Id,
            new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = newAnswer.Id }), default);

        var progress = Assert.Single(progressRows);
        Assert.Equal(newAnswer.Id, progress.SelectedPracticeAnswerOptionId);
        Assert.True(progress.IsCorrect);
        Assert.Equal(PackagePracticeProgressState.AnsweredCorrect, progress.State);
        Assert.True(progress.LastAnsweredAt >= firstAnsweredAt);
    }

    private static Mock<IApplicationDbContext> CreateContext(
        PracticeFixture fixture,
        List<PackagePracticeProgress> progressRows,
        IReadOnlyCollection<NurseFixture>? nurses = null,
        IReadOnlyCollection<PracticeItem>? extraItems = null,
        IReadOnlyCollection<PracticeAnswerOption>? extraAnswers = null)
    {
        var context = new Mock<IApplicationDbContext>();
        var allNurses = nurses ?? [fixture.Nurse];
        var users = allNurses.Select(nurse => nurse.User).ToList();
        var profiles = allNurses.Select(nurse => nurse.Profile).ToList();
        var entitlements = new List<PackagePurchaseEntitlement> { fixture.Entitlement };
        var items = fixture.Items.Concat(extraItems ?? []).ToList();
        var answers = fixture.Items.SelectMany(item => item.AnswerOptions).Concat(extraAnswers ?? []).ToList();
        var progressSet = progressRows.AsQueryable().BuildMockDbSet();
        progressSet.Setup(set => set.Add(It.IsAny<PackagePracticeProgress>()))
            .Callback<PackagePracticeProgress>(progressRows.Add);

        context.Setup(db => db.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.PackagePurchaseEntitlements).Returns(entitlements.AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.PackagePracticeProgresses).Returns(progressSet.Object);
        context.Setup(db => db.PracticeItems).Returns(items.AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.PracticeAnswerOptions).Returns(answers.AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return context;
    }

    private static NurseRoleGuard CreateGuard(IApplicationDbContext context, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(user => user.UserId).Returns(userId);
        currentUser.SetupGet(user => user.IsAuthenticated).Returns(true);
        return new NurseRoleGuard(context, currentUser.Object);
    }

    private static PracticeFixture CreatePackagePractice(DateTime startsAt, int accessDurationDays, int itemCount)
    {
        var nurse = CreateNurse("nurse@nurse.test");
        var version = PracticeCollectionVersion.CreateDraft(Guid.NewGuid(), 1);
        for (var index = 1; index <= itemCount; index++)
        {
            version.AddPracticeItem(CreatePracticeItem(index));
        }

        var paymentOrderItemId = Guid.NewGuid();
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
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Included paid assessment",
            Guid.NewGuid(),
            version.Id,
            [Guid.NewGuid()],
            9900,
            "USD",
            accessDurationDays,
            startsAt);
        snapshot.AssignPaymentOrderItem(paymentOrderItemId);
        var entitlement = PackagePurchaseEntitlement.CreateFromSnapshot(nurse.NurseProfileId, Guid.NewGuid(), paymentOrderItemId, snapshot, startsAt);

        return new PracticeFixture(nurse, entitlement, version.Items.OrderBy(item => item.DisplayOrder).ToList());
    }

    private static PracticeItem CreatePracticeItem(int displayOrder)
    {
        var item = PracticeItem.Create(
            Guid.NewGuid(),
            $"Practice prompt {displayOrder}",
            $"Practice feedback {displayOrder}",
            displayOrder);
        item.AddAnswerOption($"Accurate practice option {displayOrder}", isCorrect: true, displayOrder: 1);
        item.AddAnswerOption($"Inaccurate practice option {displayOrder}", isCorrect: false, displayOrder: 2);
        return item;
    }

    private static NurseFixture CreateNurse(string email)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hash", IsActive = true };
        var userRole = new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role };
        user.UserRoles.Add(userRole);
        role.UserRoles.Add(userRole);
        var profile = new NurseProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user };
        return new NurseFixture(user, profile);
    }

    private sealed record NurseFixture(User User, NurseProfile Profile)
    {
        public Guid UserId => User.Id;
        public Guid NurseProfileId => Profile.Id;
    }

    private sealed record PracticeFixture(
        NurseFixture Nurse,
        PackagePurchaseEntitlement Entitlement,
        IReadOnlyList<PracticeItem> Items)
    {
        public Guid UserId => Nurse.UserId;
        public Guid NurseProfileId => Nurse.NurseProfileId;
    }
}
