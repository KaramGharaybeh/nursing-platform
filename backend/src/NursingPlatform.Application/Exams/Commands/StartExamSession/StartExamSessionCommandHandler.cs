using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Commands.StartExamSession;

public class StartExamSessionCommandHandler : IRequestHandler<StartExamSessionCommand, ExamSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public StartExamSessionCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<ExamSessionDto> Handle(StartExamSessionCommand request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var now = DateTime.UtcNow;

        var exam = await _context.Exams
            .FirstOrDefaultAsync(e => e.Id == request.ExamId && e.Status == ExamStatus.Published, cancellationToken);

        if (exam is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        var version = await _context.ExamVersions
            .Where(v => v.ExamId == exam.Id && v.Status == ExamVersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (version is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        if (!exam.IsFree)
        {
            var hasGrant = await _context.ExamAccessGrants.AnyAsync(g =>
                    g.NurseProfileId == nurseProfileId
                    && g.ExamId == exam.Id
                    && (g.ExpiresAt == null || g.ExpiresAt > now),
                cancellationToken);

            if (!hasGrant)
            {
                throw new ForbiddenAccessException("Exam access is required.");
            }
        }

        var existing = await _context.ExamSessions
            .Where(s => s.NurseProfileId == nurseProfileId
                && s.ExamVersionId == version.Id
                && s.Status == ExamSessionStatus.InProgress)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            var existingBundle = await ExamHandlerHelpers.GetOwnedSessionBundleAsync(_context, nurseProfileId, existing.Id, cancellationToken);
            if (now < existing.ExpiresAt)
            {
                return ExamMapping.ToSessionDto(
                    existingBundle.Session,
                    existingBundle.ExamTitle,
                    existingBundle.Questions,
                    existingBundle.Options,
                    existingBundle.Answers,
                    now);
            }

            await ExamHandlerHelpers.FinalizeIfExpiredAsync(_context, existingBundle, exam.PassingScorePercentage, now, cancellationToken);
        }

        var questions = await _context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id && q.IsActive)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.Id)
            .ToListAsync(cancellationToken);

        ValidatePublishedContent(questions);

        var questionIds = questions.Select(q => q.Id).ToList();
        var options = await _context.ExamAnswerOptions
            .Where(o => questionIds.Contains(o.ExamQuestionId) && o.IsActive)
            .OrderBy(o => o.DisplayOrder)
            .ThenBy(o => o.Id)
            .ToListAsync(cancellationToken);

        ValidatePublishedOptions(questions, options);

        var session = ExamSession.Create(nurseProfileId, exam.Id, version.Id, now, exam.DurationMinutes);
        _context.ExamSessions.Add(session);

        foreach (var question in questions)
        {
            var sessionQuestion = new ExamSessionQuestion
            {
                Id = Guid.NewGuid(),
                ExamSessionId = session.Id,
                ExamQuestionId = question.Id,
                DisplayOrder = question.DisplayOrder,
                QuestionTextSnapshot = question.QuestionText,
                ExplanationSnapshot = question.Explanation,
                Points = question.Points
            };
            _context.ExamSessionQuestions.Add(sessionQuestion);

            foreach (var option in options.Where(o => o.ExamQuestionId == question.Id).OrderBy(o => o.DisplayOrder).ThenBy(o => o.Id))
            {
                _context.ExamSessionAnswerOptions.Add(new ExamSessionAnswerOption
                {
                    Id = Guid.NewGuid(),
                    ExamSessionQuestionId = sessionQuestion.Id,
                    ExamAnswerOptionId = option.Id,
                    DisplayOrder = option.DisplayOrder,
                    OptionTextSnapshot = option.OptionText,
                    IsCorrectSnapshot = option.IsCorrect
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        var bundle = await ExamHandlerHelpers.GetOwnedSessionBundleAsync(_context, nurseProfileId, session.Id, cancellationToken);

        return ExamMapping.ToSessionDto(bundle.Session, exam.Title, bundle.Questions, bundle.Options, bundle.Answers, now);
    }

    private static void ValidatePublishedContent(IReadOnlyCollection<ExamQuestion> questions)
    {
        if (questions.Count == 0
            || questions.Any(q => q.QuestionType != ExamQuestionType.SingleBestAnswer)
            || questions.Any(q => q.Points <= 0))
        {
            throw new InvalidOperationException("Published exam content is not startable.");
        }
    }

    private static void ValidatePublishedOptions(
        IReadOnlyCollection<ExamQuestion> questions,
        IReadOnlyCollection<ExamAnswerOption> options)
    {
        foreach (var question in questions)
        {
            var activeOptions = options.Where(o => o.ExamQuestionId == question.Id).ToList();
            if (activeOptions.Count < 2 || activeOptions.Count(o => o.IsCorrect) != 1)
            {
                throw new InvalidOperationException("Published exam content is not startable.");
            }
        }
    }
}
