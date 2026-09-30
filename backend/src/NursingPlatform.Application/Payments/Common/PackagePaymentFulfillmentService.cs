using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.Payments.Common;

public sealed class PackagePaymentFulfillmentService
{
    private readonly IApplicationDbContext _context;

    public PackagePaymentFulfillmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task FulfillPackageOrderItemsAsync(
        PaymentOrder order,
        Guid nurseProfileId,
        DateTime fulfilledAt,
        CancellationToken cancellationToken)
    {
        var packageItems = order.Items
            .Where(i => i.SourceType == PaymentOrderItemSourceType.PreparationPackageOffer)
            .OrderBy(i => i.Id)
            .ToArray();

        foreach (var item in packageItems)
        {
            ValidatePackageOrderItemSnapshot(order, item);
            var snapshot = item.PackageOrderItemSnapshot!;

            var existingEntitlement = await _context.PackagePurchaseEntitlements
                .Include(e => e.Rights)
                .FirstOrDefaultAsync(e => e.PaymentOrderItemId == item.Id, cancellationToken);

            if (existingEntitlement is not null)
            {
                ValidateExistingEntitlement(order, item, snapshot, existingEntitlement, nurseProfileId, fulfilledAt);
                continue;
            }

            await ExpireStaleSamePackageEntitlementsAsync(nurseProfileId, snapshot.PackageDefinitionId, fulfilledAt, cancellationToken);
            await EnsureNoActiveSamePackageEntitlementAsync(nurseProfileId, snapshot.PackageDefinitionId, fulfilledAt, cancellationToken);

            var entitlement = PackagePurchaseEntitlement.CreateFromSnapshot(
                nurseProfileId,
                order.Id,
                item.Id,
                snapshot,
                fulfilledAt);

            _context.PackagePurchaseEntitlements.Add(entitlement);
        }
    }

    private async Task ExpireStaleSamePackageEntitlementsAsync(
        Guid nurseProfileId,
        Guid packageDefinitionId,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        var staleEntitlements = await _context.PackagePurchaseEntitlements
            .Include(e => e.Rights)
            .Where(e => e.NurseProfileId == nurseProfileId
                && e.PreparationPackageDefinitionId == packageDefinitionId
                && e.Status == PackagePurchaseEntitlementStatus.Active
                && e.AccessEndsAt <= timestamp)
            .ToListAsync(cancellationToken);

        foreach (var entitlement in staleEntitlements)
        {
            entitlement.ExpireIfPastAccessWindow(timestamp);
        }
    }

    private async Task EnsureNoActiveSamePackageEntitlementAsync(
        Guid nurseProfileId,
        Guid packageDefinitionId,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        var activeSamePackageExists = await _context.PackagePurchaseEntitlements
            .AnyAsync(e => e.NurseProfileId == nurseProfileId
                && e.PreparationPackageDefinitionId == packageDefinitionId
                && e.Status == PackagePurchaseEntitlementStatus.Active
                && e.AccessStartsAt <= timestamp
                && timestamp < e.AccessEndsAt,
                cancellationToken);

        if (activeSamePackageExists)
        {
            throw new InvalidOperationException("An active preparation package entitlement already exists for this package.");
        }
    }

    private static void ValidatePackageOrderItemSnapshot(PaymentOrder order, PaymentOrderItem item)
    {
        var snapshot = item.PackageOrderItemSnapshot
            ?? throw new InvalidOperationException("Package order item snapshot is required for package fulfillment.");

        if (item.OrderId != order.Id
            || snapshot.PaymentOrderItemId != item.Id
            || item.SourceId != snapshot.PackageOfferId
            || item.Quantity != 1
            || item.Currency != snapshot.Currency
            || item.UnitAmountMinor != snapshot.PriceAmountMinor
            || item.LineTotalAmountMinor != snapshot.PriceAmountMinor)
        {
            throw new InvalidOperationException("Package order item snapshot does not match the paid order item.");
        }
    }

    private static void ValidateExistingEntitlement(
        PaymentOrder order,
        PaymentOrderItem item,
        PackageOrderItemSnapshot snapshot,
        PackagePurchaseEntitlement entitlement,
        Guid nurseProfileId,
        DateTime fulfilledAt)
    {
        if (entitlement.NurseProfileId != nurseProfileId
            || entitlement.PaymentOrderId != order.Id
            || entitlement.PaymentOrderItemId != item.Id
            || entitlement.PurchasedOfferSnapshotId != snapshot.Id
            || entitlement.PreparationPackageDefinitionId != snapshot.PackageDefinitionId
            || entitlement.PreparationPackageVersionId != snapshot.PackageVersionId
            || entitlement.PreparationPackageOfferId != snapshot.PackageOfferId
            || entitlement.IncludedExamId != snapshot.IncludedExamId
            || entitlement.IncludedExamVersionId != snapshot.IncludedExamVersionId
            || entitlement.ReportingProfilePublicationId != snapshot.ReportingProfilePublicationId
            || entitlement.PracticeCollectionVersionId != snapshot.PracticeCollectionVersionId
            || entitlement.PriceAmountMinor != snapshot.PriceAmountMinor
            || entitlement.Currency != snapshot.Currency
            || entitlement.AccessDurationDays != snapshot.AccessDurationDays
            || entitlement.AccessStartsAt != fulfilledAt
            || entitlement.AccessEndsAt != fulfilledAt.AddDays(snapshot.AccessDurationDays)
            || !entitlement.StudyMaterialVersionIds.SequenceEqual(snapshot.StudyMaterialVersionIds)
            || !HasExpectedBenefitRights(entitlement))
        {
            throw new InvalidOperationException("Existing package entitlement does not match the paid order item snapshot.");
        }
    }

    private static bool HasExpectedBenefitRights(PackagePurchaseEntitlement entitlement)
    {
        var rights = entitlement.Rights.ToArray();
        return rights.Length == 4
            && rights.Any(r => r.RightType == PackageBenefitRightType.MaterialsAccess
                && r.Status == PackageBenefitRightStatus.Available
                && r.AccessStartsAt == entitlement.AccessStartsAt
                && r.AccessEndsAt == entitlement.AccessEndsAt)
            && rights.Any(r => r.RightType == PackageBenefitRightType.PracticeAccess
                && r.Status == PackageBenefitRightStatus.Available
                && r.AccessStartsAt == entitlement.AccessStartsAt
                && r.AccessEndsAt == entitlement.AccessEndsAt)
            && rights.Any(r => r.RightType == PackageBenefitRightType.PackageExamAttemptEligibility
                && r.Status == PackageBenefitRightStatus.Available
                && r.AccessStartsAt == entitlement.AccessStartsAt
                && r.AccessEndsAt == entitlement.AccessEndsAt)
            && rights.Any(r => r.RightType == PackageBenefitRightType.ReportEligibility
                && r.Status == PackageBenefitRightStatus.Dormant
                && r.AccessStartsAt == entitlement.AccessStartsAt
                && r.AccessEndsAt is null);
    }
}
