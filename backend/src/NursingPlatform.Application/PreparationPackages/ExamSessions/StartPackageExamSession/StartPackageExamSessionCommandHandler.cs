using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs;
using NursingPlatform.Application.PreparationPackages.ExamSessions.Exceptions;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.ExamSessions.StartPackageExamSession;

public sealed class StartPackageExamSessionCommandHandler : IRequestHandler<StartPackageExamSessionCommand, PackageExamSessionStartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public StartPackageExamSessionCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PackageExamSessionStartDto> Handle(StartPackageExamSessionCommand request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var now = DateTime.UtcNow;

        var entitlement = await _context.PackagePurchaseEntitlements
            .Include(e => e.Rights)
            .FirstOrDefaultAsync(e => e.Id == request.EntitlementId && e.NurseProfileId == nurseProfileId, cancellationToken);

        if (entitlement is null)
        {
            throw new KeyNotFoundException("Package entitlement was not found.");
        }

        var sameSourceSession = await GetSameSourceSessionAsync(entitlement, nurseProfileId, cancellationToken);
        if (sameSourceSession is not null)
        {
            if (sameSourceSession.Status == ExamSessionStatus.InProgress && now < sameSourceSession.ExpiresAt)
            {
                return await CreateResponseAsync(entitlement, sameSourceSession, now, cancellationToken);
            }

            if (sameSourceSession.Status == ExamSessionStatus.InProgress)
            {
                var bundle = await ExamHandlerHelpers.GetOwnedSessionBundleAsync(_context, nurseProfileId, sameSourceSession.Id, cancellationToken);
                var examForFinalization = await _context.Exams.FirstAsync(e => e.Id == sameSourceSession.ExamId, cancellationToken);
                try
                {
                    await ExamHandlerHelpers.FinalizeIfExpiredAsync(_context, bundle, examForFinalization.PassingScorePercentage, now, cancellationToken);
                }
                catch (InvalidOperationException)
                {
                    throw Conflict("package-attempt-consumed", "Package exam attempt has already been consumed.");
                }
            }

            throw Conflict("package-attempt-consumed", "Package exam attempt has already been consumed.");
        }

        if (!entitlement.IsActiveAt(now))
        {
            throw Conflict("package-entitlement-inactive", "Package entitlement is not active for exam attempt start.");
        }

        var attemptRight = entitlement.Rights.SingleOrDefault(r => r.RightType == PackageBenefitRightType.PackageExamAttemptEligibility);
        if (attemptRight is null)
        {
            throw Conflict("package-attempt-right-missing", "Package exam attempt right is missing.");
        }

        if (attemptRight.Status != PackageBenefitRightStatus.Available)
        {
            throw Conflict("package-attempt-consumed", "Package exam attempt has already been consumed.");
        }

        if (attemptRight.AccessStartsAt > now || (attemptRight.AccessEndsAt.HasValue && now >= attemptRight.AccessEndsAt.Value))
        {
            throw Conflict("package-entitlement-inactive", "Package exam attempt right is outside its access window.");
        }

        var exam = await _context.Exams
            .FirstOrDefaultAsync(e => e.Id == entitlement.IncludedExamId && e.Status == ExamStatus.Published, cancellationToken);
        var version = await _context.ExamVersions
            .FirstOrDefaultAsync(v => v.Id == entitlement.IncludedExamVersionId
                && v.ExamId == entitlement.IncludedExamId
                && v.Status == ExamVersionStatus.Published,
                cancellationToken);

        if (exam is null || version is null)
        {
            throw Conflict("package-exam-version-unavailable", "Package exam version is not available for start.");
        }

        var existing = await _context.ExamSessions
            .Where(s => s.NurseProfileId == nurseProfileId
                && s.ExamVersionId == entitlement.IncludedExamVersionId
                && s.Status == ExamSessionStatus.InProgress)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            throw Conflict("exam-session-source-conflict", "An in-progress exam session already exists for this exam version.");
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

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            var session = ExamSession.Create(
                nurseProfileId,
                entitlement.IncludedExamId,
                entitlement.IncludedExamVersionId,
                now,
                exam.DurationMinutes,
                ExamSessionSource.PackageAttempt);
            _context.ExamSessions.Add(session);

            _context.ExamSessionProvenances.Add(ExamSessionProvenance.CreateForPackageAttempt(
                session.Id,
                entitlement.Id,
                attemptRight.Id,
                entitlement.PurchasedOfferSnapshotId,
                entitlement.PaymentOrderId,
                entitlement.PaymentOrderItemId,
                entitlement.PreparationPackageDefinitionId,
                entitlement.PreparationPackageVersionId,
                entitlement.PreparationPackageOfferId,
                entitlement.IncludedExamId,
                entitlement.IncludedExamVersionId,
                entitlement.ReportingProfilePublicationId,
                entitlement.PracticeCollectionVersionId,
                entitlement.AccessStartsAt,
                entitlement.AccessEndsAt,
                now));

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

            attemptRight.ConsumePackageExamAttempt(now);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await CreateResponseAsync(entitlement, session, now, cancellationToken);
        }
        catch (DbUpdateException exception) when (_context.IsUniqueInProgressExamSessionViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return await ResolveInProgressSessionRaceAsync(entitlement, nurseProfileId, now, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static PackageExamSessionConflictException Conflict(string code, string message)
    {
        return new PackageExamSessionConflictException(code, message);
    }

    private async Task<ExamSession?> GetSameSourceSessionAsync(
        PackagePurchaseEntitlement entitlement,
        Guid nurseProfileId,
        CancellationToken cancellationToken)
    {
        var sameSourceSessionId = await _context.ExamSessionProvenances
            .Where(p => p.PackagePurchaseEntitlementId == entitlement.Id
                && p.IncludedExamVersionId == entitlement.IncludedExamVersionId)
            .Select(p => (Guid?)p.ExamSessionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (sameSourceSessionId is null)
        {
            return null;
        }

        return await _context.ExamSessions
            .FirstOrDefaultAsync(s => s.Id == sameSourceSessionId.Value
                && s.NurseProfileId == nurseProfileId
                && s.ExamVersionId == entitlement.IncludedExamVersionId
                && s.Source == ExamSessionSource.PackageAttempt,
                cancellationToken);
    }

    private async Task<PackageExamSessionStartDto> ResolveInProgressSessionRaceAsync(
        PackagePurchaseEntitlement entitlement,
        Guid nurseProfileId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var existing = await _context.ExamSessions
            .AsNoTracking()
            .Where(s => s.NurseProfileId == nurseProfileId
                && s.ExamVersionId == entitlement.IncludedExamVersionId
                && s.Status == ExamSessionStatus.InProgress)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null || existing.Source != ExamSessionSource.PackageAttempt || now >= existing.ExpiresAt)
        {
            throw Conflict("exam-session-source-conflict", "An in-progress exam session already exists for this exam version.");
        }

        var hasMatchingProvenance = await _context.ExamSessionProvenances
            .AsNoTracking()
            .AnyAsync(p => p.ExamSessionId == existing.Id
                && p.PackagePurchaseEntitlementId == entitlement.Id
                && p.IncludedExamVersionId == entitlement.IncludedExamVersionId,
                cancellationToken);

        if (!hasMatchingProvenance)
        {
            throw Conflict("exam-session-source-conflict", "An in-progress exam session already exists for this exam version.");
        }

        var durableSession = await _context.ExamSessions
            .FirstAsync(s => s.Id == existing.Id, cancellationToken);

        return await CreateResponseAsync(entitlement, durableSession, now, cancellationToken);
    }

    private async Task<PackageExamSessionStartDto> CreateResponseAsync(
        PackagePurchaseEntitlement entitlement,
        ExamSession session,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var bundle = await ExamHandlerHelpers.GetOwnedSessionBundleAsync(_context, entitlement.NurseProfileId, session.Id, cancellationToken);
        var examTitle = await _context.Exams
            .Where(e => e.Id == entitlement.IncludedExamId)
            .Select(e => e.Title)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return new PackageExamSessionStartDto
        {
            Session = ExamMapping.ToSessionDto(bundle.Session, examTitle, bundle.Questions, bundle.Options, bundle.Answers, now),
            Source = ExamSessionSource.PackageAttempt.ToString(),
            EntitlementId = entitlement.Id,
            IncludedExamId = entitlement.IncludedExamId,
            IncludedExamVersionId = entitlement.IncludedExamVersionId,
            PreparationPackageOfferId = entitlement.PreparationPackageOfferId,
            AccessEndsAt = entitlement.AccessEndsAt
        };
    }

    private static void ValidatePublishedContent(IReadOnlyCollection<ExamQuestion> questions)
    {
        if (questions.Count == 0
            || questions.Any(q => q.QuestionType != ExamQuestionType.SingleBestAnswer)
            || questions.Any(q => q.Points <= 0))
        {
            throw Conflict("package-exam-version-unavailable", "Package exam content is not startable.");
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
                throw Conflict("package-exam-version-unavailable", "Package exam content is not startable.");
            }
        }
    }
}
