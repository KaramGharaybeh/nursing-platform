using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PackagePurchaseEntitlement : AuditableEntity
{
    private readonly List<Guid> _studyMaterialVersionIds = [];
    private readonly List<PackageBenefitRight> _rights = [];

    private PackagePurchaseEntitlement()
    {
    }

    public Guid Id { get; private set; }
    public Guid NurseProfileId { get; private set; }
    public Guid PaymentOrderId { get; private set; }
    public Guid PaymentOrderItemId { get; private set; }
    public Guid PurchasedOfferSnapshotId { get; private set; }
    public Guid PreparationPackageDefinitionId { get; private set; }
    public Guid PreparationPackageVersionId { get; private set; }
    public Guid PreparationPackageOfferId { get; private set; }
    public Guid IncludedExamId { get; private set; }
    public Guid IncludedExamVersionId { get; private set; }
    public Guid ReportingProfilePublicationId { get; private set; }
    public Guid PracticeCollectionVersionId { get; private set; }
    public IReadOnlyList<Guid> StudyMaterialVersionIds => _studyMaterialVersionIds.AsReadOnly();
    public long PriceAmountMinor { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int AccessDurationDays { get; private set; }
    public PackagePurchaseEntitlementStatus Status { get; private set; }
    public DateTime FulfilledAt { get; private set; }
    public DateTime AccessStartsAt { get; private set; }
    public DateTime AccessEndsAt { get; private set; }
    public IReadOnlyCollection<PackageBenefitRight> Rights => _rights.AsReadOnly();

    public static PackagePurchaseEntitlement CreateFromSnapshot(
        Guid nurseProfileId,
        Guid paymentOrderId,
        Guid paymentOrderItemId,
        PackageOrderItemSnapshot snapshot,
        DateTime fulfilledAt)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        RequireNotEmpty(nurseProfileId, nameof(nurseProfileId));
        RequireNotEmpty(paymentOrderId, nameof(paymentOrderId));
        RequireNotEmpty(paymentOrderItemId, nameof(paymentOrderItemId));

        if (snapshot.PaymentOrderItemId != Guid.Empty && snapshot.PaymentOrderItemId != paymentOrderItemId)
        {
            throw new InvalidOperationException("Package entitlement source order item must match the purchased snapshot.");
        }

        if (snapshot.AccessDurationDays <= 0)
        {
            throw new InvalidOperationException("Package access duration must be positive.");
        }

        if (fulfilledAt.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Package fulfillment timestamp must be UTC.");
        }

        var accessEndsAt = fulfilledAt.AddDays(snapshot.AccessDurationDays);
        if (accessEndsAt <= fulfilledAt)
        {
            throw new InvalidOperationException("Package access end must be after access start.");
        }

        var entitlement = new PackagePurchaseEntitlement
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            PaymentOrderId = paymentOrderId,
            PaymentOrderItemId = paymentOrderItemId,
            PurchasedOfferSnapshotId = snapshot.Id,
            PreparationPackageDefinitionId = snapshot.PackageDefinitionId,
            PreparationPackageVersionId = snapshot.PackageVersionId,
            PreparationPackageOfferId = snapshot.PackageOfferId,
            IncludedExamId = snapshot.IncludedExamId,
            IncludedExamVersionId = snapshot.IncludedExamVersionId,
            ReportingProfilePublicationId = snapshot.ReportingProfilePublicationId,
            PracticeCollectionVersionId = snapshot.PracticeCollectionVersionId,
            PriceAmountMinor = snapshot.PriceAmountMinor,
            Currency = snapshot.Currency,
            AccessDurationDays = snapshot.AccessDurationDays,
            Status = PackagePurchaseEntitlementStatus.Active,
            FulfilledAt = fulfilledAt,
            AccessStartsAt = fulfilledAt,
            AccessEndsAt = accessEndsAt,
            CreatedAt = fulfilledAt,
            UpdatedAt = fulfilledAt
        };

        entitlement._studyMaterialVersionIds.AddRange(snapshot.StudyMaterialVersionIds);

        foreach (var right in PackageBenefitRight.CreateDefaultSet(entitlement.Id, entitlement.AccessStartsAt, entitlement.AccessEndsAt))
        {
            entitlement.AddBenefitRight(right);
        }

        return entitlement;
    }

    public bool IsActiveAt(DateTime timestamp)
    {
        return Status == PackagePurchaseEntitlementStatus.Active
            && AccessStartsAt <= timestamp
            && timestamp < AccessEndsAt;
    }

    public bool ExpireIfPastAccessWindow(DateTime timestamp)
    {
        if (Status != PackagePurchaseEntitlementStatus.Active || AccessEndsAt > timestamp)
        {
            return false;
        }

        Status = PackagePurchaseEntitlementStatus.Expired;
        UpdatedAt = timestamp;

        foreach (var right in _rights)
        {
            if (right.RightType != PackageBenefitRightType.ReportEligibility)
            {
                right.Expire(timestamp);
            }
        }

        return true;
    }

    public void AddBenefitRight(PackageBenefitRight right)
    {
        ArgumentNullException.ThrowIfNull(right);

        if (right.PackagePurchaseEntitlementId != Id)
        {
            throw new InvalidOperationException("Benefit right must belong to the package purchase entitlement.");
        }

        if (_rights.Any(existing => existing.RightType == right.RightType))
        {
            throw new InvalidOperationException("Duplicate package benefit right type is not allowed.");
        }

        if (_rights.Count >= 4)
        {
            throw new InvalidOperationException("A package purchase entitlement cannot contain more than four Stage 2 benefit rights.");
        }

        _rights.Add(right);
    }

    private static void RequireNotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{name} is required.");
        }
    }
}
