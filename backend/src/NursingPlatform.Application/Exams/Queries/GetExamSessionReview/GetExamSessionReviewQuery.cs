using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Queries.GetExamSessionReview;

public class GetExamSessionReviewQuery : IRequest<ExamSessionReviewDto>
{
    public Guid ExamSessionId { get; set; }
}
