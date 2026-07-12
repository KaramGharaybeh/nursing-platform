using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Commands.StartExamSession;

public class StartExamSessionCommand : IRequest<ExamSessionDto>
{
    public Guid ExamId { get; set; }
}
