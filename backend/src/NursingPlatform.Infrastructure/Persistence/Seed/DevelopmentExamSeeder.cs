using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Infrastructure.Persistence.Seed;

/// <summary>
/// Development-only seed for a single free practice exam with simple addition
/// questions, used to exercise the exam session UI and navigation locally.
/// Runs after <see cref="ReferenceDataSeeder"/> (which owns countries) and is
/// invoked only when the host environment is Development. Never runs in
/// Production or Test so automated-suite baselines stay untouched.
/// Idempotent: re-running leaves exactly one exam, one published version and
/// exactly 30 canonical questions (no duplicate rows on restart).
/// </summary>
public static class DevelopmentExamSeeder
{
    public const string ExamSlug = "dev-addition-practice";

    private const string CategorySlug = "dev-practice";
    private const int ExpectedQuestionCount = 30;
    private const int PointsPerQuestion = 1;

    // Canonical operand pairs: simple positive whole numbers, unique sums,
    // gradually increasing in size. Question text is always "A + B = ?".
    private static readonly (int A, int B)[] QuestionOperands =
    [
        (1, 1), (2, 3), (3, 4), (4, 5), (5, 5),
        (3, 9), (7, 6), (6, 9), (10, 6), (9, 8),
        (8, 11), (12, 8), (13, 9), (11, 14), (9, 18),
        (15, 13), (12, 19), (16, 17), (14, 22), (18, 21),
        (23, 19), (25, 22), (30, 21), (27, 28), (33, 26),
        (35, 29), (41, 28), (38, 36), (45, 33), (52, 41),
    ];

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var country = await context.Countries.FirstOrDefaultAsync(c => c.Code == "US");
        if (country is null)
        {
            throw new InvalidOperationException(
                "Development exam seeding requires the 'US' reference country. Run ReferenceDataSeeder first.");
        }

        var category = await context.ExamCategories
            .FirstOrDefaultAsync(c => c.CountryId == country.Id && c.Slug == CategorySlug);
        if (category is null)
        {
            category = new ExamCategory
            {
                Id = Guid.NewGuid(),
                CountryId = country.Id,
                Name = "Development Practice",
                Slug = CategorySlug,
                Description = "Development-only category for seeded practice content.",
                DisplayOrder = 0,
                IsActive = true,
            };
            context.ExamCategories.Add(category);
        }

        var now = DateTime.UtcNow;

        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Slug == ExamSlug);
        if (exam is null)
        {
            exam = new Exam
            {
                Id = Guid.NewGuid(),
                CountryId = country.Id,
                ExamCategoryId = category.Id,
                Title = "Development Addition Practice",
                Slug = ExamSlug,
                Description = "Development-only seed exam with simple addition questions for exercising the exam session UI. Not for production use.",
                Instructions = "Answer each addition question.",
                DurationMinutes = 60,
                PassingScorePercentage = 60,
                Status = ExamStatus.Published,
                IsFree = true,
                PublishedAt = now,
            };
            context.Exams.Add(exam);
        }
        else
        {
            exam.CountryId = country.Id;
            exam.ExamCategoryId = category.Id;
            exam.Status = ExamStatus.Published;
            exam.IsFree = true;
            exam.PublishedAt ??= now;
        }

        var version = await context.ExamVersions
            .FirstOrDefaultAsync(v => v.ExamId == exam.Id && v.VersionNumber == 1);
        if (version is null)
        {
            version = new ExamVersion
            {
                Id = Guid.NewGuid(),
                ExamId = exam.Id,
                VersionNumber = 1,
                Status = ExamVersionStatus.Published,
                QuestionCount = ExpectedQuestionCount,
                TotalPoints = ExpectedQuestionCount * PointsPerQuestion,
                PublishedAt = now,
            };
            context.ExamVersions.Add(version);
        }
        else
        {
            version.Status = ExamVersionStatus.Published;
            version.PublishedAt ??= now;
        }

        var existingQuestions = await context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.Id)
            .ToListAsync();
        var existingQuestionIds = existingQuestions.Select(q => q.Id).ToList();
        var existingOptions = await context.ExamAnswerOptions
            .Where(o => existingQuestionIds.Contains(o.ExamQuestionId))
            .ToListAsync();

        if (MatchesCanonicalContent(existingQuestions, existingOptions))
        {
            return;
        }

        // Reconcile to exactly the canonical 30 questions. Removal is safe on a
        // fresh Development database; if live sessions already snapshot these
        // questions the database restrict rules will reject the delete and
        // surface an error instead of silently corrupting session history.
        context.ExamAnswerOptions.RemoveRange(existingOptions);
        context.ExamQuestions.RemoveRange(existingQuestions);

        for (var index = 0; index < QuestionOperands.Length; index++)
        {
            var order = index + 1;
            var (a, b) = QuestionOperands[index];
            var sum = a + b;

            var question = new ExamQuestion
            {
                Id = Guid.NewGuid(),
                ExamVersionId = version.Id,
                QuestionText = $"{a} + {b} = ?",
                Explanation = $"Adding {a} and {b} gives {sum}.",
                QuestionType = ExamQuestionType.SingleBestAnswer,
                Points = PointsPerQuestion,
                DisplayOrder = order,
                IsActive = true,
            };
            context.ExamQuestions.Add(question);

            var optionOrder = 1;
            foreach (var (optionText, isCorrect) in BuildOptions(order, sum))
            {
                context.ExamAnswerOptions.Add(new ExamAnswerOption
                {
                    Id = Guid.NewGuid(),
                    ExamQuestionId = question.Id,
                    OptionText = optionText,
                    DisplayOrder = optionOrder++,
                    IsCorrect = isCorrect,
                    IsActive = true,
                });
            }
        }

        version.QuestionCount = ExpectedQuestionCount;
        version.TotalPoints = ExpectedQuestionCount * PointsPerQuestion;

        await context.SaveChangesAsync();
    }

    // Four ascending numeric options per question: the correct sum plus three
    // plausible neighbours. Odd-ordered questions place the correct answer
    // second, even-ordered third, so the correct slot varies.
    private static IEnumerable<(string Text, bool IsCorrect)> BuildOptions(int displayOrder, int sum)
    {
        int[] neighbours = displayOrder % 2 == 1 || sum - 2 < 1
            ? [sum - 1, sum + 1, sum + 2]
            : [sum - 2, sum - 1, sum + 1];

        return neighbours
            .Append(sum)
            .Distinct()
            .OrderBy(value => value)
            .Select(value => (value.ToString(), value == sum));
    }

    private static bool MatchesCanonicalContent(
        List<ExamQuestion> existingQuestions,
        List<ExamAnswerOption> existingOptions)
    {
        if (existingQuestions.Count != QuestionOperands.Length)
        {
            return false;
        }

        for (var index = 0; index < QuestionOperands.Length; index++)
        {
            var order = index + 1;
            var (a, b) = QuestionOperands[index];
            var sum = a + b;
            var question = existingQuestions[index];

            if (question.DisplayOrder != order
                || !string.Equals(question.QuestionText, $"{a} + {b} = ?", StringComparison.Ordinal)
                || question.QuestionType != ExamQuestionType.SingleBestAnswer
                || question.Points <= 0
                || !question.IsActive)
            {
                return false;
            }

            var expectedOptions = BuildOptions(order, sum).ToList();
            var actualOptions = existingOptions
                .Where(o => o.ExamQuestionId == question.Id && o.IsActive)
                .OrderBy(o => o.DisplayOrder)
                .ThenBy(o => o.Id)
                .ToList();

            if (actualOptions.Count != expectedOptions.Count)
            {
                return false;
            }

            for (var optionIndex = 0; optionIndex < expectedOptions.Count; optionIndex++)
            {
                if (!string.Equals(actualOptions[optionIndex].OptionText, expectedOptions[optionIndex].Text, StringComparison.Ordinal)
                    || actualOptions[optionIndex].IsCorrect != expectedOptions[optionIndex].IsCorrect)
                {
                    return false;
                }
            }

            if (actualOptions.Count(o => o.IsCorrect) != 1)
            {
                return false;
            }
        }

        return true;
    }
}
