using MediatR;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsTrends;

public class ListMyExamAnalyticsTrendsQueryHandler : IRequestHandler<ListMyExamAnalyticsTrendsQuery, IReadOnlyList<ExamAnalyticsTrendPointDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListMyExamAnalyticsTrendsQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<IReadOnlyList<ExamAnalyticsTrendPointDto>> Handle(ListMyExamAnalyticsTrendsQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var rows = await ExamAnalyticsDataLoader.LoadRowsAsync(_context, nurseProfileId, request, cancellationToken);

        return rows
            .GroupBy(r => ExamAnalyticsMetricCalculator.GetBucket(r.StartedAt, request.Bucket))
            .OrderBy(g => g.Key.Start)
            .Select(g => ExamAnalyticsMetricCalculator.CreateTrendPoint(g.Key.Start, g.Key.End, g))
            .ToList();
    }
}
