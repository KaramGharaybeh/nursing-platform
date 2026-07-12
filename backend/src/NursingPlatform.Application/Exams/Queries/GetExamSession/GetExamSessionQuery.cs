using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Queries.GetExamSession;

public class GetExamSessionQuery : IRequest<ExamSessionDto>
{
    public Guid ExamSessionId { get; set; }
}
