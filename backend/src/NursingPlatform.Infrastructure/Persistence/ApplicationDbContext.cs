using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public async Task<IApplicationDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken);
        return new ApplicationDbTransaction(transaction);
    }

    public Task<int> AcquirePaymentCheckoutProviderLeaseAsync(
        Guid checkoutSessionId,
        Guid leaseId,
        DateTime leaseExpiresAt,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        return PaymentCheckoutSessions
            .Where(s => s.Id == checkoutSessionId
                && s.Status == PaymentCheckoutSessionStatus.Created
                && s.ExpiresAt > timestamp
                && (s.ProviderCallLeaseId == null || s.ProviderCallLeaseExpiresAt <= timestamp))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.ProviderCallLeaseId, leaseId)
                .SetProperty(s => s.ProviderCallLeaseExpiresAt, leaseExpiresAt)
                .SetProperty(s => s.UpdatedAt, timestamp), cancellationToken);
    }

    public Task<int> ExecuteContactRequestTransitionAsync(
        Guid id,
        Guid ownerProfileId,
        bool isEmployerOwner,
        ContactRequestStatus status,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var query = ContactRequests.Where(r => r.Id == id && r.Status == ContactRequestStatus.Pending);
        query = isEmployerOwner
            ? query.Where(r => r.EmployerProfileId == ownerProfileId)
            : query.Where(r => r.NurseProfileId == ownerProfileId);

        return query.ExecuteUpdateAsync(setters =>
            setters
                .SetProperty(r => r.Status, status)
                .SetProperty(r => r.UpdatedAt, timestamp)
                .SetProperty(r => r.RespondedAt,
                    status is ContactRequestStatus.Approved or ContactRequestStatus.Rejected
                        ? timestamp
                        : (DateTime?)null)
                .SetProperty(r => r.CancelledAt,
                    status == ContactRequestStatus.Cancelled
                        ? timestamp
                        : (DateTime?)null),
            cancellationToken);
    }

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
        return ExamSessions
            .Where(s => s.Id == id
                && s.NurseProfileId == nurseProfileId
                && s.Status == ExamSessionStatus.InProgress)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(s => s.Status, status)
                    .SetProperty(s => s.Score, score)
                    .SetProperty(s => s.MaxScore, maxScore)
                    .SetProperty(s => s.Percentage, percentage)
                    .SetProperty(s => s.Passed, passed)
                    .SetProperty(s => s.CorrectCount, correctCount)
                    .SetProperty(s => s.QuestionCount, questionCount)
                    .SetProperty(s => s.FinalizedAt, timestamp)
                    .SetProperty(s => s.SubmittedAt,
                        status == ExamSessionStatus.Submitted
                            ? timestamp
                            : (DateTime?)null)
                    .SetProperty(s => s.UpdatedAt, timestamp),
                cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker
            .Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                    break;
            }
        }
    }

    private sealed class ApplicationDbTransaction : IApplicationDbTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public ApplicationDbTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return _transaction.CommitAsync(cancellationToken);
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return _transaction.RollbackAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _transaction.DisposeAsync();
        }
    }
}
