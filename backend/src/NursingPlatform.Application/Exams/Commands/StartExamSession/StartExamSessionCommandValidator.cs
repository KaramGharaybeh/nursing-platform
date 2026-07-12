using FluentValidation;

namespace NursingPlatform.Application.Exams.Commands.StartExamSession;

public class StartExamSessionCommandValidator : AbstractValidator<StartExamSessionCommand>
{
    public StartExamSessionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
    }
}
