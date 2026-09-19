using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.PreparationPackages.ExamSessions.GetMyPackageExamSessionState;

public sealed class GetMyPackageExamSessionStateQueryHandler : IRequestHandler<GetMyPackageExamSessionStateQuery, PackageExamSessionStateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetMyPackageExamSessionStateQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PackageExamSessionStateDto> Handle(GetMyPackageExamSessionStateQuery request, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await _context.NurseProfiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (nurseProfileId is null)
        {
            throw new ForbiddenAccessException("Nurse profile is required before using package exam sessions.");
        }

        var entitlement = await _context.PackagePurchaseEntitlements
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.EntitlementId && item.NurseProfileId == nurseProfileId, cancellationToken)
            ?? throw new KeyNotFoundException("Package exam session was not found.");

        var sessionId = await _context.ExamSessionProvenances
            .AsNoTracking()
            .Where(provenance => provenance.PackagePurchaseEntitlementId == entitlement.Id
                && provenance.IncludedExamVersionId == entitlement.IncludedExamVersionId)
            .Select(provenance => (Guid?)provenance.ExamSessionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (sessionId is null)
        {
            return new PackageExamSessionStateDto { HasSession = false };
        }

        var session = await _context.ExamSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == sessionId.Value
                && item.NurseProfileId == nurseProfileId
                && item.ExamVersionId == entitlement.IncludedExamVersionId
                && item.Source == ExamSessionSource.PackageAttempt,
                cancellationToken);

        if (session is null)
        {
            return new PackageExamSessionStateDto { HasSession = false };
        }

        return new PackageExamSessionStateDto
        {
            HasSession = true,
            SessionId = session.Id,
            ExamId = session.ExamId,
            Status = session.Status.ToString(),
            ExpiresAt = session.ExpiresAt
        };
    }
}
