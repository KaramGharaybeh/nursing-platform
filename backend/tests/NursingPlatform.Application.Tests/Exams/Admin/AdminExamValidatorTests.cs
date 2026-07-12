using FluentValidation.TestHelper;
using NursingPlatform.Application.Exams.Admin.AnswerOptions;
using NursingPlatform.Application.Exams.Admin.Categories;
using NursingPlatform.Application.Exams.Admin.Exams;
using NursingPlatform.Application.Exams.Admin.Questions;
using NursingPlatform.Application.Exams.Admin.Versions;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Tests.Exams.Admin;

public class AdminExamValidatorTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_ListAdminExamCategories_WithInvalidPagination_ShouldHaveError(int page, int pageSize)
    {
        var result = new ListAdminExamCategoriesQueryValidator()
            .TestValidate(new ListAdminExamCategoriesQuery { Page = page, PageSize = pageSize });

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
    public void Validate_UpsertCategory_WithInvalidFields_ShouldHaveError()
    {
        new CreateAdminExamCategoryCommandValidator()
            .TestValidate(new CreateAdminExamCategoryCommand
            {
                Request = new CreateAdminExamCategoryRequest { CountryId = Guid.Empty, Name = "", Slug = "" }
            })
            .ShouldHaveValidationErrorFor("Request.CountryId");
    }

    [Fact]
    public void Validate_UpdateCategory_WithCountryIdChangeAttempt_ShouldHaveConflictTestCoverage()
    {
        var result = new UpdateAdminExamCategoryCommandValidator()
            .TestValidate(new UpdateAdminExamCategoryCommand
            {
                Id = Guid.NewGuid(),
                Request = new UpdateAdminExamCategoryRequest { CountryId = Guid.NewGuid(), Name = "NCLEX", Slug = "nclex" }
            });

        result.ShouldNotHaveValidationErrorFor(x => x.Request.CountryId);
    }

    [Fact]
    public void Validate_UpsertExam_WithInvalidDurationOrPassingScore_ShouldHaveError()
    {
        var result = new CreateAdminExamCommandValidator()
            .TestValidate(new CreateAdminExamCommand
            {
                Request = new CreateAdminExamRequest
                {
                    CountryId = Guid.NewGuid(),
                    Title = "Exam",
                    Slug = "exam",
                    DurationMinutes = 0,
                    PassingScorePercentage = 101
                }
            });

        result.ShouldHaveValidationErrorFor("Request.DurationMinutes");
        result.ShouldHaveValidationErrorFor("Request.PassingScorePercentage");
    }

    [Fact]
    public void Validate_CreateDraftVersion_WithEmptyExamId_ShouldHaveError()
    {
        new CreateAdminDraftExamVersionCommandValidator()
            .TestValidate(new CreateAdminDraftExamVersionCommand { ExamId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.ExamId);
    }

    [Fact]
    public void Validate_NoUpsertAdminExamVersionRequest_IsCreated()
    {
        var type = typeof(CreateAdminDraftExamVersionCommand).Assembly.GetType(
            "NursingPlatform.Application.Exams.Admin.Versions.UpsertAdminExamVersionRequest",
            throwOnError: false);

        Assert.Null(type);
    }

    [Fact]
    public void Validate_Question_WithUnsupportedQuestionType_ShouldHaveError()
    {
        new UpsertAdminExamQuestionRequestValidator()
            .TestValidate(new UpsertAdminExamQuestionRequest { QuestionText = "Q", QuestionType = (ExamQuestionType)99, Points = 1 })
            .ShouldHaveValidationErrorFor(x => x.QuestionType);
    }

    [Fact]
    public void Validate_Question_WithNonPositivePoints_ShouldHaveError()
    {
        new UpsertAdminExamQuestionRequestValidator()
            .TestValidate(new UpsertAdminExamQuestionRequest { QuestionText = "Q", Points = 0 })
            .ShouldHaveValidationErrorFor(x => x.Points);
    }

    [Fact]
    public void Validate_AnswerOption_WithEmptyText_ShouldHaveError()
    {
        new UpsertAdminExamAnswerOptionRequestValidator()
            .TestValidate(new UpsertAdminExamAnswerOptionRequest { OptionText = "" })
            .ShouldHaveValidationErrorFor(x => x.OptionText);
    }

    [Fact]
    public void Validate_AllRouteIds_WithEmptyGuid_ShouldHaveError()
    {
        new UpdateAdminExamQuestionCommandValidator()
            .TestValidate(new UpdateAdminExamQuestionCommand
            {
                ExamId = Guid.Empty,
                VersionId = Guid.Empty,
                QuestionId = Guid.Empty,
                Request = new UpsertAdminExamQuestionRequest { QuestionText = "Q", Points = 1 }
            })
            .ShouldHaveValidationErrorFor(x => x.ExamId);

        new UpdateAdminExamAnswerOptionCommandValidator()
            .TestValidate(new UpdateAdminExamAnswerOptionCommand
            {
                ExamId = Guid.Empty,
                VersionId = Guid.Empty,
                QuestionId = Guid.Empty,
                OptionId = Guid.Empty,
                Request = new UpsertAdminExamAnswerOptionRequest { OptionText = "A" }
            })
            .ShouldHaveValidationErrorFor(x => x.OptionId);
    }
}
