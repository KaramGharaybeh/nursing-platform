using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.Reports.DTOs;
using NursingPlatform.Application.PreparationPackages.Reports.Generation;
using NursingPlatform.Application.PreparationPackages.Reports.Mapping;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Reports.GetPackageAnalyticalReport;

public sealed class GetPackageAnalyticalReportQueryHandler : IRequestHandler<GetPackageAnalyticalReportQuery, PackageAnalyticalReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetPackageAnalyticalReportQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PackageAnalyticalReportDto> Handle(GetPackageAnalyticalReportQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);

        var existing = await LoadExistingReportAsync(request.SessionId, nurseProfileId, cancellationToken);
        if (existing is not null)
        {
            return PackageAnalyticalReportMapping.ToDto(existing);
        }

        var session = await _context.ExamSessions
            .FirstOrDefaultAsync(item => item.Id == request.SessionId && item.NurseProfileId == nurseProfileId, cancellationToken);

        if (session is null)
        {
            throw new KeyNotFoundException("Package analytical report was not found.");
        }

        ValidateSessionQualification(session);

        var provenance = await LoadValidatedProvenanceAsync(session, cancellationToken);
        var entitlement = await LoadValidatedEntitlementAsync(session, provenance, nurseProfileId, cancellationToken);

        var now = DateTime.UtcNow;
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            var created = await PackageAnalyticalReportGenerator.GenerateAsync(
                _context,
                session,
                provenance,
                entitlement,
                now,
                cancellationToken);

            _context.PackageAnalyticalReports.Add(created);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return PackageAnalyticalReportMapping.ToDto(created);
        }
        catch (DbUpdateException exception) when (_context.IsUniquePackageAnalyticalReportSessionViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            var raceWinner = await LoadExistingReportAsync(request.SessionId, nurseProfileId, cancellationToken);
            if (raceWinner is not null)
            {
                return PackageAnalyticalReportMapping.ToDto(raceWinner);
            }

            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<PackageAnalyticalReport?> LoadExistingReportAsync(
        Guid sessionId,
        Guid nurseProfileId,
        CancellationToken cancellationToken)
    {
        return await _context.PackageAnalyticalReports
            .Include(report => report.TopicResults)
            .Include(report => report.GuidanceItems)
            .FirstOrDefaultAsync(report => report.ExamSessionId == sessionId && report.NurseProfileId == nurseProfileId, cancellationToken);
    }

    private static void ValidateSessionQualification(ExamSession session)
    {
        if (session.Source != ExamSessionSource.PackageAttempt)
        {
            throw Conflict("package-report-session-not-qualified", "Exam session does not qualify for a package analytical report.");
        }

        if (session.Status == ExamSessionStatus.InProgress)
        {
            throw Conflict("package-report-session-not-finalized", "Package exam session has not been finalized.");
        }

        if (session.Status is not ExamSessionStatus.Submitted and not ExamSessionStatus.Expired)
        {
            throw Conflict("package-report-session-not-qualified", "Exam session does not qualify for a package analytical report.");
        }

        if (session.FinalizedAt is null)
        {
            throw Conflict("package-report-session-not-finalized", "Package exam session has not been finalized.");
        }
    }

    private async Task<ExamSessionProvenance> LoadValidatedProvenanceAsync(ExamSession session, CancellationToken cancellationToken)
    {
        var provenances = await _context.ExamSessionProvenances
            .Where(provenance => provenance.ExamSessionId == session.Id)
            .ToListAsync(cancellationToken);

        if (provenances.Count != 1)
        {
            throw Conflict("package-report-provenance-invalid", "Package exam session provenance is invalid.");
        }

        var provenance = provenances[0];
        if (provenance.ExamSessionId != session.Id
            || provenance.IncludedExamId != session.ExamId
            || provenance.IncludedExamVersionId != session.ExamVersionId)
        {
            throw Conflict("package-report-provenance-invalid", "Package exam session provenance is invalid.");
        }

        return provenance;
    }

    private async Task<PackagePurchaseEntitlement> LoadValidatedEntitlementAsync(
        ExamSession session,
        ExamSessionProvenance provenance,
        Guid nurseProfileId,
        CancellationToken cancellationToken)
    {
        var entitlement = await _context.PackagePurchaseEntitlements
            .Include(item => item.Rights)
            .FirstOrDefaultAsync(item => item.Id == provenance.PackagePurchaseEntitlementId && item.NurseProfileId == nurseProfileId, cancellationToken);

        if (entitlement is null)
        {
            throw Conflict("package-report-right-missing", "Package report eligibility right is missing.");
        }

        var hasMismatchedFacts = entitlement.IncludedExamId != session.ExamId
            || entitlement.IncludedExamVersionId != session.ExamVersionId
            || entitlement.ReportingProfilePublicationId != provenance.ReportingProfilePublicationId
            || entitlement.PracticeCollectionVersionId != provenance.PracticeCollectionVersionId
            || entitlement.PreparationPackageDefinitionId != provenance.PreparationPackageDefinitionId
            || entitlement.PreparationPackageVersionId != provenance.PreparationPackageVersionId
            || entitlement.PreparationPackageOfferId != provenance.PreparationPackageOfferId;

        if (hasMismatchedFacts)
        {
            throw Conflict("package-report-provenance-invalid", "Package exam session provenance is invalid.");
        }

        var reportRight = entitlement.Rights.SingleOrDefault(right => right.RightType == PackageBenefitRightType.ReportEligibility);
        if (reportRight is null || reportRight.Status is PackageBenefitRightStatus.Revoked)
        {
            throw Conflict("package-report-right-missing", "Package report eligibility right is missing.");
        }

        return entitlement;
    }

    private static PackageReportConflictException Conflict(string code, string message)
    {
        return new PackageReportConflictException(code, message);
    }
}
