using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Admin.AnswerOptions;
using NursingPlatform.Application.Exams.Admin.Categories;
using NursingPlatform.Application.Exams.Admin.Exams;
using NursingPlatform.Application.Exams.Admin.Questions;
using NursingPlatform.Application.Exams.Admin.Versions;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Exams.Admin;

public class AdminExamContentHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Handle_CreateCategory_CreatesActiveCategory()
    {
        var country = CreateCountry();
        var categories = new List<ExamCategory>();
        SetupContext([country], categories);
        var handler = new CreateAdminExamCategoryCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new CreateAdminExamCategoryCommand
        {
            Request = new CreateAdminExamCategoryRequest
            {
                CountryId = country.Id,
                Name = "NCLEX",
                Slug = "nclex",
                DisplayOrder = 1
            }
        }, CancellationToken.None);

        Assert.Equal("NCLEX", result.Name);
        Assert.True(result.IsActive);
        Assert.Single(categories);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateCategory_WhenCountryIdChanges_ThrowsInvalidOperationException()
    {
        var country = CreateCountry();
        var category = CreateCategory(country.Id);
        SetupContext([country], [category]);
        var handler = new UpdateAdminExamCategoryCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateAdminExamCategoryCommand
            {
                Id = category.Id,
                Request = new UpdateAdminExamCategoryRequest
                {
                    CountryId = Guid.NewGuid(),
                    Name = "Updated",
                    Slug = "updated"
                }
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeleteCategory_WhenReferencedByExam_ThrowsInvalidOperationException()
    {
        var country = CreateCountry();
        var category = CreateCategory(country.Id);
        var exam = CreateExam(country.Id, category.Id);
        SetupContext([country], [category], [exam]);
        var handler = new DeleteAdminExamCategoryCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteAdminExamCategoryCommand { Id = category.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ArchiveCategory_SetsIsActiveFalse()
    {
        var country = CreateCountry();
        var category = CreateCategory(country.Id);
        SetupContext([country], [category]);
        var handler = new ArchiveAdminExamCategoryCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new ArchiveAdminExamCategoryCommand { Id = category.Id }, CancellationToken.None);

        Assert.False(result.IsActive);
        Assert.False(category.IsActive);
    }

    [Fact]
    public async Task Handle_CreateExam_CreatesDraftExam()
    {
        var country = CreateCountry();
        var category = CreateCategory(country.Id);
        var exams = new List<Exam>();
        SetupContext([country], [category], exams);
        var handler = new CreateAdminExamCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new CreateAdminExamCommand
        {
            Request = CreateExamRequest(country.Id, category.Id)
        }, CancellationToken.None);

        Assert.Equal("Draft", result.Status);
        Assert.Single(exams);
    }

    [Fact]
    public async Task Handle_UpdateExam_WithStructuralOrScoringFieldChangeAndPublishedVersion_ThrowsInvalidOperationException()
    {
        var country = CreateCountry();
        var category = CreateCategory(country.Id);
        var exam = CreateExam(country.Id, category.Id);
        var version = CreateVersion(exam.Id, ExamVersionStatus.Published);
        SetupContext([country], [category], [exam], [version]);
        var handler = new UpdateAdminExamCommandHandler(_contextMock.Object);
        var request = CreateUpdateExamRequest(country.Id, category.Id);
        request.DurationMinutes = 120;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateAdminExamCommand { Id = exam.Id, Request = request }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UpdateExam_WithStructuralOrScoringFieldChangeAndSession_ThrowsInvalidOperationException()
    {
        var country = CreateCountry();
        var category = CreateCategory(country.Id);
        var exam = CreateExam(country.Id, category.Id);
        var session = new ExamSession { Id = Guid.NewGuid(), ExamId = exam.Id, ExamVersionId = Guid.NewGuid(), NurseProfileId = Guid.NewGuid() };
        SetupContext([country], [category], [exam], [], [], [], [session]);
        var handler = new UpdateAdminExamCommandHandler(_contextMock.Object);
        var request = CreateUpdateExamRequest(country.Id, category.Id);
        request.PassingScorePercentage = 80;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateAdminExamCommand { Id = exam.Id, Request = request }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UpdateExam_WithSafeDisplayFieldChangesAndPublishedVersion_UpdatesFields()
    {
        var country = CreateCountry();
        var category = CreateCategory(country.Id);
        var exam = CreateExam(country.Id, category.Id);
        var version = CreateVersion(exam.Id, ExamVersionStatus.Published);
        SetupContext([country], [category], [exam], [version]);
        var handler = new UpdateAdminExamCommandHandler(_contextMock.Object);
        var request = CreateUpdateExamRequest(country.Id, category.Id);
        request.Title = "Updated NCLEX";
        request.Slug = "updated-nclex";
        request.Description = "Updated";
        request.Instructions = "Updated instructions";
        request.IsFree = false;

        var result = await handler.Handle(new UpdateAdminExamCommand { Id = exam.Id, Request = request }, CancellationToken.None);

        Assert.Equal("Updated NCLEX", result.Title);
        Assert.False(result.IsFree);
        Assert.Equal(60, exam.DurationMinutes);
    }

    [Fact]
    public async Task Handle_UpdateExam_WithCategoryCountryMismatch_ThrowsInvalidOperationException()
    {
        var country = CreateCountry();
        var otherCountry = CreateCountry();
        var category = CreateCategory(otherCountry.Id);
        var exam = CreateExam(country.Id, null);
        SetupContext([country, otherCountry], [category], [exam]);
        var handler = new UpdateAdminExamCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateAdminExamCommand
            {
                Id = exam.Id,
                Request = CreateUpdateExamRequest(country.Id, category.Id)
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CreateDraftVersion_AssignsNextVersionNumber()
    {
        var country = CreateCountry();
        var exam = CreateExam(country.Id, null);
        var versions = new List<ExamVersion> { CreateVersion(exam.Id, ExamVersionStatus.Published, 1) };
        SetupContext([country], [], [exam], versions);
        var handler = new CreateAdminDraftExamVersionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new CreateAdminDraftExamVersionCommand { ExamId = exam.Id }, CancellationToken.None);

        Assert.Equal(2, result.VersionNumber);
        Assert.Equal(2, versions.Count);
    }

    [Fact]
    public void Handle_DraftVersionUpdateEndpoint_IsNotImplementedWhenNoEditableFieldsExist()
    {
        var requestType = typeof(CreateAdminDraftExamVersionCommand).Assembly.GetType(
            "NursingPlatform.Application.Exams.Admin.Versions.UpsertAdminExamVersionRequest",
            throwOnError: false);

        Assert.Null(requestType);
    }

    [Fact]
    public async Task Handle_ValidateDraftVersion_WithValidContent_ReturnsValidSummary()
    {
        var (exam, version, question, options) = CreateValidDraftContent();
        SetupContext([CreateCountry(exam.CountryId)], [], [exam], [version], [question], options);
        var handler = new ValidateAdminDraftExamVersionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new ValidateAdminDraftExamVersionCommand { ExamId = exam.Id, VersionId = version.Id }, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(1, result.QuestionCount);
        Assert.Equal(question.Points, result.TotalPoints);
    }

    [Fact]
    public async Task Handle_PublishDraftVersion_WithValidContent_PublishesVersionAndExam()
    {
        var (exam, version, question, options) = CreateValidDraftContent();
        SetupContext([CreateCountry(exam.CountryId)], [], [exam], [version], [question], options);
        var handler = new PublishAdminDraftExamVersionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new PublishAdminDraftExamVersionCommand { ExamId = exam.Id, VersionId = version.Id }, CancellationToken.None);

        Assert.Equal("Published", result.Status);
        Assert.Equal(ExamStatus.Published, exam.Status);
        Assert.NotNull(version.PublishedAt);
    }

    [Fact]
    public async Task Handle_PublishDraftVersion_WithInvalidContent_ThrowsInvalidOperationException()
    {
        var country = CreateCountry();
        var exam = CreateExam(country.Id, null);
        var version = CreateVersion(exam.Id, ExamVersionStatus.Draft);
        SetupContext([country], [], [exam], [version]);
        var handler = new PublishAdminDraftExamVersionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new PublishAdminDraftExamVersionCommand { ExamId = exam.Id, VersionId = version.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CreateQuestion_WhenVersionNotDraft_ThrowsInvalidOperationException()
    {
        var country = CreateCountry();
        var exam = CreateExam(country.Id, null);
        var version = CreateVersion(exam.Id, ExamVersionStatus.Published);
        SetupContext([country], [], [exam], [version]);
        var handler = new CreateAdminExamQuestionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateAdminExamQuestionCommand
            {
                ExamId = exam.Id,
                VersionId = version.Id,
                Request = new UpsertAdminExamQuestionRequest { QuestionText = "Q", Points = 1 }
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UpdateQuestion_WhenVersionDraft_UpdatesContent()
    {
        var (exam, version, question, options) = CreateValidDraftContent();
        SetupContext([CreateCountry(exam.CountryId)], [], [exam], [version], [question], options);
        var handler = new UpdateAdminExamQuestionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new UpdateAdminExamQuestionCommand
        {
            ExamId = exam.Id,
            VersionId = version.Id,
            QuestionId = question.Id,
            Request = new UpsertAdminExamQuestionRequest { QuestionText = "Updated?", Points = 3 }
        }, CancellationToken.None);

        Assert.Equal("Updated?", result.QuestionText);
        Assert.Equal(3, question.Points);
    }

    [Fact]
    public async Task Handle_DeleteQuestion_WhenReferencedBySessionSnapshot_ThrowsInvalidOperationException()
    {
        var (exam, version, question, options) = CreateValidDraftContent();
        var sessionQuestion = new ExamSessionQuestion { Id = Guid.NewGuid(), ExamQuestionId = question.Id, ExamSessionId = Guid.NewGuid() };
        SetupContext([CreateCountry(exam.CountryId)], [], [exam], [version], [question], options, [], [sessionQuestion]);
        var handler = new DeleteAdminExamQuestionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteAdminExamQuestionCommand { ExamId = exam.Id, VersionId = version.Id, QuestionId = question.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CreateAnswerOption_WhenVersionDraft_CreatesOption()
    {
        var (exam, version, question, options) = CreateValidDraftContent();
        SetupContext([CreateCountry(exam.CountryId)], [], [exam], [version], [question], options);
        var handler = new CreateAdminExamAnswerOptionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new CreateAdminExamAnswerOptionCommand
        {
            ExamId = exam.Id,
            VersionId = version.Id,
            QuestionId = question.Id,
            Request = new UpsertAdminExamAnswerOptionRequest { OptionText = "C", DisplayOrder = 3 }
        }, CancellationToken.None);

        Assert.Equal("C", result.OptionText);
        Assert.Equal(3, options.Count);
    }

    [Fact]
    public async Task Handle_UpdateAnswerOption_WhenVersionNotDraft_ThrowsInvalidOperationException()
    {
        var (exam, version, question, options) = CreateValidDraftContent(ExamVersionStatus.Published);
        SetupContext([CreateCountry(exam.CountryId)], [], [exam], [version], [question], options);
        var handler = new UpdateAdminExamAnswerOptionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateAdminExamAnswerOptionCommand
            {
                ExamId = exam.Id,
                VersionId = version.Id,
                QuestionId = question.Id,
                OptionId = options[0].Id,
                Request = new UpsertAdminExamAnswerOptionRequest { OptionText = "A" }
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeleteAnswerOption_WhenReferencedBySessionSnapshot_ThrowsInvalidOperationException()
    {
        var (exam, version, question, options) = CreateValidDraftContent();
        var sessionOption = new ExamSessionAnswerOption { Id = Guid.NewGuid(), ExamAnswerOptionId = options[0].Id, ExamSessionQuestionId = Guid.NewGuid() };
        SetupContext([CreateCountry(exam.CountryId)], [], [exam], [version], [question], options, [], [], [sessionOption]);
        var handler = new DeleteAdminExamAnswerOptionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteAdminExamAnswerOptionCommand
            {
                ExamId = exam.Id,
                VersionId = version.Id,
                QuestionId = question.Id,
                OptionId = options[0].Id
            }, CancellationToken.None));
    }

    private void SetupContext(
        IReadOnlyCollection<Country> countries,
        IReadOnlyCollection<ExamCategory> categories,
        IReadOnlyCollection<Exam>? exams = null,
        IReadOnlyCollection<ExamVersion>? versions = null,
        IReadOnlyCollection<ExamQuestion>? questions = null,
        IReadOnlyCollection<ExamAnswerOption>? options = null,
        IReadOnlyCollection<ExamSession>? sessions = null,
        IReadOnlyCollection<ExamSessionQuestion>? sessionQuestions = null,
        IReadOnlyCollection<ExamSessionAnswerOption>? sessionOptions = null)
    {
        var categoryList = categories as List<ExamCategory> ?? categories.ToList();
        var examList = exams as List<Exam> ?? exams?.ToList() ?? [];
        var versionList = versions as List<ExamVersion> ?? versions?.ToList() ?? [];
        var questionList = questions as List<ExamQuestion> ?? questions?.ToList() ?? [];
        var optionList = options as List<ExamAnswerOption> ?? options?.ToList() ?? [];

        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
        SetupDbSet(c => c.ExamCategories, categoryList);
        SetupDbSet(c => c.Exams, examList);
        SetupDbSet(c => c.ExamVersions, versionList);
        SetupDbSet(c => c.ExamQuestions, questionList);
        SetupDbSet(c => c.ExamAnswerOptions, optionList);
        _contextMock.Setup(c => c.ExamSessions).Returns((sessions ?? []).AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ExamSessionQuestions).Returns((sessionQuestions ?? []).AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ExamSessionAnswerOptions).Returns((sessionOptions ?? []).AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupDbSet<T>(System.Linq.Expressions.Expression<Func<IApplicationDbContext, Microsoft.EntityFrameworkCore.DbSet<T>>> expression, List<T> items)
        where T : class
    {
        var dbSet = items.AsQueryable().BuildMockDbSet();
        dbSet.Setup(s => s.Add(It.IsAny<T>())).Callback<T>(items.Add);
        dbSet.Setup(s => s.Remove(It.IsAny<T>())).Callback<T>(item => items.Remove(item));
        dbSet.Setup(s => s.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(removed =>
        {
            foreach (var item in removed.ToList())
            {
                items.Remove(item);
            }
        });
        _contextMock.Setup(expression).Returns(dbSet.Object);
    }

    private static Country CreateCountry(Guid? id = null)
    {
        return new Country
        {
            Id = id ?? Guid.NewGuid(),
            Name = "United States",
            Code = "US",
            IsActive = true
        };
    }

    private static ExamCategory CreateCategory(Guid countryId)
    {
        return new ExamCategory
        {
            Id = Guid.NewGuid(),
            CountryId = countryId,
            Name = "NCLEX",
            Slug = "nclex",
            IsActive = true
        };
    }

    private static Exam CreateExam(Guid countryId, Guid? categoryId)
    {
        return new Exam
        {
            Id = Guid.NewGuid(),
            CountryId = countryId,
            ExamCategoryId = categoryId,
            Title = "NCLEX RN",
            Slug = "nclex-rn",
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = ExamStatus.Draft,
            IsFree = true
        };
    }

    private static CreateAdminExamRequest CreateExamRequest(Guid countryId, Guid? categoryId)
    {
        return new CreateAdminExamRequest
        {
            CountryId = countryId,
            ExamCategoryId = categoryId,
            Title = "NCLEX RN",
            Slug = "nclex-rn",
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            IsFree = true
        };
    }

    private static UpdateAdminExamRequest CreateUpdateExamRequest(Guid countryId, Guid? categoryId)
    {
        return new UpdateAdminExamRequest
        {
            CountryId = countryId,
            ExamCategoryId = categoryId,
            Title = "NCLEX RN",
            Slug = "nclex-rn",
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            IsFree = true
        };
    }

    private static ExamVersion CreateVersion(Guid examId, ExamVersionStatus status, int versionNumber = 1)
    {
        return new ExamVersion
        {
            Id = Guid.NewGuid(),
            ExamId = examId,
            VersionNumber = versionNumber,
            Status = status
        };
    }

    private static (Exam Exam, ExamVersion Version, ExamQuestion Question, List<ExamAnswerOption> Options) CreateValidDraftContent(
        ExamVersionStatus versionStatus = ExamVersionStatus.Draft)
    {
        var countryId = Guid.NewGuid();
        var exam = CreateExam(countryId, null);
        var version = CreateVersion(exam.Id, versionStatus);
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = version.Id,
            QuestionText = "What is first?",
            QuestionType = ExamQuestionType.SingleBestAnswer,
            Points = 2,
            DisplayOrder = 1,
            IsActive = true
        };
        var options = new List<ExamAnswerOption>
        {
            new() { Id = Guid.NewGuid(), ExamQuestionId = question.Id, OptionText = "A", DisplayOrder = 1, IsCorrect = true, IsActive = true },
            new() { Id = Guid.NewGuid(), ExamQuestionId = question.Id, OptionText = "B", DisplayOrder = 2, IsCorrect = false, IsActive = true }
        };

        return (exam, version, question, options);
    }
}
