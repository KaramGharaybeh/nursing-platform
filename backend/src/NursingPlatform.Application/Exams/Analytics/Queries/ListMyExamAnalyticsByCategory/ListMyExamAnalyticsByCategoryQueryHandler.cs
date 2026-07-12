using MediatR;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByCategory;

public class ListMyExamAnalyticsByCategoryQueryHandler : IRequestHandler<ListMyExamAnalyticsByCategoryQuery, PaginatedResult<ExamAnalyticsByCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListMyExamAnalyticsByCategoryQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaginatedResult<ExamAnalyticsByCategoryDto>> Handle(ListMyExamAnalyticsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var rows = await ExamAnalyticsDataLoader.LoadRowsAsync(_context, nurseProfileId, request, cancellationToken);
        var groups = rows
            .GroupBy(r => new { r.CountryId, r.CountryName, r.CategoryId, r.CategoryName })
            .OrderBy(g => g.Key.CountryName)
            .ThenBy(g => g.Key.CategoryName)
            .ThenBy(g => g.Key.CountryId)
            .ThenBy(g => g.Key.CategoryId)
            .Select(g =>
            {
                var dto = new ExamAnalyticsByCategoryDto
                {
                    CountryId = g.Key.CountryId,
                    CountryName = g.Key.CountryName,
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName
                };
                ExamAnalyticsMetricCalculator.ApplySnapshot(dto, ExamAnalyticsMetricCalculator.Calculate(g));
                return dto;
            })
            .ToList();

        return new PaginatedResult<ExamAnalyticsByCategoryDto>
        {
            Items = groups.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = groups.Count
        };
    }
}
