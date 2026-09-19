using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.ExamSessions.GetMyPackageExamSessionState;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.PreparationPackages.PackageExamSessions;

public class GetMyPackageExamSessionStateHandlerTests
{
    [Fact]
    public async Task Handle_NoPackageSession_ReturnsEmptyStateWithoutWriting()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        var context = CreateContext(fixture);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, fixture.UserId, true));

        var result = await handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default);

        Assert.False(result.HasSession);
        Assert.Null(result.SessionId);
        Assert.Null(result.ExamId);
        Assert.Null(result.Status);
        Assert.Null(result.ExpiresAt);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PackageInProgressSession_ReturnsSessionIdentityAndStatus()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        var session = AddPackageSession(fixture, ExamSessionStatus.InProgress, now.AddHours(-1), 60);
        var context = CreateContext(fixture);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, fixture.UserId, true));

        var result = await handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default);

        Assert.True(result.HasSession);
        Assert.Equal(session.Id, result.SessionId);
        Assert.Equal(fixture.ExamId, result.ExamId);
        Assert.Equal(ExamSessionStatus.InProgress.ToString(), result.Status);
        Assert.Equal(session.ExpiresAt, result.ExpiresAt);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SubmittedPackageSession_ReturnsFinalizedState()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        var session = AddPackageSession(fixture, ExamSessionStatus.Submitted, now.AddHours(-2), 60);
        var context = CreateContext(fixture);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, fixture.UserId, true));

        var result = await handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default);

        Assert.True(result.HasSession);
        Assert.Equal(session.Id, result.SessionId);
        Assert.Equal(ExamSessionStatus.Submitted.ToString(), result.Status);
    }

    [Fact]
    public async Task Handle_ExpiredPackageSession_ReturnsFinalizedState()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        var session = AddPackageSession(fixture, ExamSessionStatus.Expired, now.AddHours(-2), 60);
        var context = CreateContext(fixture);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, fixture.UserId, true));

        var result = await handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default);

        Assert.True(result.HasSession);
        Assert.Equal(session.Id, result.SessionId);
        Assert.Equal(ExamSessionStatus.Expired.ToString(), result.Status);
    }

    [Fact]
    public async Task Handle_StandaloneSessionForSameExam_IsNotReturned()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        fixture.Sessions.Add(ExamSession.Create(
            fixture.NurseProfileId, fixture.ExamId, fixture.ExamVersionId, now.AddHours(-1), 60));
        var context = CreateContext(fixture);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, fixture.UserId, true));

        var result = await handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default);

        Assert.False(result.HasSession);
        Assert.Null(result.SessionId);
    }

    [Fact]
    public async Task Handle_SessionFromAnotherEntitlement_IsNotReturned()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        var otherSession = ExamSession.Create(
            fixture.NurseProfileId, fixture.ExamId, fixture.ExamVersionId, now.AddHours(-1), 60);
        otherSession.Status = ExamSessionStatus.InProgress;
        fixture.Sessions.Add(otherSession);
        fixture.Provenances.Add(ExamSessionProvenance.CreateForPackageAttempt(
            otherSession.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), fixture.DefinitionId, fixture.VersionId, Guid.NewGuid(), fixture.ExamId,
            fixture.ExamVersionId, Guid.NewGuid(), Guid.NewGuid(), now.AddDays(-1), now.AddDays(30), now));
        var context = CreateContext(fixture);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, fixture.UserId, true));

        var result = await handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default);

        Assert.False(result.HasSession);
        Assert.Null(result.SessionId);
    }

    [Fact]
    public async Task Handle_ForeignEntitlement_ThrowsKeyNotFoundWithoutRevealingData()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        var other = CreateNurse("other@nurse.test");
        var context = CreateContext(fixture, [fixture.Nurse, other]);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, other.UserId, true));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default));
    }

    [Fact]
    public async Task Handle_UnauthenticatedCaller_ThrowsUnauthorized()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        var context = CreateContext(fixture);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, null, false));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default));
    }

    [Fact]
    public async Task Handle_RepeatedReads_AreNonConsumptiveAndIdempotent()
    {
        var now = DateTime.UtcNow;
        var fixture = CreateFixture(now.AddDays(-1), now.AddDays(30));
        var session = AddPackageSession(fixture, ExamSessionStatus.InProgress, now.AddHours(-1), 60);
        var context = CreateContext(fixture);
        var handler = new GetMyPackageExamSessionStateQueryHandler(
            context.Object, CreateGuard(context.Object, fixture.UserId, true));

        var first = await handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default);
        var second = await handler.Handle(new GetMyPackageExamSessionStateQuery(fixture.EntitlementId), default);

        Assert.Equal(session.Id, first.SessionId);
        Assert.Equal(session.Id, second.SessionId);
        Assert.Equal(first.Status, second.Status);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ExamSession AddPackageSession(
        StateFixture fixture, ExamSessionStatus status, DateTime startedAt, int durationMinutes)
    {
        var session = ExamSession.Create(
            fixture.NurseProfileId, fixture.ExamId, fixture.ExamVersionId, startedAt, durationMinutes,
            ExamSessionSource.PackageAttempt);
        session.Status = status;
        fixture.Sessions.Add(session);
        fixture.Provenances.Add(ExamSessionProvenance.CreateForPackageAttempt(
            session.Id, fixture.EntitlementId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), fixture.DefinitionId, fixture.VersionId, Guid.NewGuid(), fixture.ExamId,
            fixture.ExamVersionId, Guid.NewGuid(), Guid.NewGuid(), startedAt, startedAt.AddDays(30), startedAt));
        return session;
    }

    private static Mock<IApplicationDbContext> CreateContext(
        StateFixture fixture, IReadOnlyCollection<NurseFixture>? nurses = null)
    {
        var context = new Mock<IApplicationDbContext>();
        var allNurses = nurses ?? [fixture.Nurse];
        context.Setup(db => db.Users).Returns(allNurses.Select(n => n.User).ToList().AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.NurseProfiles).Returns(allNurses.Select(n => n.Profile).ToList().AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.PackagePurchaseEntitlements).Returns(new List<PackagePurchaseEntitlement> { fixture.Entitlement }.AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.ExamSessionProvenances).Returns(fixture.Provenances.AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.ExamSessions).Returns(fixture.Sessions.AsQueryable().BuildMockDbSet().Object);
        context.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return context;
    }

    private static NurseRoleGuard CreateGuard(IApplicationDbContext context, Guid? userId, bool authenticated)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(user => user.UserId).Returns(userId);
        currentUser.SetupGet(user => user.IsAuthenticated).Returns(authenticated);
        return new NurseRoleGuard(context, currentUser.Object);
    }

    private static StateFixture CreateFixture(DateTime startsAt, DateTime endsAt)
    {
        var nurse = CreateNurse("nurse@nurse.test");
        var examId = Guid.NewGuid();
        var examVersionId = Guid.NewGuid();
        var entitlement = PackagePurchaseEntitlement.CreateFromSnapshot(
            nurse.NurseProfileId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            PackageOrderItemSnapshot.Create(
                Guid.NewGuid(), "Package", "package", null, Guid.NewGuid(), "Package", "package",
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, examId, examVersionId,
                "Included assessment", Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid()], 9900, "USD", 30, startsAt),
            startsAt);
        return new StateFixture(nurse, entitlement, examId, examVersionId);
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

    private sealed class StateFixture
    {
        public StateFixture(NurseFixture nurse, PackagePurchaseEntitlement entitlement, Guid examId, Guid examVersionId)
        {
            Nurse = nurse;
            Entitlement = entitlement;
            ExamId = examId;
            ExamVersionId = examVersionId;
        }

        public NurseFixture Nurse { get; }
        public PackagePurchaseEntitlement Entitlement { get; }
        public Guid EntitlementId => Entitlement.Id;
        public Guid UserId => Nurse.UserId;
        public Guid NurseProfileId => Nurse.NurseProfileId;
        public Guid ExamId { get; }
        public Guid ExamVersionId { get; }
        public Guid DefinitionId { get; } = Guid.NewGuid();
        public Guid VersionId { get; } = Guid.NewGuid();
        public List<ExamSessionProvenance> Provenances { get; } = [];
        public List<ExamSession> Sessions { get; } = [];
    }
}
