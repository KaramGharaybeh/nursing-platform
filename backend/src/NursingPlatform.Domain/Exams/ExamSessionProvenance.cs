using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Exams;

public class ExamSessionProvenance : AuditableEntity
{
    private ExamSessionProvenance()
    {
    }

    public Guid Id { get; private set; }
    public Guid ExamSessionId { get; private set; }
    public Guid PackagePurchaseEntitlementId { get; private set; }
    public Guid PackageBenefitRightId { get; private set; }
    public Guid PackageOrderItemSnapshotId { get; private set; }
    public Guid PaymentOrderId { get; private set; }
    public Guid PaymentOrderItemId { get; private set; }
    public Guid PreparationPackageDefinitionId { get; private set; }
    public Guid PreparationPackageVersionId { get; private set; }
    public Guid PreparationPackageOfferId { get; private set; }
    public Guid IncludedExamId { get; private set; }
    public Guid IncludedExamVersionId { get; private set; }
    public Guid ReportingProfilePublicationId { get; private set; }
    public Guid PracticeCollectionVersionId { get; private set; }
    public DateTime PackageAccessStartsAt { get; private set; }
    public DateTime PackageAccessEndsAt { get; private set; }
    public DateTime StartedAt { get; private set; }
    public ExamSession ExamSession { get; private set; } = null!;

    public static ExamSessionProvenance CreateForPackageAttempt(
        Guid examSessionId,
        Guid packagePurchaseEntitlementId,
        Guid packageBenefitRightId,
        Guid packageOrderItemSnapshotId,
        Guid paymentOrderId,
        Guid paymentOrderItemId,
        Guid preparationPackageDefinitionId,
        Guid preparationPackageVersionId,
        Guid preparationPackageOfferId,
        Guid includedExamId,
        Guid includedExamVersionId,
        Guid reportingProfilePublicationId,
        Guid practiceCollectionVersionId,
        DateTime packageAccessStartsAt,
        DateTime packageAccessEndsAt,
        DateTime startedAt)
    {
        RequireNotEmpty(examSessionId, nameof(examSessionId));
        RequireNotEmpty(packagePurchaseEntitlementId, nameof(packagePurchaseEntitlementId));
        RequireNotEmpty(packageBenefitRightId, nameof(packageBenefitRightId));
        RequireNotEmpty(packageOrderItemSnapshotId, nameof(packageOrderItemSnapshotId));
        RequireNotEmpty(paymentOrderId, nameof(paymentOrderId));
        RequireNotEmpty(paymentOrderItemId, nameof(paymentOrderItemId));
        RequireNotEmpty(preparationPackageDefinitionId, nameof(preparationPackageDefinitionId));
        RequireNotEmpty(preparationPackageVersionId, nameof(preparationPackageVersionId));
        RequireNotEmpty(preparationPackageOfferId, nameof(preparationPackageOfferId));
        RequireNotEmpty(includedExamId, nameof(includedExamId));
        RequireNotEmpty(includedExamVersionId, nameof(includedExamVersionId));
        RequireNotEmpty(reportingProfilePublicationId, nameof(reportingProfilePublicationId));
        RequireNotEmpty(practiceCollectionVersionId, nameof(practiceCollectionVersionId));

        if (packageAccessEndsAt <= packageAccessStartsAt)
        {
            throw new InvalidOperationException("Package provenance access end must be after access start.");
        }

        return new ExamSessionProvenance
        {
            Id = Guid.NewGuid(),
            ExamSessionId = examSessionId,
            PackagePurchaseEntitlementId = packagePurchaseEntitlementId,
            PackageBenefitRightId = packageBenefitRightId,
            PackageOrderItemSnapshotId = packageOrderItemSnapshotId,
            PaymentOrderId = paymentOrderId,
            PaymentOrderItemId = paymentOrderItemId,
            PreparationPackageDefinitionId = preparationPackageDefinitionId,
            PreparationPackageVersionId = preparationPackageVersionId,
            PreparationPackageOfferId = preparationPackageOfferId,
            IncludedExamId = includedExamId,
            IncludedExamVersionId = includedExamVersionId,
            ReportingProfilePublicationId = reportingProfilePublicationId,
            PracticeCollectionVersionId = practiceCollectionVersionId,
            PackageAccessStartsAt = packageAccessStartsAt,
            PackageAccessEndsAt = packageAccessEndsAt,
            StartedAt = startedAt,
            CreatedAt = startedAt,
            UpdatedAt = startedAt
        };
    }

    private static void RequireNotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{name} is required.");
        }
    }
}
