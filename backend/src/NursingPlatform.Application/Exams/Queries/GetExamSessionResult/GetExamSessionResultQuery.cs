using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Queries.GetExamSessionResult;

public class GetExamSessionResultQuery : IRequest<ExamSessionResultDto>
{
    public Guid ExamSessionId { get; set; }
}
