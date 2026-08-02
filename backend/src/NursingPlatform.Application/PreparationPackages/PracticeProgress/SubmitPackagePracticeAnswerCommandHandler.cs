using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.PreparationPackages.Authorization;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.PracticeProgress;

public sealed class SubmitPackagePracticeAnswerCommandHandler : IRequestHandler<SubmitPackagePracticeAnswerCommand, PackagePracticeAnswerSubmissionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly PackageBenefitAuthorizationService _authorizationService;

    public SubmitPackagePracticeAnswerCommandHandler(IApplicationDbContext context, NursingPlatform.Application.Nurses.Common.NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _authorizationService = new PackageBenefitAuthorizationService(context, nurseRoleGuard);
    }

    public async Task<PackagePracticeAnswerSubmissionDto> Handle(SubmitPackagePracticeAnswerCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var authorization = await _authorizationService.AuthorizeAsync(
            request.EntitlementId,
            PackageBenefitRightType.PracticeAccess,
            now,
            cancellationToken);

        if (!authorization.IsAuthorized)
        {
            throw new InvalidOperationException("Package practice access is not available.");
        }

        var entitlement = await _context.PackagePurchaseEntitlements
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.EntitlementId, cancellationToken)
            ?? throw new KeyNotFoundException("Package practice progress was not found.");

        var itemExists = await _context.PracticeItems
            .AsNoTracking()
            .AnyAsync(item => item.Id == request.PracticeItemId
                && item.PracticeCollectionVersionId == entitlement.PracticeCollectionVersionId, cancellationToken);
        if (!itemExists)
        {
            throw new KeyNotFoundException("Practice item was not found for the package practice collection.");
        }

        var selectedOption = await _context.PracticeAnswerOptions
            .AsNoTracking()
            .FirstOrDefaultAsync(option => option.Id == request.Request.SelectedPracticeAnswerOptionId
                && option.PracticeItemId == request.PracticeItemId, cancellationToken)
            ?? throw new KeyNotFoundException("Practice answer option was not found for the practice item.");

        var nurseProfileId = await _context.PackagePurchaseEntitlements
            .AsNoTracking()
            .Where(item => item.Id == request.EntitlementId)
            .Select(item => item.NurseProfileId)
            .FirstAsync(cancellationToken);

        var existing = await _context.PackagePracticeProgresses
            .FirstOrDefaultAsync(progress => progress.NurseProfileId == nurseProfileId
                && progress.PackagePurchaseEntitlementId == entitlement.Id
                && progress.PracticeCollectionVersionId == entitlement.PracticeCollectionVersionId
                && progress.PracticeItemId == request.PracticeItemId, cancellationToken);

        if (existing is null)
        {
            existing = PackagePracticeProgress.Create(
                nurseProfileId,
                entitlement.Id,
                entitlement.PracticeCollectionVersionId,
                request.PracticeItemId,
                selectedOption.Id,
                selectedOption.IsCorrect,
                now);
            _context.PackagePracticeProgresses.Add(existing);
        }
        else
        {
            existing.UpdateAnswer(selectedOption.Id, selectedOption.IsCorrect, now);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PackagePracticeAnswerSubmissionDto
        {
            PracticeItemId = existing.PracticeItemId,
            State = selectedOption.IsCorrect
                ? PackagePracticeProgressItemState.AnsweredCorrect
                : PackagePracticeProgressItemState.AnsweredIncorrect,
            SelectedPracticeAnswerOptionId = existing.SelectedPracticeAnswerOptionId,
            LastAnsweredAt = existing.LastAnsweredAt
        };
    }
}
