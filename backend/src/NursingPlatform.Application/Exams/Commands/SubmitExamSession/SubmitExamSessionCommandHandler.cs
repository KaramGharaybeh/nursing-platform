using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Commands.SubmitExamSession;

public class SubmitExamSessionCommandHandler : IRequestHandler<SubmitExamSessionCommand, ExamSessionResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public SubmitExamSessionCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<ExamSessionResultDto> Handle(SubmitExamSessionCommand request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var bundle = await ExamHandlerHelpers.GetOwnedSessionBundleAsync(_context, nurseProfileId, request.ExamSessionId, cancellationToken);
        var exam = await _context.Exams.FirstAsync(e => e.Id == bundle.Session.ExamId, cancellationToken);
        var now = DateTime.UtcNow;

        if (bundle.Session.Status != ExamSessionStatus.InProgress)
        {
            throw new InvalidOperationException("Only in-progress exam sessions can be submitted.");
        }

        var status = now >= bundle.Session.ExpiresAt
            ? ExamSessionStatus.Expired
            : ExamSessionStatus.Submitted;

        await ExamHandlerHelpers.FinalizeAsync(_context, bundle, status, exam.PassingScorePercentage, now, cancellationToken);

        return ExamMapping.ToResultDto(bundle.Session, bundle.ExamTitle);
    }
}
