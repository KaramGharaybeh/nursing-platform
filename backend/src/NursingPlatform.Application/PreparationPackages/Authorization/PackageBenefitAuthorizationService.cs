using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Authorization;

public sealed class PackageBenefitAuthorizationService
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public PackageBenefitAuthorizationService(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PackageBenefitAuthorizationResult> AuthorizeAsync(
        Guid packagePurchaseEntitlementId,
        PackageBenefitRightType rightType,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var nurseProfileId = await GetCurrentNurseProfileIdAsync(cancellationToken);
        var entitlement = await _context.PackagePurchaseEntitlements
            .AsNoTracking()
            .Include(e => e.Rights)
            .FirstOrDefaultAsync(e => e.Id == packagePurchaseEntitlementId && e.NurseProfileId == nurseProfileId, cancellationToken);

        if (entitlement is null)
        {
            return Denied(rightType, "NotFound");
        }

        var right = entitlement.Rights.SingleOrDefault(r => r.RightType == rightType);
        if (right is null)
        {
            return Denied(rightType, "RightMissing", entitlement);
        }

        if (!entitlement.IsActiveAt(timestamp))
        {
            return Denied(rightType, "EntitlementInactiveOrOutsideAccessWindow", entitlement, right);
        }

        if (rightType == PackageBenefitRightType.ReportEligibility && right.Status == PackageBenefitRightStatus.Dormant)
        {
            return Denied(rightType, "ReportRightDormant", entitlement, right, isVisibleDormantRight: true);
        }

        if (right.Status != PackageBenefitRightStatus.Available)
        {
            return Denied(rightType, "RightNotAvailable", entitlement, right);
        }

        if (right.AccessStartsAt > timestamp || (right.AccessEndsAt.HasValue && timestamp >= right.AccessEndsAt.Value))
        {
            return Denied(rightType, "RightOutsideAccessWindow", entitlement, right);
        }

        return new PackageBenefitAuthorizationResult
        {
            IsAuthorized = true,
            PackagePurchaseEntitlementId = entitlement.Id,
            RequestedRightType = rightType,
            EntitlementStatus = entitlement.Status.ToString(),
            RightStatus = right.Status.ToString()
        };
    }

    private async Task<Guid> GetCurrentNurseProfileIdAsync(CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await _context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return nurseProfileId ?? throw new ForbiddenAccessException("Nurse profile is required before using package benefits.");
    }

    private static PackageBenefitAuthorizationResult Denied(
        PackageBenefitRightType rightType,
        string reason,
        PackagePurchaseEntitlement? entitlement = null,
        PackageBenefitRight? right = null,
        bool isVisibleDormantRight = false)
    {
        return new PackageBenefitAuthorizationResult
        {
            IsAuthorized = false,
            IsVisibleDormantRight = isVisibleDormantRight,
            PackagePurchaseEntitlementId = entitlement?.Id,
            RequestedRightType = rightType,
            EntitlementStatus = entitlement?.Status.ToString() ?? string.Empty,
            RightStatus = right?.Status.ToString() ?? string.Empty,
            DenialReason = reason
        };
    }
}
