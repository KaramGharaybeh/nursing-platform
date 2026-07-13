using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Country> Countries { get; }
    DbSet<Language> Languages { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<NurseProfile> NurseProfiles { get; }
    DbSet<NurseExperience> NurseExperiences { get; }
    DbSet<NurseEducation> NurseEducation { get; }
    DbSet<NurseCertificate> NurseCertificates { get; }
    DbSet<NurseLanguage> NurseLanguages { get; }
    DbSet<NurseSkill> NurseSkills { get; }
    DbSet<NurseCvDocument> NurseCvDocuments { get; }
    DbSet<EmployerProfile> EmployerProfiles { get; }
    DbSet<EmployerOrganization> EmployerOrganizations { get; }
    DbSet<ContactRequest> ContactRequests { get; }
    DbSet<ExamCategory> ExamCategories { get; }
    DbSet<Exam> Exams { get; }
    DbSet<ExamVersion> ExamVersions { get; }
    DbSet<ExamQuestion> ExamQuestions { get; }
    DbSet<ExamAnswerOption> ExamAnswerOptions { get; }
    DbSet<ExamAccessGrant> ExamAccessGrants { get; }
    DbSet<ExamSession> ExamSessions { get; }
    DbSet<ExamSessionQuestion> ExamSessionQuestions { get; }
    DbSet<ExamSessionAnswerOption> ExamSessionAnswerOptions { get; }
    DbSet<ExamSessionAnswer> ExamSessionAnswers { get; }
    DbSet<PaymentProduct> PaymentProducts { get; }
    DbSet<PaymentOrder> PaymentOrders { get; }
    DbSet<PaymentOrderItem> PaymentOrderItems { get; }
    DbSet<PaymentCheckoutSession> PaymentCheckoutSessions { get; }
    Task<IApplicationDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> AcquirePaymentCheckoutProviderLeaseAsync(
        Guid checkoutSessionId,
        Guid leaseId,
        DateTime leaseExpiresAt,
        DateTime timestamp,
        CancellationToken cancellationToken = default);
    Task<int> ExecutePaymentOrderPaidTransitionAsync(
        Guid orderId,
        Guid nurseProfileId,
        DateTime paidAt,
        CancellationToken cancellationToken = default);
    bool IsUniqueEffectiveExamAccessGrantViolation(DbUpdateException exception);
    Task<int> ExecuteContactRequestTransitionAsync(
        Guid id,
        Guid ownerProfileId,
        bool isEmployerOwner,
        ContactRequestStatus status,
        DateTime timestamp,
        CancellationToken cancellationToken = default);
    Task<int> ExecuteExamSessionFinalizationAsync(
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
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IApplicationDbTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
