using MediatR;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;

namespace NursingPlatform.Application.Exams.Analytics.Queries.GetMyExamAnalyticsSummary;

public class GetMyExamAnalyticsSummaryQuery : ExamAnalyticsFilters, IRequest<ExamAnalyticsSummaryDto>;
