using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Commands.SetExamSessionQuestionFlag;

public class SetExamSessionQuestionFlagCommand : IRequest<ExamSessionDto>
{
    public Guid ExamSessionId { get; set; }
    public Guid ExamSessionQuestionId { get; set; }
    public SetExamSessionQuestionFlagRequest Request { get; set; } = new();
}
