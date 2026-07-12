using FluentValidation;

namespace NursingPlatform.Application.Exams.Commands.SaveExamSessionAnswers;

public class SaveExamSessionAnswersCommandValidator : AbstractValidator<SaveExamSessionAnswersCommand>
{
    public SaveExamSessionAnswersCommandValidator()
    {
        RuleFor(x => x.ExamSessionId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.Answers)
            .NotEmpty()
            .Must(answers => answers.Select(a => a.ExamSessionQuestionId).Distinct().Count() == answers.Count)
            .WithMessage("Duplicate exam session question ids are not allowed.");
        RuleForEach(x => x.Request.Answers).ChildRules(answer =>
        {
            answer.RuleFor(x => x.ExamSessionQuestionId).NotEmpty();
            answer.RuleFor(x => x.SelectedExamSessionAnswerOptionId).NotEmpty();
        });
    }
}
