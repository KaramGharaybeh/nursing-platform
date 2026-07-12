using MediatR;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Exams.Analytics.Queries.GetMyExamAnalyticsSummary;

public class GetMyExamAnalyticsSummaryQueryHandler : IRequestHandler<GetMyExamAnalyticsSummaryQuery, ExamAnalyticsSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetMyExamAnalyticsSummaryQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<ExamAnalyticsSummaryDto> Handle(GetMyExamAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var rows = await ExamAnalyticsDataLoader.LoadRowsAsync(_context, nurseProfileId, request, cancellationToken);
        var dto = new ExamAnalyticsSummaryDto();

        ExamAnalyticsMetricCalculator.ApplySnapshot(dto, ExamAnalyticsMetricCalculator.Calculate(rows));

        return dto;
    }
}
