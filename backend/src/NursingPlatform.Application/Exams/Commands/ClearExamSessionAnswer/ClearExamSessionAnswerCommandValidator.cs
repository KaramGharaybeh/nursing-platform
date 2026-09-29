using FluentValidation;

namespace NursingPlatform.Application.Exams.Commands.ClearExamSessionAnswer;

public class ClearExamSessionAnswerCommandValidator : AbstractValidator<ClearExamSessionAnswerCommand>
{
    public ClearExamSessionAnswerCommandValidator()
    {
        RuleFor(x => x.ExamSessionId).NotEmpty();
        RuleFor(x => x.ExamSessionQuestionId).NotEmpty();
    }
}
