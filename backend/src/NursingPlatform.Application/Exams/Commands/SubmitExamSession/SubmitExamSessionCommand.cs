using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Commands.SubmitExamSession;

public class SubmitExamSessionCommand : IRequest<ExamSessionResultDto>
{
    public Guid ExamSessionId { get; set; }
}
