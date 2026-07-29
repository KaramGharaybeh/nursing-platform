using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.Entitlements.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Entitlements.ListMyPackageEntitlements;

public class ListMyPackageEntitlementsQuery : IRequest<PaginatedResult<PackageEntitlementListItemDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ListMyPackageEntitlementsQueryValidator : AbstractValidator<ListMyPackageEntitlementsQuery>
{
    public ListMyPackageEntitlementsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class ListMyPackageEntitlementsQueryHandler : IRequestHandler<ListMyPackageEntitlementsQuery, PaginatedResult<PackageEntitlementListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListMyPackageEntitlementsQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaginatedResult<PackageEntitlementListItemDto>> Handle(ListMyPackageEntitlementsQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await GetCurrentNurseProfileIdAsync(cancellationToken);
        var entitlements = await _context.PackagePurchaseEntitlements
            .AsNoTracking()
            .Where(e => e.NurseProfileId == nurseProfileId)
            .OrderByDescending(e => e.AccessStartsAt)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        var snapshots = await LoadSnapshotsAsync(entitlements.Select(e => e.PurchasedOfferSnapshotId), cancellationToken);
        var items = entitlements.Select(e => PackageEntitlementMapping.ToListItem(e, snapshots.GetValueOrDefault(e.PurchasedOfferSnapshotId))).ToList();

        return new PaginatedResult<PackageEntitlementListItemDto>
        {
            Items = items.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = items.Count
        };
    }

    private async Task<Guid> GetCurrentNurseProfileIdAsync(CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await _context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return nurseProfileId ?? throw new ForbiddenAccessException("Nurse profile is required before using package entitlements.");
    }

    private async Task<Dictionary<Guid, PackageOrderItemSnapshot>> LoadSnapshotsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await _context.PackageOrderItemSnapshots
            .AsNoTracking()
            .Where(s => idList.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);
    }
}

internal static class PackageEntitlementMapping
{
    public static PackageEntitlementListItemDto ToListItem(PackagePurchaseEntitlement entitlement, PackageOrderItemSnapshot? snapshot)
    {
        return new PackageEntitlementListItemDto
        {
            Id = entitlement.Id,
            PackageOfferId = entitlement.PreparationPackageOfferId,
            PackageOfferTitle = snapshot?.PackageOfferTitle ?? string.Empty,
            PackageDefinitionId = entitlement.PreparationPackageDefinitionId,
            PackageDefinitionTitle = snapshot?.PackageDefinitionTitle ?? string.Empty,
            PackageVersionId = entitlement.PreparationPackageVersionId,
            IncludedExamId = entitlement.IncludedExamId,
            IncludedExamTitle = snapshot?.IncludedExamTitle ?? string.Empty,
            AccessStartsAt = entitlement.AccessStartsAt,
            AccessEndsAt = entitlement.AccessEndsAt,
            Status = entitlement.Status.ToString()
        };
    }

    public static PackageEntitlementDetailDto ToDetail(PackagePurchaseEntitlement entitlement, PackageOrderItemSnapshot? snapshot)
    {
        var listItem = ToListItem(entitlement, snapshot);
        return new PackageEntitlementDetailDto
        {
            Id = listItem.Id,
            PackageOfferId = listItem.PackageOfferId,
            PackageOfferTitle = listItem.PackageOfferTitle,
            PackageDefinitionId = listItem.PackageDefinitionId,
            PackageDefinitionTitle = listItem.PackageDefinitionTitle,
            PackageVersionId = listItem.PackageVersionId,
            IncludedExamId = listItem.IncludedExamId,
            IncludedExamTitle = listItem.IncludedExamTitle,
            AccessStartsAt = listItem.AccessStartsAt,
            AccessEndsAt = listItem.AccessEndsAt,
            Status = listItem.Status,
            PurchasedSnapshot = new PackageEntitlementSnapshotDto
            {
                PackageOfferSlug = snapshot?.PackageOfferSlug ?? string.Empty,
                PackageOfferSummary = snapshot?.PackageOfferSummary,
                PackageDefinitionSlug = snapshot?.PackageDefinitionSlug ?? string.Empty,
                CountryId = snapshot?.CountryId ?? Guid.Empty,
                ExamCategoryId = snapshot?.ExamCategoryId ?? Guid.Empty,
                IncludedExamVersionId = entitlement.IncludedExamVersionId,
                ReportingProfilePublicationId = entitlement.ReportingProfilePublicationId,
                PracticeCollectionVersionId = entitlement.PracticeCollectionVersionId,
                StudyMaterialVersionIds = entitlement.StudyMaterialVersionIds.ToList(),
                PriceAmountMinor = entitlement.PriceAmountMinor,
                Currency = entitlement.Currency,
                AccessDurationDays = entitlement.AccessDurationDays
            },
            BenefitRights = entitlement.Rights
                .OrderBy(r => r.RightType.ToString())
                .Select(r => new PackageBenefitRightSummaryDto
                {
                    RightType = r.RightType.ToString(),
                    Status = r.Status.ToString(),
                    AccessStartsAt = r.AccessStartsAt,
                    AccessEndsAt = r.AccessEndsAt
                })
                .ToList()
        };
    }
}
