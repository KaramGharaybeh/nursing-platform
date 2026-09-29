using FluentValidation;

namespace NursingPlatform.Application.Exams.Commands.SetExamSessionQuestionFlag;

public class SetExamSessionQuestionFlagCommandValidator : AbstractValidator<SetExamSessionQuestionFlagCommand>
{
    public SetExamSessionQuestionFlagCommandValidator()
    {
        RuleFor(x => x.ExamSessionId).NotEmpty();
        RuleFor(x => x.ExamSessionQuestionId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
    }
}
