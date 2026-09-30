using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.PracticeProgress;

public sealed class GetPackagePracticeProgressQueryHandler : IRequestHandler<GetPackagePracticeProgressQuery, PackagePracticeProgressSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetPackagePracticeProgressQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PackagePracticeProgressSummaryDto> Handle(GetPackagePracticeProgressQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await GetCurrentNurseProfileIdAsync(cancellationToken);
        var entitlement = await _context.PackagePurchaseEntitlements
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.EntitlementId && item.NurseProfileId == nurseProfileId, cancellationToken)
            ?? throw new KeyNotFoundException("Package practice progress was not found.");

        var items = await _context.PracticeItems
            .AsNoTracking()
            .Where(item => item.PracticeCollectionVersionId == entitlement.PracticeCollectionVersionId)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

        var itemIds = items.Select(item => item.Id).ToList();
        var progressByItemId = await _context.PackagePracticeProgresses
            .AsNoTracking()
            .Where(progress => progress.NurseProfileId == nurseProfileId
                && progress.PackagePurchaseEntitlementId == entitlement.Id
                && progress.PracticeCollectionVersionId == entitlement.PracticeCollectionVersionId
                && itemIds.Contains(progress.PracticeItemId))
            .ToDictionaryAsync(progress => progress.PracticeItemId, cancellationToken);

        var itemStates = items
            .Select(item => MapItemState(item.Id, progressByItemId.GetValueOrDefault(item.Id)))
            .ToList();

        var answeredCount = itemStates.Count(item => item.State != PackagePracticeProgressItemState.Unanswered);
        var correctCount = itemStates.Count(item => item.State == PackagePracticeProgressItemState.AnsweredCorrect);
        var incorrectCount = itemStates.Count(item => item.State == PackagePracticeProgressItemState.AnsweredIncorrect);

        return new PackagePracticeProgressSummaryDto
        {
            PackagePurchaseEntitlementId = entitlement.Id,
            PracticeCollectionVersionId = entitlement.PracticeCollectionVersionId,
            TotalItems = items.Count,
            AnsweredCount = answeredCount,
            UnansweredCount = items.Count - answeredCount,
            CorrectCount = correctCount,
            IncorrectCount = incorrectCount,
            ItemStates = itemStates
        };
    }

    private async Task<Guid> GetCurrentNurseProfileIdAsync(CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await _context.NurseProfiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return nurseProfileId ?? throw new ForbiddenAccessException("Nurse profile is required before using package practice progress.");
    }

    private static PackagePracticeProgressItemStateDto MapItemState(Guid itemId, PackagePracticeProgress? progress)
    {
        if (progress is null)
        {
            return new PackagePracticeProgressItemStateDto
            {
                PracticeItemId = itemId,
                State = PackagePracticeProgressItemState.Unanswered
            };
        }

        return new PackagePracticeProgressItemStateDto
        {
            PracticeItemId = itemId,
            State = MapState(progress.State),
            SelectedPracticeAnswerOptionId = progress.SelectedPracticeAnswerOptionId,
            LastAnsweredAt = progress.LastAnsweredAt
        };
    }

    private static PackagePracticeProgressItemState MapState(PackagePracticeProgressState state)
    {
        return state switch
        {
            PackagePracticeProgressState.AnsweredCorrect => PackagePracticeProgressItemState.AnsweredCorrect,
            PackagePracticeProgressState.AnsweredIncorrect => PackagePracticeProgressItemState.AnsweredIncorrect,
            _ => throw new InvalidOperationException("Unsupported package practice progress state.")
        };
    }
}
