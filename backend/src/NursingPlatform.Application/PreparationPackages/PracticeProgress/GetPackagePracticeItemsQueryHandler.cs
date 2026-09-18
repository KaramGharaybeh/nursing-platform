using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.Authorization;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.PracticeProgress;

public sealed class GetPackagePracticeItemsQueryHandler : IRequestHandler<GetPackagePracticeItemsQuery, PackagePracticeContentListDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;
    private readonly PackageBenefitAuthorizationService _authorizationService;

    public GetPackagePracticeItemsQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
        _authorizationService = new PackageBenefitAuthorizationService(context, nurseRoleGuard);
    }

    public async Task<PackagePracticeContentListDto> Handle(GetPackagePracticeItemsQuery request, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await _context.NurseProfiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (nurseProfileId is null)
        {
            throw new ForbiddenAccessException("Nurse profile is required before using package practice items.");
        }

        var entitlement = await _context.PackagePurchaseEntitlements
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.EntitlementId && item.NurseProfileId == nurseProfileId, cancellationToken)
            ?? throw new KeyNotFoundException("Package practice items were not found.");

        var authorization = await _authorizationService.AuthorizeAsync(
            request.EntitlementId,
            PackageBenefitRightType.PracticeAccess,
            DateTime.UtcNow,
            cancellationToken);

        if (!authorization.IsAuthorized)
        {
            throw new InvalidOperationException("Package practice access is not available.");
        }

        var items = await _context.PracticeItems
            .AsNoTracking()
            .Where(item => item.PracticeCollectionVersionId == entitlement.PracticeCollectionVersionId)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Id)
            .Include(item => item.AnswerOptions)
            .ToListAsync(cancellationToken);

        return new PackagePracticeContentListDto
        {
            PackagePurchaseEntitlementId = entitlement.Id,
            PracticeCollectionVersionId = entitlement.PracticeCollectionVersionId,
            TotalItems = items.Count,
            Items = items.Select(item => new PackagePracticeItemContentDto
            {
                PracticeItemId = item.Id,
                DisplayOrder = item.DisplayOrder,
                Prompt = item.Prompt,
                AnswerOptions = item.AnswerOptions
                    .OrderBy(option => option.DisplayOrder)
                    .ThenBy(option => option.Id)
                    .Select(option => new PackagePracticeAnswerOptionContentDto
                    {
                        PracticeAnswerOptionId = option.Id,
                        OptionText = option.OptionText,
                        DisplayOrder = option.DisplayOrder
                    })
                    .ToList()
            }).ToList()
        };
    }
}
