using FluentValidation;

namespace NursingPlatform.Application.Exams.Commands.SubmitExamSession;

public class SubmitExamSessionCommandValidator : AbstractValidator<SubmitExamSessionCommand>
{
    public SubmitExamSessionCommandValidator()
    {
        RuleFor(x => x.ExamSessionId).NotEmpty();
    }
}
