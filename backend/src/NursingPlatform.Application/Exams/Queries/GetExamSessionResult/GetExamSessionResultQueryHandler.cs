using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Queries.GetExamSessionResult;

public class GetExamSessionResultQueryHandler : IRequestHandler<GetExamSessionResultQuery, ExamSessionResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetExamSessionResultQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<ExamSessionResultDto> Handle(GetExamSessionResultQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var bundle = await ExamHandlerHelpers.GetOwnedSessionBundleAsync(_context, nurseProfileId, request.ExamSessionId, cancellationToken);
        var exam = await _context.Exams.FirstAsync(e => e.Id == bundle.Session.ExamId, cancellationToken);
        var now = DateTime.UtcNow;

        await ExamHandlerHelpers.FinalizeIfExpiredAsync(_context, bundle, exam.PassingScorePercentage, now, cancellationToken);

        if (bundle.Session.Status == ExamSessionStatus.InProgress)
        {
            throw new InvalidOperationException("Exam session result is available only after completion.");
        }

        return ExamMapping.ToResultDto(bundle.Session, bundle.ExamTitle);
    }
}
