using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Queries.GetExam;

public class GetExamQueryHandler : IRequestHandler<GetExamQuery, ExamDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetExamQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<ExamDetailDto> Handle(GetExamQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var now = DateTime.UtcNow;

        var row = await _context.Exams
            .Where(e => e.Id == request.ExamId && e.Status == ExamStatus.Published)
            .Join(_context.Countries, e => e.CountryId, c => c.Id, (exam, country) => new { exam, country })
            .GroupJoin(_context.ExamCategories, ec => ec.exam.ExamCategoryId, category => category.Id,
                (ec, categories) => new { ec.exam, ec.country, category = categories.FirstOrDefault() })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        var version = await _context.ExamVersions
            .Where(v => v.ExamId == row.exam.Id && v.Status == ExamVersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (version is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        var hasGrant = await _context.ExamAccessGrants
            .AnyAsync(g => g.NurseProfileId == nurseProfileId
                && g.ExamId == row.exam.Id
                && (g.ExpiresAt == null || g.ExpiresAt > now),
                cancellationToken);

        return ExamMapping.ToDetail(row.exam, version, row.country.Name, row.category?.Name, row.exam.IsFree || hasGrant);
    }
}
