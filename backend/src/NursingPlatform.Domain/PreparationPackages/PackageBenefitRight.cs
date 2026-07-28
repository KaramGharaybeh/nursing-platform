using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PackageBenefitRight : AuditableEntity
{
    private PackageBenefitRight()
    {
    }

    public Guid Id { get; private set; }
    public Guid PackagePurchaseEntitlementId { get; private set; }
    public PackageBenefitRightType RightType { get; private set; }
    public PackageBenefitRightStatus Status { get; private set; }
    public DateTime AccessStartsAt { get; private set; }
    public DateTime? AccessEndsAt { get; private set; }

    public static PackageBenefitRight Create(
        Guid packagePurchaseEntitlementId,
        PackageBenefitRightType rightType,
        PackageBenefitRightStatus status,
        DateTime accessStartsAt,
        DateTime? accessEndsAt)
    {
        if (packagePurchaseEntitlementId == Guid.Empty)
        {
            throw new InvalidOperationException("Package purchase entitlement id is required.");
        }

        if (accessEndsAt.HasValue && accessEndsAt.Value <= accessStartsAt)
        {
            throw new InvalidOperationException("Benefit right access end must be after access start.");
        }

        return new PackageBenefitRight
        {
            Id = Guid.NewGuid(),
            PackagePurchaseEntitlementId = packagePurchaseEntitlementId,
            RightType = rightType,
            Status = status,
            AccessStartsAt = accessStartsAt,
            AccessEndsAt = accessEndsAt
        };
    }

    public static IReadOnlyCollection<PackageBenefitRight> CreateDefaultSet(
        Guid packagePurchaseEntitlementId,
        DateTime accessStartsAt,
        DateTime accessEndsAt)
    {
        return
        [
            Create(packagePurchaseEntitlementId, PackageBenefitRightType.MaterialsAccess, PackageBenefitRightStatus.Available, accessStartsAt, accessEndsAt),
            Create(packagePurchaseEntitlementId, PackageBenefitRightType.PracticeAccess, PackageBenefitRightStatus.Available, accessStartsAt, accessEndsAt),
            Create(packagePurchaseEntitlementId, PackageBenefitRightType.PackageExamAttemptEligibility, PackageBenefitRightStatus.Available, accessStartsAt, accessEndsAt),
            Create(packagePurchaseEntitlementId, PackageBenefitRightType.ReportEligibility, PackageBenefitRightStatus.Dormant, accessStartsAt, null)
        ];
    }

    public void Expire(DateTime timestamp)
    {
        if (Status is PackageBenefitRightStatus.Consumed or PackageBenefitRightStatus.Revoked)
        {
            return;
        }

        Status = PackageBenefitRightStatus.Expired;
        UpdatedAt = timestamp;
    }
}
