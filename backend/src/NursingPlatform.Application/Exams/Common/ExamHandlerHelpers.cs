using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Common;

internal static class ExamHandlerHelpers
{
    public static async Task<Guid> GetCurrentNurseProfileIdAsync(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard,
        CancellationToken cancellationToken)
    {
        var userId = await nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (nurseProfileId is null)
        {
            throw new ForbiddenAccessException("Nurse profile is required before using exams.");
        }

        return nurseProfileId.Value;
    }

    public static async Task<ExamSessionBundle> GetOwnedSessionBundleAsync(
        IApplicationDbContext context,
        Guid nurseProfileId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var session = await context.ExamSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.NurseProfileId == nurseProfileId, cancellationToken);

        if (session is null)
        {
            throw new KeyNotFoundException("Exam session was not found.");
        }

        var examTitle = await context.Exams
            .Where(e => e.Id == session.ExamId)
            .Select(e => e.Title)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var questions = await context.ExamSessionQuestions
            .Where(q => q.ExamSessionId == session.Id)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.Id)
            .ToListAsync(cancellationToken);

        var questionIds = questions.Select(q => q.Id).ToList();
        var options = await context.ExamSessionAnswerOptions
            .Where(o => questionIds.Contains(o.ExamSessionQuestionId))
            .OrderBy(o => o.DisplayOrder)
            .ThenBy(o => o.Id)
            .ToListAsync(cancellationToken);

        var answers = await context.ExamSessionAnswers
            .Where(a => questionIds.Contains(a.ExamSessionQuestionId))
            .ToListAsync(cancellationToken);

        return new ExamSessionBundle(session, examTitle, questions, options, answers);
    }

    public static async Task FinalizeIfExpiredAsync(
        IApplicationDbContext context,
        ExamSessionBundle bundle,
        decimal passingScorePercentage,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (bundle.Session.Status != ExamSessionStatus.InProgress || now < bundle.Session.ExpiresAt)
        {
            return;
        }

        await FinalizeAsync(
            context,
            bundle,
            ExamSessionStatus.Expired,
            passingScorePercentage,
            now,
            cancellationToken);
    }

    public static async Task FinalizeAsync(
        IApplicationDbContext context,
        ExamSessionBundle bundle,
        ExamSessionStatus status,
        decimal passingScorePercentage,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        ExamScoringService.FinalizeSession(
            bundle.Session,
            bundle.Questions,
            bundle.Options,
            bundle.Answers,
            status,
            passingScorePercentage,
            timestamp);

        var affectedRows = await context.ExecuteExamSessionFinalizationAsync(
            bundle.Session.Id,
            bundle.Session.NurseProfileId,
            status,
            bundle.Session.Score,
            bundle.Session.MaxScore,
            bundle.Session.Percentage,
            bundle.Session.Passed,
            bundle.Session.CorrectCount,
            bundle.Session.QuestionCount,
            timestamp,
            cancellationToken);

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Exam session has already been finalized.");
        }
    }
}

internal sealed record ExamSessionBundle(
    ExamSession Session,
    string ExamTitle,
    IReadOnlyList<ExamSessionQuestion> Questions,
    IReadOnlyList<ExamSessionAnswerOption> Options,
    IReadOnlyList<ExamSessionAnswer> Answers);
