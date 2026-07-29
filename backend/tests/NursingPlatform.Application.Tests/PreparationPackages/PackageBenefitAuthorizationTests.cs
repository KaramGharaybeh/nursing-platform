using Microsoft.EntityFrameworkCore;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.PreparationPackages.Authorization;
using NursingPlatform.Application.PreparationPackages.Entitlements.GetMyPackageEntitlement;
using NursingPlatform.Application.PreparationPackages.Entitlements.ListMyPackageEntitlements;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.PreparationPackages;

public class PackageBenefitAuthorizationTests
{
    [Fact]
    public async Task Handle_AuthorizePackageBenefit_WithActiveRightAndWindow_AllowsAccessWithoutPaymentLookup()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        await context.SaveChangesAsync();
        var service = CreateService(context, fixture.UserId);

        var result = await service.AuthorizeAsync(fixture.Entitlement.Id, PackageBenefitRightType.MaterialsAccess, DateTime.UtcNow, default);

        Assert.True(result.IsAuthorized);
        Assert.Equal("Available", result.RightStatus);
        Assert.Equal("Active", result.EntitlementStatus);
        Assert.Equal(fixture.Entitlement.Id, result.PackagePurchaseEntitlementId);
        Assert.False(context.PaymentOrdersRead);
    }

    [Fact]
    public async Task Handle_AuthorizePackageBenefit_DoesNotQueryPaymentOrderStatus()
    {
        await using var context = CreateContext(throwOnPaymentOrderRead: true);
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        await context.SaveChangesAsync();
        var service = CreateService(context, fixture.UserId);

        var result = await service.AuthorizeAsync(fixture.Entitlement.Id, PackageBenefitRightType.PracticeAccess, DateTime.UtcNow, default);

        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public async Task Handle_AuthorizePackageBenefit_WithExpiredWindow_DeniesAccess()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1));
        await context.SaveChangesAsync();
        var service = CreateService(context, fixture.UserId);

        var result = await service.AuthorizeAsync(fixture.Entitlement.Id, PackageBenefitRightType.MaterialsAccess, DateTime.UtcNow, default);

        Assert.False(result.IsAuthorized);
        Assert.Equal("EntitlementInactiveOrOutsideAccessWindow", result.DenialReason);
        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_AuthorizePackageBenefit_WithMissingRight_DeniesAccess()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        context.PackageBenefitRights.RemoveRange(fixture.Entitlement.Rights.Where(r => r.RightType == PackageBenefitRightType.PracticeAccess).ToList());
        await context.SaveChangesAsync();
        var service = CreateService(context, fixture.UserId);

        var result = await service.AuthorizeAsync(fixture.Entitlement.Id, PackageBenefitRightType.PracticeAccess, DateTime.UtcNow, default);

        Assert.False(result.IsAuthorized);
        Assert.Equal("RightMissing", result.DenialReason);
    }

    [Theory]
    [InlineData(PackageBenefitRightStatus.Dormant)]
    [InlineData(PackageBenefitRightStatus.Consumed)]
    [InlineData(PackageBenefitRightStatus.Expired)]
    [InlineData(PackageBenefitRightStatus.Revoked)]
    public async Task Handle_AuthorizePackageBenefit_WithUnavailableRightStatus_DeniesAccess(PackageBenefitRightStatus status)
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        SetRightStatus(fixture.Entitlement, PackageBenefitRightType.MaterialsAccess, status);
        await context.SaveChangesAsync();
        var service = CreateService(context, fixture.UserId);

        var result = await service.AuthorizeAsync(fixture.Entitlement.Id, PackageBenefitRightType.MaterialsAccess, DateTime.UtcNow, default);

        Assert.False(result.IsAuthorized);
        Assert.Equal(status.ToString(), result.RightStatus);
        Assert.Equal("RightNotAvailable", result.DenialReason);
    }

    [Fact]
    public async Task Handle_AuthorizePackageBenefit_WithDormantReportRight_DoesNotAuthorizeReportAccessInStage2()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        await context.SaveChangesAsync();
        var service = CreateService(context, fixture.UserId);

        var result = await service.AuthorizeAsync(fixture.Entitlement.Id, PackageBenefitRightType.ReportEligibility, DateTime.UtcNow, default);

        Assert.False(result.IsAuthorized);
        Assert.True(result.IsVisibleDormantRight);
        Assert.Equal("Dormant", result.RightStatus);
        Assert.Equal("ReportRightDormant", result.DenialReason);
    }

    [Fact]
    public async Task Handle_AuthorizePackageBenefit_ForDifferentNurse_DeniesWithoutExposure()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        var other = AddNurseUser(context, "other@nurse.test");
        await context.SaveChangesAsync();
        var service = CreateService(context, other.UserId);

        var result = await service.AuthorizeAsync(fixture.Entitlement.Id, PackageBenefitRightType.MaterialsAccess, DateTime.UtcNow, default);

        Assert.False(result.IsAuthorized);
        Assert.Equal("NotFound", result.DenialReason);
        Assert.Null(result.PackagePurchaseEntitlementId);
    }

    [Fact]
    public async Task Handle_AuthorizePackageBenefit_ForDifferentPackage_DeniesAccess()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        await context.SaveChangesAsync();
        var service = CreateService(context, fixture.UserId);

        var result = await service.AuthorizeAsync(Guid.NewGuid(), PackageBenefitRightType.MaterialsAccess, DateTime.UtcNow, default);

        Assert.False(result.IsAuthorized);
        Assert.Equal("NotFound", result.DenialReason);
    }

    [Fact]
    public async Task Handle_AuthorizePackageBenefit_WithStandaloneExamAccessGrant_DoesNotSatisfyPackageRight()
    {
        await using var context = CreateContext();
        var nurse = AddNurseUser(context, "nurse@nurse.test");
        context.ExamAccessGrants.Add(new ExamAccessGrant { Id = Guid.NewGuid(), NurseProfileId = nurse.NurseProfileId, ExamId = Guid.NewGuid(), GrantedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var service = CreateService(context, nurse.UserId);

        var result = await service.AuthorizeAsync(Guid.NewGuid(), PackageBenefitRightType.PackageExamAttemptEligibility, DateTime.UtcNow, default);

        Assert.False(result.IsAuthorized);
        Assert.Equal("NotFound", result.DenialReason);
        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_AuthorizeStandaloneExamStart_WithPackageBenefitRight_DoesNotSatisfyExamAccessGrantRequirement()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        await context.SaveChangesAsync();

        var hasStandaloneGrant = await context.ExamAccessGrants.AnyAsync(g =>
            g.NurseProfileId == fixture.NurseProfileId && g.ExamId == fixture.Entitlement.IncludedExamId);

        Assert.False(hasStandaloneGrant);
        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_AuthorizeReportEligibility_WithStandaloneOrDifferentPackageSession_DoesNotQualifyRight()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        context.ExamSessions.Add(ExamSession.Create(fixture.NurseProfileId, fixture.Entitlement.IncludedExamId, fixture.Entitlement.IncludedExamVersionId, DateTime.UtcNow, 60));
        await context.SaveChangesAsync();
        var service = CreateService(context, fixture.UserId);

        var result = await service.AuthorizeAsync(fixture.Entitlement.Id, PackageBenefitRightType.ReportEligibility, DateTime.UtcNow, default);

        Assert.False(result.IsAuthorized);
        Assert.True(result.IsVisibleDormantRight);
        Assert.Equal("Dormant", result.RightStatus);
    }

    [Fact]
    public async Task Handle_ListMyPackageEntitlements_ReturnsDeterministicallySortedNurseOwnedSummaries()
    {
        await using var context = CreateContext();
        var now = DateTime.UtcNow;
        var current = AddNurseUser(context, "current@nurse.test");
        var other = AddNurseUser(context, "other@nurse.test");
        var older = AddEntitlement(context, current.NurseProfileId, now.AddDays(-20), now.AddDays(10), title: "Older");
        var newer = AddEntitlement(context, current.NurseProfileId, now.AddDays(-1), now.AddDays(20), title: "Newer");
        AddEntitlement(context, other.NurseProfileId, now.AddDays(1), now.AddDays(30), title: "Foreign");
        await context.SaveChangesAsync();
        var handler = new ListMyPackageEntitlementsQueryHandler(context, CreateGuard(context, current.UserId));

        var result = await handler.Handle(new ListMyPackageEntitlementsQuery { Page = 1, PageSize = 10 }, default);

        Assert.Equal(2, result.TotalCount);
        Assert.Collection(result.Items,
            item =>
            {
                Assert.Equal(newer.Entitlement.Id, item.Id);
                Assert.Equal("Newer", item.PackageOfferTitle);
                Assert.Equal(4, item.BenefitRights.Count);
                Assert.Contains(item.BenefitRights, r => r.RightType == "MaterialsAccess" && r.IsAvailable);
                Assert.Contains(item.BenefitRights, r => r.RightType == "ReportEligibility" && r.IsDormant && !r.IsAvailable);
            },
            item => Assert.Equal(older.Entitlement.Id, item.Id));
    }

    [Fact]
    public async Task Handle_GetMyPackageEntitlement_ProjectsBenefitRightStatusesWithoutInternalNavigationObjects()
    {
        await using var context = CreateContext();
        var fixture = CreateAuthorizedEntitlement(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
        await context.SaveChangesAsync();
        var handler = new GetMyPackageEntitlementQueryHandler(context, CreateGuard(context, fixture.UserId));

        var result = await handler.Handle(new GetMyPackageEntitlementQuery { Id = fixture.Entitlement.Id }, default);

        Assert.Equal(4, result.BenefitRights.Count);
        Assert.DoesNotContain(result.BenefitRights.GetType().GetProperties(), p => p.Name.Contains("Navigation", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(PackageBenefitAuthorizationResult).GetProperties(), p => p.Name.EndsWith("Id", StringComparison.Ordinal) && p.Name.Contains("Right", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.BenefitRights, r => r.RightType == "PackageExamAttemptEligibility" && r.IsAvailable);
        Assert.Contains(result.BenefitRights, r => r.RightType == "ReportEligibility" && r.IsDormant && !r.IsAvailable);
    }

    private static PackageBenefitAuthorizationService CreateService(TestPackageBenefitDbContext context, Guid userId)
    {
        return new PackageBenefitAuthorizationService(context, CreateGuard(context, userId));
    }

    private static NursingPlatform.Application.Nurses.Common.NurseRoleGuard CreateGuard(TestPackageBenefitDbContext context, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.UserId).Returns(userId);
        currentUser.SetupGet(u => u.IsAuthenticated).Returns(true);
        return new NursingPlatform.Application.Nurses.Common.NurseRoleGuard(context, currentUser.Object);
    }

    private static TestPackageBenefitDbContext CreateContext(bool throwOnPaymentOrderRead = false)
    {
        var options = new DbContextOptionsBuilder<TestPackageBenefitDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestPackageBenefitDbContext(options) { ThrowOnPaymentOrderRead = throwOnPaymentOrderRead };
    }

    private static EntitlementFixture CreateAuthorizedEntitlement(TestPackageBenefitDbContext context, DateTime startsAt, DateTime endsAt)
    {
        var nurse = AddNurseUser(context, "nurse@nurse.test");
        return AddEntitlement(context, nurse.NurseProfileId, startsAt, endsAt, userId: nurse.UserId);
    }

    private static NurseFixture AddNurseUser(TestPackageBenefitDbContext context, string email)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hash", IsActive = true };
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

    private static EntitlementFixture AddEntitlement(
        TestPackageBenefitDbContext context,
        Guid nurseProfileId,
        DateTime startsAt,
        DateTime endsAt,
        string title = "Package",
        Guid? userId = null)
    {
        var snapshot = PackageOrderItemSnapshot.Create(
            Guid.NewGuid(),
            title,
            title.ToLowerInvariant(),
            null,
            Guid.NewGuid(),
            title,
            title.ToLowerInvariant(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NCLEX RN",
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()],
            9900,
            "USD",
            Math.Max(1, (int)Math.Ceiling((endsAt - startsAt).TotalDays)),
            startsAt);
        var paymentOrderItemId = Guid.NewGuid();
        snapshot.AssignPaymentOrderItem(paymentOrderItemId);
        var entitlement = PackagePurchaseEntitlement.CreateFromSnapshot(nurseProfileId, Guid.NewGuid(), paymentOrderItemId, snapshot, startsAt);
        context.PackageOrderItemSnapshots.Add(snapshot);
        context.PackagePurchaseEntitlements.Add(entitlement);
        return new EntitlementFixture(userId ?? Guid.Empty, nurseProfileId, entitlement);
    }

    private static void SetRightStatus(PackagePurchaseEntitlement entitlement, PackageBenefitRightType rightType, PackageBenefitRightStatus status)
    {
        var right = entitlement.Rights.Single(r => r.RightType == rightType);
        typeof(PackageBenefitRight).GetProperty(nameof(PackageBenefitRight.Status))!.SetValue(right, status);
    }

    private sealed record NurseFixture(Guid UserId, Guid NurseProfileId);

    private sealed record EntitlementFixture(Guid UserId, Guid NurseProfileId, PackagePurchaseEntitlement Entitlement);

    private sealed class TestPackageBenefitDbContext : DbContext, IApplicationDbContext
    {
        public TestPackageBenefitDbContext(DbContextOptions<TestPackageBenefitDbContext> options) : base(options)
        {
        }

        public bool ThrowOnPaymentOrderRead { get; init; }
        public bool PaymentOrdersRead { get; private set; }

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
        public DbSet<ExamAccessGrant> ExamAccessGrants => Set<ExamAccessGrant>();
        public DbSet<ExamSession> ExamSessions => Set<ExamSession>();
        public DbSet<ExamSessionQuestion> ExamSessionQuestions => Set<ExamSessionQuestion>();
        public DbSet<ExamSessionAnswerOption> ExamSessionAnswerOptions => Set<ExamSessionAnswerOption>();
        public DbSet<ExamSessionAnswer> ExamSessionAnswers => Set<ExamSessionAnswer>();
        public DbSet<PaymentProduct> PaymentProducts => Set<PaymentProduct>();
        public DbSet<PaymentOrder> PaymentOrders
        {
            get
            {
                PaymentOrdersRead = true;
                if (ThrowOnPaymentOrderRead)
                {
                    throw new InvalidOperationException("Payment orders must not be read for package benefit authorization.");
                }

                return Set<PaymentOrder>();
            }
        }
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

        public Task<IApplicationDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> AcquirePaymentCheckoutProviderLeaseAsync(Guid checkoutSessionId, Guid leaseId, DateTime leaseExpiresAt, DateTime timestamp, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> ExecutePaymentOrderPaidTransitionAsync(Guid orderId, Guid nurseProfileId, DateTime paidAt, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public bool IsUniqueEffectiveExamAccessGrantViolation(DbUpdateException exception) => false;
        public bool IsUniqueInProgressExamSessionViolation(DbUpdateException exception) => false;
        public Task<int> ExecuteContactRequestTransitionAsync(Guid id, Guid ownerProfileId, bool isEmployerOwner, ContactRequestStatus status, DateTime timestamp, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> ExecuteExamSessionFinalizationAsync(Guid id, Guid nurseProfileId, ExamSessionStatus status, int score, int maxScore, decimal percentage, bool passed, int correctCount, int questionCount, DateTime timestamp, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            modelBuilder.Entity<PackagePurchaseEntitlement>().HasMany(e => e.Rights).WithOne().HasForeignKey(r => r.PackagePurchaseEntitlementId);
            modelBuilder.Entity<PackagePurchaseEntitlement>().Property<IReadOnlyList<Guid>>(nameof(PackagePurchaseEntitlement.StudyMaterialVersionIds));
            modelBuilder.Entity<PackageOrderItemSnapshot>().Property<IReadOnlyList<Guid>>(nameof(PackageOrderItemSnapshot.StudyMaterialVersionIds));
        }
    }
}
