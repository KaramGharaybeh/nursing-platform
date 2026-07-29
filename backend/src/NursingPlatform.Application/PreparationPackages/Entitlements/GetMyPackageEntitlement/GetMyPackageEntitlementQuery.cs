using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.Entitlements.DTOs;
using NursingPlatform.Application.PreparationPackages.Entitlements.ListMyPackageEntitlements;

namespace NursingPlatform.Application.PreparationPackages.Entitlements.GetMyPackageEntitlement;

public class GetMyPackageEntitlementQuery : IRequest<PackageEntitlementDetailDto>
{
    public Guid Id { get; set; }
}

public class GetMyPackageEntitlementQueryValidator : AbstractValidator<GetMyPackageEntitlementQuery>
{
    public GetMyPackageEntitlementQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetMyPackageEntitlementQueryHandler : IRequestHandler<GetMyPackageEntitlementQuery, PackageEntitlementDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetMyPackageEntitlementQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PackageEntitlementDetailDto> Handle(GetMyPackageEntitlementQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await GetCurrentNurseProfileIdAsync(cancellationToken);
        var entitlement = await _context.PackagePurchaseEntitlements
            .AsNoTracking()
            .Include(e => e.Rights)
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.NurseProfileId == nurseProfileId, cancellationToken)
            ?? throw new KeyNotFoundException("Package entitlement was not found.");

        var snapshot = await _context.PackageOrderItemSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == entitlement.PurchasedOfferSnapshotId, cancellationToken);

        return PackageEntitlementMapping.ToDetail(entitlement, snapshot);
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
}
