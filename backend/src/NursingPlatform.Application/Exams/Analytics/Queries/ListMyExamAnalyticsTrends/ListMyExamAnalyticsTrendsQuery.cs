using MediatR;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;

namespace NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsTrends;

public class ListMyExamAnalyticsTrendsQuery : ExamAnalyticsFilters, IRequest<IReadOnlyList<ExamAnalyticsTrendPointDto>>
{
    public ExamAnalyticsBucket Bucket { get; set; } = ExamAnalyticsBucket.Month;
}
