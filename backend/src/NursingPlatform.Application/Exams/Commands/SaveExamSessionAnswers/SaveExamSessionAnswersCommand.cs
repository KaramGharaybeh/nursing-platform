using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Commands.SaveExamSessionAnswers;

public class SaveExamSessionAnswersCommand : IRequest<ExamSessionDto>
{
    public Guid ExamSessionId { get; set; }
    public SaveExamSessionAnswersRequest Request { get; set; } = new();
}
