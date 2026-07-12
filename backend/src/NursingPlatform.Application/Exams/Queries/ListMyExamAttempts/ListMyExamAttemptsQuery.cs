using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Queries.ListMyExamAttempts;

public class ListMyExamAttemptsQuery : IRequest<PaginatedResult<ExamAttemptDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public ExamSessionStatus? Status { get; set; }
}
