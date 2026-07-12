using MediatR;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Queries.GetExam;

public class GetExamQuery : IRequest<ExamDetailDto>
{
    public Guid ExamId { get; set; }
}
