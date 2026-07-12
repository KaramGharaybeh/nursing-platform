using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Common;

internal static class ExamMapping
{
    public static ExamCatalogItemDto ToCatalogItem(
        Exam exam,
        ExamVersion version,
        string countryName,
        string? categoryName,
        bool canStart)
    {
        return new ExamCatalogItemDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            CountryId = exam.CountryId,
            CountryName = countryName,
            CategoryId = exam.ExamCategoryId,
            CategoryName = categoryName,
            DurationMinutes = exam.DurationMinutes,
            QuestionCount = version.QuestionCount,
            PassingScorePercentage = exam.PassingScorePercentage,
            IsFree = exam.IsFree,
            CanStart = canStart
        };
    }

    public static ExamDetailDto ToDetail(
        Exam exam,
        ExamVersion version,
        string countryName,
        string? categoryName,
        bool canStart)
    {
        return new ExamDetailDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            Instructions = exam.Instructions,
            CountryId = exam.CountryId,
            CountryName = countryName,
            CategoryId = exam.ExamCategoryId,
            CategoryName = categoryName,
            DurationMinutes = exam.DurationMinutes,
            QuestionCount = version.QuestionCount,
            PassingScorePercentage = exam.PassingScorePercentage,
            IsFree = exam.IsFree,
            CanStart = canStart
        };
    }

    public static ExamSessionDto ToSessionDto(
        ExamSession session,
        string examTitle,
        IReadOnlyCollection<ExamSessionQuestion> questions,
        IReadOnlyCollection<ExamSessionAnswerOption> options,
        IReadOnlyCollection<ExamSessionAnswer> answers,
        DateTime now)
    {
        var selectedByQuestion = answers.ToDictionary(a => a.ExamSessionQuestionId, a => (Guid?)a.SelectedExamSessionAnswerOptionId);
        var optionsByQuestion = options
            .GroupBy(o => o.ExamSessionQuestionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(o => o.DisplayOrder).ThenBy(o => o.Id).ToList());

        return new ExamSessionDto
        {
            Id = session.Id,
            ExamId = session.ExamId,
            ExamTitle = examTitle,
            Status = session.Status.ToString(),
            StartedAt = session.StartedAt,
            ExpiresAt = session.ExpiresAt,
            RemainingSeconds = Math.Max(0, (int)Math.Floor((session.ExpiresAt - now).TotalSeconds)),
            Items = questions
                .OrderBy(q => q.DisplayOrder)
                .ThenBy(q => q.Id)
                .Select(q => new ExamSessionQuestionDto
                {
                    Id = q.Id,
                    DisplayOrder = q.DisplayOrder,
                    Text = q.QuestionTextSnapshot,
                    Points = q.Points,
                    SelectedExamSessionAnswerOptionId = selectedByQuestion.GetValueOrDefault(q.Id),
                    Options = optionsByQuestion.GetValueOrDefault(q.Id, [])
                        .Select(o => new ExamSessionAnswerOptionDto
                        {
                            Id = o.Id,
                            DisplayOrder = o.DisplayOrder,
                            Text = o.OptionTextSnapshot
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public static ExamSessionResultDto ToResultDto(ExamSession session, string examTitle)
    {
        return new ExamSessionResultDto
        {
            Id = session.Id,
            ExamId = session.ExamId,
            ExamTitle = examTitle,
            Status = session.Status.ToString(),
            StartedAt = session.StartedAt,
            ExpiresAt = session.ExpiresAt,
            SubmittedAt = session.SubmittedAt,
            FinalizedAt = session.FinalizedAt,
            Score = session.Score,
            MaxScore = session.MaxScore,
            Percentage = session.Percentage,
            Passed = session.Passed,
            CorrectCount = session.CorrectCount,
            QuestionCount = session.QuestionCount
        };
    }

    public static ExamSessionReviewDto ToReviewDto(
        ExamSession session,
        string examTitle,
        IReadOnlyCollection<ExamSessionQuestion> questions,
        IReadOnlyCollection<ExamSessionAnswerOption> options,
        IReadOnlyCollection<ExamSessionAnswer> answers)
    {
        var selectedByQuestion = answers.ToDictionary(a => a.ExamSessionQuestionId, a => (Guid?)a.SelectedExamSessionAnswerOptionId);
        var optionsByQuestion = options
            .GroupBy(o => o.ExamSessionQuestionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(o => o.DisplayOrder).ThenBy(o => o.Id).ToList());

        return new ExamSessionReviewDto
        {
            Id = session.Id,
            ExamId = session.ExamId,
            ExamTitle = examTitle,
            Status = session.Status.ToString(),
            Score = session.Score,
            MaxScore = session.MaxScore,
            Percentage = session.Percentage,
            Passed = session.Passed,
            Items = questions
                .OrderBy(q => q.DisplayOrder)
                .ThenBy(q => q.Id)
                .Select(q =>
                {
                    var questionOptions = optionsByQuestion.GetValueOrDefault(q.Id, []);
                    var selected = selectedByQuestion.GetValueOrDefault(q.Id);
                    var correct = questionOptions.Single(o => o.IsCorrectSnapshot);

                    return new ExamSessionReviewQuestionDto
                    {
                        Id = q.Id,
                        DisplayOrder = q.DisplayOrder,
                        Text = q.QuestionTextSnapshot,
                        Explanation = q.ExplanationSnapshot,
                        Points = q.Points,
                        PointsEarned = selected == correct.Id ? q.Points : 0,
                        SelectedExamSessionAnswerOptionId = selected,
                        CorrectAnswerOptionId = correct.Id,
                        Options = questionOptions
                            .Select(o => new ExamSessionReviewOptionDto
                            {
                                Id = o.Id,
                                DisplayOrder = o.DisplayOrder,
                                Text = o.OptionTextSnapshot,
                                IsCorrect = o.IsCorrectSnapshot
                            })
                            .ToList()
                    };
                })
                .ToList()
        };
    }
}
