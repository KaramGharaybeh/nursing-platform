using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;

namespace NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByExam;

public class ListMyExamAnalyticsByExamQuery : ExamAnalyticsFilters, IExamAnalyticsPaginatedQuery, IRequest<PaginatedResult<ExamAnalyticsByExamDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
