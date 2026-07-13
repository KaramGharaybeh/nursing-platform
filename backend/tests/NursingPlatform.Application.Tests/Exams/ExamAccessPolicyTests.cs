using Microsoft.EntityFrameworkCore;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Exams.Commands.StartExamSession;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Exams.Queries.GetExam;
using NursingPlatform.Application.Exams.Queries.ListExams;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Exams;

public class ExamAccessPolicyTests
{
    [Fact]
    public async Task AuthorizeStartAsync_FreeExamWithoutPaidProduct_AllowsStartWithoutGrant()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        var result = await handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default);

        Assert.Equal(exam.Id, result.ExamId);
        Assert.Single(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_ExamMarkedNotFreeWithoutPaidProduct_RequiresGrant()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        exam.IsFree = false;
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default));

        Assert.Empty(context.ExamSessions);
        Assert.Empty(context.ExamSessionQuestions);
        Assert.Empty(context.ExamSessionAnswerOptions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_ExamMarkedNotFreeWithoutProductWithValidGrant_AllowsStart()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        exam.IsFree = false;
        SeedGrant(context, user.NurseProfileId, exam.Id, expiresAt: null);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        var result = await handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default);

        Assert.Equal(exam.Id, result.ExamId);
        Assert.Single(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_DirectHandlerInvocationCannotBypassNotFreeExamEnforcement()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        exam.IsFree = false;
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default));

        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_PaidExamWithNonExpiringGrant_AllowsStart()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        SeedGrant(context, user.NurseProfileId, exam.Id, expiresAt: null);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        var result = await handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default);

        Assert.Equal(exam.Id, result.ExamId);
        Assert.Single(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_PaidExamWithFutureExpiryGrant_AllowsStart()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        SeedGrant(context, user.NurseProfileId, exam.Id, DateTime.UtcNow.AddMinutes(10));
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        var result = await handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default);

        Assert.Equal(exam.Id, result.ExamId);
        Assert.Single(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_PaidExamWithoutGrant_DeniesAndCreatesNoSession()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default));

        Assert.Empty(context.ExamSessions);
        Assert.Empty(context.ExamSessionQuestions);
        Assert.Empty(context.ExamSessionAnswerOptions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_ForeignGrant_DeniesAndCreatesNoSession()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        SeedGrant(context, Guid.NewGuid(), exam.Id, expiresAt: null);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default));

        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_GrantForAnotherExam_DeniesAndCreatesNoSession()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        var otherExam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        SeedGrant(context, user.NurseProfileId, otherExam.Id, expiresAt: null);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default));

        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_ExpiredGrant_DeniesAndCreatesNoSession()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        SeedGrant(context, user.NurseProfileId, exam.Id, DateTime.UtcNow.AddMinutes(-1));
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default));

        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_GrantExpiringExactlyNow_DeniesAndCreatesNoSession()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateContext(() => now);
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        SeedGrant(context, user.NurseProfileId, exam.Id, now);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId, () => now);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default));

        Assert.Empty(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_DirectPolicyInvocationCannotBypassPaidAccessEnforcement()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        exam.IsFree = false;
        await context.SaveChangesAsync();
        var policy = new ExamAccessPolicy(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            policy.AuthorizeStartAsync(user.NurseProfileId, exam.Id, default));
    }

    [Fact]
    public async Task AuthorizeStartAsync_InactivePaymentProduct_DoesNotMakeExamPaid()
    {
        await AssertFreeProductVariantAllowsStart(product => product.IsActive = false);
    }

    [Fact]
    public async Task AuthorizeStartAsync_ZeroPricedPaymentProduct_DoesNotMakeExamPaid()
    {
        await AssertFreeProductVariantAllowsStart(product => product.UnitAmountMinor = 0);
    }

    [Fact]
    public async Task AuthorizeStartAsync_NonExamAccessProduct_DoesNotMakeExamPaid()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        context.PaymentProducts.Add(new PaymentProduct
        {
            Id = Guid.NewGuid(),
            Type = (PaymentProductType)999,
            ExamId = exam.Id,
            Name = "Bundle",
            Currency = "USD",
            UnitAmountMinor = 1000,
            IsActive = true
        });
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        var result = await handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default);

        Assert.Equal(exam.Id, result.ExamId);
        Assert.Single(context.ExamSessions);
    }

    [Fact]
    public async Task AuthorizeStartAsync_MultiplePaidProductsForSameExam_RequireOnlyOneValidGrant()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id, name: "First");
        SeedPaidProduct(context, exam.Id, name: "Second");
        SeedGrant(context, user.NurseProfileId, exam.Id, expiresAt: null);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        var result = await handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default);

        Assert.Equal(exam.Id, result.ExamId);
        Assert.Single(context.ExamSessions);
    }

    [Fact]
    public async Task Handle_WithExistingActiveSession_PreservesResumeBehaviorAfterAccessCheck()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        await context.SaveChangesAsync();
        var version = await context.ExamVersions.SingleAsync(v => v.ExamId == exam.Id);
        SeedPaidProduct(context, exam.Id);
        SeedGrant(context, user.NurseProfileId, exam.Id, expiresAt: null);
        var existing = ExamSession.Create(user.NurseProfileId, exam.Id, version.Id, DateTime.UtcNow.AddMinutes(-1), exam.DurationMinutes);
        context.ExamSessions.Add(existing);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        var result = await handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default);

        Assert.Equal(existing.Id, result.Id);
        Assert.Single(context.ExamSessions);
    }

    [Fact]
    public async Task GetExam_FreeMarkedExamWithPaidProductWithoutGrant_ReturnsNotFreeAndCannotStart()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        await context.SaveChangesAsync();
        var handler = new GetExamQueryHandler(context, CreateGuard(context, user.UserId));

        var result = await handler.Handle(new GetExamQuery { ExamId = exam.Id }, default);

        Assert.False(result.IsFree);
        Assert.False(result.CanStart);
    }

    [Fact]
    public async Task ListExams_FreeMarkedExamWithPaidProductWithoutGrant_DoesNotListAsStartable()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        await context.SaveChangesAsync();
        var handler = new ListExamsQueryHandler(context, CreateGuard(context, user.UserId));

        var result = await handler.Handle(new ListExamsQuery(), default);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task ListExams_FreeMarkedExamWithPaidProductAndValidGrant_ListsAsPaidAndStartable()
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        SeedPaidProduct(context, exam.Id);
        SeedGrant(context, user.NurseProfileId, exam.Id, expiresAt: null);
        await context.SaveChangesAsync();
        var handler = new ListExamsQueryHandler(context, CreateGuard(context, user.UserId));

        var result = await handler.Handle(new ListExamsQuery(), default);

        var item = Assert.Single(result.Items);
        Assert.Equal(exam.Id, item.Id);
        Assert.False(item.IsFree);
        Assert.True(item.CanStart);
    }

    private static async Task AssertFreeProductVariantAllowsStart(Action<PaymentProduct> mutateProduct)
    {
        await using var context = CreateContext();
        var user = SeedNurse(context);
        var exam = SeedStartableExam(context);
        var product = SeedPaidProduct(context, exam.Id);
        mutateProduct(product);
        await context.SaveChangesAsync();
        var handler = CreateHandler(context, user.UserId);

        var result = await handler.Handle(new StartExamSessionCommand { ExamId = exam.Id }, default);

        Assert.Equal(exam.Id, result.ExamId);
        Assert.Single(context.ExamSessions);
    }

    private static StartExamSessionCommandHandler CreateHandler(
        TestExamAccessDbContext context,
        Guid userId,
        Func<DateTime>? clock = null)
    {
        return new StartExamSessionCommandHandler(
            context,
            CreateGuard(context, userId),
            clock is null ? new ExamAccessPolicy(context) : new ExamAccessPolicy(context, clock));
    }

    private static TestExamAccessDbContext CreateContext(Func<DateTime>? clock = null)
    {
        var options = new DbContextOptionsBuilder<TestExamAccessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestExamAccessDbContext(options, clock);
    }

    private static SeededNurse SeedNurse(TestExamAccessDbContext context)
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var user = new User { Id = userId, Email = $"nurse-{Guid.NewGuid():N}@example.com", IsActive = true };
        user.UserRoles.Add(new UserRole { User = user, UserId = userId, Role = role, RoleId = role.Id });
        context.Users.Add(user);
        context.Roles.Add(role);
        context.NurseProfiles.Add(new NurseProfile { Id = nurseProfileId, UserId = userId });
        return new SeededNurse(userId, nurseProfileId);
    }

    private static Exam SeedStartableExam(TestExamAccessDbContext context)
    {
        var country = new Country
        {
            Id = Guid.NewGuid(),
            Name = $"Country {Guid.NewGuid():N}"[..20],
            Code = Guid.NewGuid().ToString("N")[..2].ToUpperInvariant(),
            IsActive = true
        };
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            CountryId = country.Id,
            Title = $"Exam {Guid.NewGuid():N}",
            Slug = Guid.NewGuid().ToString("N"),
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = ExamStatus.Published,
            IsFree = true
        };
        var version = new ExamVersion
        {
            Id = Guid.NewGuid(),
            ExamId = exam.Id,
            VersionNumber = 1,
            Status = ExamVersionStatus.Published
        };
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = version.Id,
            DisplayOrder = 1,
            QuestionText = "Question?",
            QuestionType = ExamQuestionType.SingleBestAnswer,
            Points = 1,
            IsActive = true
        };
        context.Countries.Add(country);
        context.Exams.Add(exam);
        context.ExamVersions.Add(version);
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
        return exam;
    }

    private static PaymentProduct SeedPaidProduct(TestExamAccessDbContext context, Guid examId, string name = "Exam Access")
    {
        var product = new PaymentProduct
        {
            Id = Guid.NewGuid(),
            Type = PaymentProductType.ExamAccess,
            ExamId = examId,
            Name = name,
            Currency = "USD",
            UnitAmountMinor = 1000,
            IsActive = true
        };
        context.PaymentProducts.Add(product);
        return product;
    }

    private static void SeedGrant(TestExamAccessDbContext context, Guid nurseProfileId, Guid examId, DateTime? expiresAt)
    {
        context.ExamAccessGrants.Add(new ExamAccessGrant
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            ExamId = examId,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            Reason = "Test"
        });
    }

    private static NurseRoleGuard CreateGuard(TestExamAccessDbContext context, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.UserId).Returns(userId);
        currentUser.SetupGet(u => u.IsAuthenticated).Returns(true);
        return new NurseRoleGuard(context, currentUser.Object);
    }

    private sealed record SeededNurse(Guid UserId, Guid NurseProfileId);

    private sealed class TestExamAccessDbContext : DbContext, IApplicationDbContext
    {
        private readonly Func<DateTime>? _clock;

        public TestExamAccessDbContext(DbContextOptions<TestExamAccessDbContext> options, Func<DateTime>? clock)
            : base(options)
        {
            _clock = clock;
        }

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
        public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
        public DbSet<PaymentOrderItem> PaymentOrderItems => Set<PaymentOrderItem>();
        public DbSet<PaymentCheckoutSession> PaymentCheckoutSessions => Set<PaymentCheckoutSession>();

        public Task<IApplicationDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IApplicationDbTransaction>(new NoopApplicationDbTransaction());
        }

        public Task<int> AcquirePaymentCheckoutProviderLeaseAsync(Guid checkoutSessionId, Guid leaseId, DateTime leaseExpiresAt, DateTime timestamp, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<int> ExecutePaymentOrderPaidTransitionAsync(Guid orderId, Guid nurseProfileId, DateTime paidAt, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public bool IsUniqueEffectiveExamAccessGrantViolation(DbUpdateException exception) => false;

        public Task<int> ExecuteContactRequestTransitionAsync(Guid id, Guid ownerProfileId, bool isEmployerOwner, ContactRequestStatus status, DateTime timestamp, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<int> ExecuteExamSessionFinalizationAsync(Guid id, Guid nurseProfileId, ExamSessionStatus status, int score, int maxScore, decimal percentage, bool passed, int correctCount, int questionCount, DateTime timestamp, CancellationToken cancellationToken = default)
        {
            var session = ExamSessions.SingleOrDefault(s => s.Id == id && s.NurseProfileId == nurseProfileId && s.Status == ExamSessionStatus.InProgress);
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
        }

        public DateTime UtcNow() => _clock?.Invoke() ?? DateTime.UtcNow;
    }

    private sealed class NoopApplicationDbTransaction : IApplicationDbTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
