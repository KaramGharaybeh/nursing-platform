using FluentValidation.TestHelper;
using NursingPlatform.Application.Exams.Commands.SaveExamSessionAnswers;
using NursingPlatform.Application.Exams.Commands.StartExamSession;
using NursingPlatform.Application.Exams.Commands.SubmitExamSession;
using NursingPlatform.Application.Exams.Queries.ListExams;

namespace NursingPlatform.Application.Tests.Exams;

public class ExamValidatorTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_ListExams_WithInvalidPagination_ShouldHaveError(int page, int pageSize)
    {
        var result = new ListExamsQueryValidator()
            .TestValidate(new ListExamsQuery { Page = page, PageSize = pageSize });

        if (page < 1)
        {
            result.ShouldHaveValidationErrorFor(x => x.Page);
        }

        if (pageSize is < 1 or > 100)
        {
            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }
    }

    [Fact]
    public void Validate_StartExamSession_WithEmptyExamId_ShouldHaveError()
    {
        var result = new StartExamSessionCommandValidator()
            .TestValidate(new StartExamSessionCommand { ExamId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.ExamId);
    }

    [Fact]
    public void Validate_SaveExamSessionAnswers_WithEmptyAnswers_ShouldHaveError()
    {
        var result = new SaveExamSessionAnswersCommandValidator()
            .TestValidate(new SaveExamSessionAnswersCommand
            {
                ExamSessionId = Guid.NewGuid(),
                Request = new SaveExamSessionAnswersRequest { Answers = [] }
            });

        result.ShouldHaveValidationErrorFor("Request.Answers");
    }

    [Fact]
    public void Validate_SaveExamSessionAnswers_WithDuplicateExamSessionQuestionId_ShouldHaveError()
    {
        var questionId = Guid.NewGuid();

        var result = new SaveExamSessionAnswersCommandValidator()
            .TestValidate(new SaveExamSessionAnswersCommand
            {
                ExamSessionId = Guid.NewGuid(),
                Request = new SaveExamSessionAnswersRequest
                {
                    Answers =
                    [
                        new SaveExamSessionAnswerItemRequest
                        {
                            ExamSessionQuestionId = questionId,
                            SelectedExamSessionAnswerOptionId = Guid.NewGuid()
                        },
                        new SaveExamSessionAnswerItemRequest
                        {
                            ExamSessionQuestionId = questionId,
                            SelectedExamSessionAnswerOptionId = Guid.NewGuid()
                        }
                    ]
                }
            });

        result.ShouldHaveValidationErrorFor("Request.Answers");
    }

    [Fact]
    public void Validate_SaveExamSessionAnswers_WithEmptyGuidValues_ShouldHaveError()
    {
        var result = new SaveExamSessionAnswersCommandValidator()
            .TestValidate(new SaveExamSessionAnswersCommand
            {
                ExamSessionId = Guid.Empty,
                Request = new SaveExamSessionAnswersRequest
                {
                    Answers =
                    [
                        new SaveExamSessionAnswerItemRequest
                        {
                            ExamSessionQuestionId = Guid.Empty,
                            SelectedExamSessionAnswerOptionId = Guid.Empty
                        }
                    ]
                }
            });

        result.ShouldHaveValidationErrorFor(x => x.ExamSessionId);
        result.ShouldHaveValidationErrorFor("Request.Answers[0].ExamSessionQuestionId");
        result.ShouldHaveValidationErrorFor("Request.Answers[0].SelectedExamSessionAnswerOptionId");
    }

    [Fact]
    public void Validate_SubmitExamSession_WithEmptyId_ShouldHaveError()
    {
        var result = new SubmitExamSessionCommandValidator()
            .TestValidate(new SubmitExamSessionCommand { ExamSessionId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.ExamSessionId);
    }
}
