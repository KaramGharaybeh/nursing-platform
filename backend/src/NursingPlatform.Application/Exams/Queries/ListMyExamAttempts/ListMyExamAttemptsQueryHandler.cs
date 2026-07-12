using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Exams.Queries.ListMyExamAttempts;

public class ListMyExamAttemptsQueryHandler : IRequestHandler<ListMyExamAttemptsQuery, PaginatedResult<ExamAttemptDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListMyExamAttemptsQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaginatedResult<ExamAttemptDto>> Handle(ListMyExamAttemptsQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var query = _context.ExamSessions
            .Where(s => s.NurseProfileId == nurseProfileId);

        if (request.Status is not null)
        {
            query = query.Where(s => s.Status == request.Status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .ThenBy(s => s.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var examIds = sessions.Select(s => s.ExamId).Distinct().ToList();
        var titles = await _context.Exams
            .Where(e => examIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Title })
            .ToDictionaryAsync(e => e.Id, e => e.Title, cancellationToken);

        return new PaginatedResult<ExamAttemptDto>
        {
            Items = sessions.Select(s => new ExamAttemptDto
            {
                Id = s.Id,
                ExamId = s.ExamId,
                ExamTitle = titles.GetValueOrDefault(s.ExamId, string.Empty),
                Status = s.Status.ToString(),
                StartedAt = s.StartedAt,
                ExpiresAt = s.ExpiresAt,
                FinalizedAt = s.FinalizedAt,
                Score = s.IsTerminal ? s.Score : null,
                MaxScore = s.IsTerminal ? s.MaxScore : null,
                Percentage = s.IsTerminal ? s.Percentage : null,
                Passed = s.IsTerminal ? s.Passed : null
            }).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
