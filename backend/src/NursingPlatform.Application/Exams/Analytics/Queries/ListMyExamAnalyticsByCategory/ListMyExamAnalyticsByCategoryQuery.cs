using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;

namespace NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByCategory;

public class ListMyExamAnalyticsByCategoryQuery : ExamAnalyticsFilters, IExamAnalyticsPaginatedQuery, IRequest<PaginatedResult<ExamAnalyticsByCategoryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
