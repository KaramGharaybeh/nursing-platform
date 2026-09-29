using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Infrastructure.Persistence;
using NursingPlatform.Infrastructure.Persistence.Seed;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class DevelopmentExamSeederTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedAsync(ApplicationDbContext context)
    {
        await ReferenceDataSeeder.SeedAsync(context);
        await DevelopmentExamSeeder.SeedAsync(context);
    }

    [Fact]
    public async Task SeedAsync_CreatesPublishedFreeExamWithPublishedVersion()
    {
        var context = CreateDbContext();

        await SeedAsync(context);

        var exam = await context.Exams.SingleAsync(e => e.Slug == DevelopmentExamSeeder.ExamSlug);
        Assert.Equal(ExamStatus.Published, exam.Status);
        Assert.True(exam.IsFree);
        Assert.NotNull(exam.PublishedAt);

        var version = await context.ExamVersions.SingleAsync(v => v.ExamId == exam.Id);
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal(ExamVersionStatus.Published, version.Status);
        Assert.NotNull(version.PublishedAt);
        Assert.Equal(30, version.QuestionCount);
        Assert.Equal(30, version.TotalPoints);
    }

    [Fact]
    public async Task SeedAsync_CreatesExactly30Questions()
    {
        var context = CreateDbContext();

        await SeedAsync(context);

        var exam = await context.Exams.SingleAsync(e => e.Slug == DevelopmentExamSeeder.ExamSlug);
        var version = await context.ExamVersions.SingleAsync(v => v.ExamId == exam.Id);
        var questions = await context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id)
            .ToListAsync();

        Assert.Equal(30, questions.Count);
        Assert.All(questions, q => Assert.True(q.IsActive));
    }

    [Fact]
    public async Task SeedAsync_AllQuestionsAreAdditionOnly()
    {
        var context = CreateDbContext();

        await SeedAsync(context);

        var exam = await context.Exams.SingleAsync(e => e.Slug == DevelopmentExamSeeder.ExamSlug);
        var version = await context.ExamVersions.SingleAsync(v => v.ExamId == exam.Id);
        var questions = await context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id)
            .ToListAsync();

        var addition = new Regex(@"^(\d+) \+ (\d+) = \?$", RegexOptions.Compiled);
        foreach (var question in questions)
        {
            var match = addition.Match(question.QuestionText);
            Assert.True(match.Success, $"Question text is not a simple addition equation: '{question.QuestionText}'");
            Assert.DoesNotContain("-", question.QuestionText, StringComparison.Ordinal);
            Assert.DoesNotContain("/", question.QuestionText, StringComparison.Ordinal);
            Assert.DoesNotContain("*", question.QuestionText, StringComparison.Ordinal);
            Assert.DoesNotContain("×", question.QuestionText, StringComparison.Ordinal);
            Assert.DoesNotContain("÷", question.QuestionText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task SeedAsync_EachQuestionHasExactlyOneCorrectNumericOptionMatchingTheSum()
    {
        var context = CreateDbContext();

        await SeedAsync(context);

        var exam = await context.Exams.SingleAsync(e => e.Slug == DevelopmentExamSeeder.ExamSlug);
        var version = await context.ExamVersions.SingleAsync(v => v.ExamId == exam.Id);
        var questions = await context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id)
            .ToListAsync();
        var questionIds = questions.Select(q => q.Id).ToList();
        var options = await context.ExamAnswerOptions
            .Where(o => questionIds.Contains(o.ExamQuestionId))
            .ToListAsync();

        var addition = new Regex(@"^(\d+) \+ (\d+) = \?$", RegexOptions.Compiled);
        foreach (var question in questions)
        {
            var match = addition.Match(question.QuestionText);
            Assert.True(match.Success);
            var expectedSum = int.Parse(match.Groups[1].Value) + int.Parse(match.Groups[2].Value);

            var activeOptions = options.Where(o => o.ExamQuestionId == question.Id && o.IsActive).ToList();
            Assert.True(activeOptions.Count >= 2, $"Question '{question.QuestionText}' has fewer than 2 active options.");
            Assert.Equal(4, activeOptions.Count);
            Assert.Equal(1, activeOptions.Count(o => o.IsCorrect));

            var values = activeOptions.Select(o => o.OptionText).ToList();
            Assert.Equal(values.Count, values.Distinct(StringComparer.Ordinal).Count());
            Assert.All(values, value => Assert.True(int.TryParse(value, out _), $"Option '{value}' is not numeric."));

            var correct = Assert.Single(activeOptions, o => o.IsCorrect);
            Assert.Equal(expectedSum.ToString(), correct.OptionText);
        }
    }

    [Fact]
    public async Task SeedAsync_OrdersQuestions1Through30WithUniqueText()
    {
        var context = CreateDbContext();

        await SeedAsync(context);

        var exam = await context.Exams.SingleAsync(e => e.Slug == DevelopmentExamSeeder.ExamSlug);
        var version = await context.ExamVersions.SingleAsync(v => v.ExamId == exam.Id);
        var orders = await context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id)
            .OrderBy(q => q.DisplayOrder)
            .Select(q => q.DisplayOrder)
            .ToListAsync();

        Assert.Equal(Enumerable.Range(1, 30), orders);

        var texts = await context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id)
            .Select(q => q.QuestionText)
            .ToListAsync();
        Assert.Equal(texts.Count, texts.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_DoesNotDuplicateRows()
    {
        var context = CreateDbContext();

        await SeedAsync(context);
        await DevelopmentExamSeeder.SeedAsync(context);

        var examCount = await context.Exams.CountAsync(e => e.Slug == DevelopmentExamSeeder.ExamSlug);
        var exam = await context.Exams.SingleAsync(e => e.Slug == DevelopmentExamSeeder.ExamSlug);
        var versionCount = await context.ExamVersions.CountAsync(v => v.ExamId == exam.Id);
        var version = await context.ExamVersions.SingleAsync(v => v.ExamId == exam.Id);
        var questionCount = await context.ExamQuestions.CountAsync(q => q.ExamVersionId == version.Id);
        var questionIds = await context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id)
            .Select(q => q.Id)
            .ToListAsync();
        var optionCount = await context.ExamAnswerOptions.CountAsync(o => questionIds.Contains(o.ExamQuestionId));

        Assert.Equal(1, examCount);
        Assert.Equal(1, versionCount);
        Assert.Equal(30, questionCount);
        Assert.Equal(120, optionCount);
    }

    [Fact]
    public async Task SeedAsync_MeetsNormalSessionStartFlowInvariants()
    {
        var context = CreateDbContext();

        await SeedAsync(context);

        var exam = await context.Exams.SingleAsync(e => e.Slug == DevelopmentExamSeeder.ExamSlug);
        Assert.True(exam.IsFree);
        Assert.True(exam.DurationMinutes is >= 1 and <= 480);
        Assert.True(exam.PassingScorePercentage is >= 0 and <= 100);

        var hasPaidProduct = await context.PaymentProducts.AnyAsync(p => p.ExamId == exam.Id && p.IsActive);
        Assert.False(hasPaidProduct);

        Assert.NotNull(exam.ExamCategoryId);
        var category = await context.ExamCategories.SingleAsync(c => c.Id == exam.ExamCategoryId);
        Assert.True(category.IsActive);
        Assert.Equal(exam.CountryId, category.CountryId);

        var version = await context.ExamVersions.SingleAsync(v => v.ExamId == exam.Id);
        var questions = await context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id && q.IsActive)
            .ToListAsync();
        Assert.Equal(30, questions.Count);
        Assert.All(questions, q =>
        {
            Assert.Equal(ExamQuestionType.SingleBestAnswer, q.QuestionType);
            Assert.True(q.Points > 0);
        });

        var questionIds = questions.Select(q => q.Id).ToList();
        var options = await context.ExamAnswerOptions
            .Where(o => questionIds.Contains(o.ExamQuestionId) && o.IsActive)
            .ToListAsync();
        foreach (var question in questions)
        {
            var questionOptions = options.Where(o => o.ExamQuestionId == question.Id).ToList();
            Assert.True(questionOptions.Count >= 2);
            Assert.Equal(1, questionOptions.Count(o => o.IsCorrect));
        }
    }
}
