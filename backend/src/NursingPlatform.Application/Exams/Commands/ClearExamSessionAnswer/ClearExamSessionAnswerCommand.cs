using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Commands.ClearExamSessionAnswer;

public class ClearExamSessionAnswerCommand : IRequest<ExamSessionDto>
{
    public Guid ExamSessionId { get; set; }
    public Guid ExamSessionQuestionId { get; set; }
}
