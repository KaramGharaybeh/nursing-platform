using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Common;

internal static class ExamScoringService
{
    public static void FinalizeSession(
        ExamSession session,
        IReadOnlyCollection<ExamSessionQuestion> questions,
        IReadOnlyCollection<ExamSessionAnswerOption> options,
        IReadOnlyCollection<ExamSessionAnswer> answers,
        ExamSessionStatus status,
        decimal passingScorePercentage,
        DateTime finalizedAt)
    {
        var selectedByQuestion = answers.ToDictionary(a => a.ExamSessionQuestionId, a => a.SelectedExamSessionAnswerOptionId);
        var optionById = options.ToDictionary(o => o.Id);
        var score = 0;
        var correctCount = 0;
        var maxScore = questions.Sum(q => q.Points);

        foreach (var question in questions)
        {
            if (!selectedByQuestion.TryGetValue(question.Id, out var selectedOptionId)
                || !optionById.TryGetValue(selectedOptionId, out var selectedOption)
                || !selectedOption.IsCorrectSnapshot)
            {
                continue;
            }

            score += question.Points;
            correctCount++;
        }

        session.Status = status;
        session.Score = score;
        session.MaxScore = maxScore;
        session.Percentage = maxScore == 0
            ? 0
            : Math.Round(score * 100m / maxScore, 2, MidpointRounding.AwayFromZero);
        session.Passed = session.Percentage >= passingScorePercentage;
        session.CorrectCount = correctCount;
        session.QuestionCount = questions.Count;
        session.FinalizedAt = finalizedAt;

        if (status == ExamSessionStatus.Submitted)
        {
            session.SubmittedAt = finalizedAt;
        }
    }
}
