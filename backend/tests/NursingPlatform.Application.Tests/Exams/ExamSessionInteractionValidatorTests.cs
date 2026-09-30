using FluentValidation.TestHelper;
using NursingPlatform.Application.Exams.Commands.ClearExamSessionAnswer;
using NursingPlatform.Application.Exams.Commands.SetExamSessionQuestionFlag;

namespace NursingPlatform.Application.Tests.Exams;

public class ExamSessionInteractionValidatorTests
{
    [Fact]
    public void Validate_ClearExamSessionAnswer_WithEmptyIds_ShouldHaveError()
    {
        var result = new ClearExamSessionAnswerCommandValidator()
            .TestValidate(new ClearExamSessionAnswerCommand
            {
                ExamSessionId = Guid.Empty,
                ExamSessionQuestionId = Guid.Empty
            });

        result.ShouldHaveValidationErrorFor(x => x.ExamSessionId);
        result.ShouldHaveValidationErrorFor(x => x.ExamSessionQuestionId);
    }

    [Fact]
    public void Validate_ClearExamSessionAnswer_WithValidIds_ShouldNotHaveError()
    {
        var result = new ClearExamSessionAnswerCommandValidator()
            .TestValidate(new ClearExamSessionAnswerCommand
            {
                ExamSessionId = Guid.NewGuid(),
                ExamSessionQuestionId = Guid.NewGuid()
            });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SetExamSessionQuestionFlag_WithEmptyIds_ShouldHaveError()
    {
        var result = new SetExamSessionQuestionFlagCommandValidator()
            .TestValidate(new SetExamSessionQuestionFlagCommand
            {
                ExamSessionId = Guid.Empty,
                ExamSessionQuestionId = Guid.Empty,
                Request = new SetExamSessionQuestionFlagRequest { IsFlagged = true }
            });

        result.ShouldHaveValidationErrorFor(x => x.ExamSessionId);
        result.ShouldHaveValidationErrorFor(x => x.ExamSessionQuestionId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_SetExamSessionQuestionFlag_WithDesiredState_ShouldNotHaveError(bool isFlagged)
    {
        var result = new SetExamSessionQuestionFlagCommandValidator()
            .TestValidate(new SetExamSessionQuestionFlagCommand
            {
                ExamSessionId = Guid.NewGuid(),
                ExamSessionQuestionId = Guid.NewGuid(),
                Request = new SetExamSessionQuestionFlagRequest { IsFlagged = isFlagged }
            });

        result.ShouldNotHaveAnyValidationErrors();
    }
}
