using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Commands.ClearExamSessionAnswer;

public class ClearExamSessionAnswerCommandHandler : IRequestHandler<ClearExamSessionAnswerCommand, ExamSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ClearExamSessionAnswerCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<ExamSessionDto> Handle(ClearExamSessionAnswerCommand request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var bundle = await ExamHandlerHelpers.GetOwnedSessionBundleAsync(_context, nurseProfileId, request.ExamSessionId, cancellationToken);
        var exam = await _context.Exams.FirstAsync(e => e.Id == bundle.Session.ExamId, cancellationToken);
        var now = DateTime.UtcNow;

        if (bundle.Session.Status != ExamSessionStatus.InProgress)
        {
            throw new InvalidOperationException("Only in-progress exam sessions can accept answers.");
        }

        if (now >= bundle.Session.ExpiresAt)
        {
            await ExamHandlerHelpers.FinalizeIfExpiredAsync(_context, bundle, exam.PassingScorePercentage, now, cancellationToken);
            throw new InvalidOperationException("Exam session has expired.");
        }

        var question = bundle.Questions.FirstOrDefault(q => q.Id == request.ExamSessionQuestionId);
        if (question is null)
        {
            throw new InvalidOperationException("Answer question does not belong to this session.");
        }

        var existing = bundle.Answers.FirstOrDefault(a => a.ExamSessionQuestionId == question.Id);
        if (existing is not null)
        {
            _context.ExamSessionAnswers.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var updatedBundle = await ExamHandlerHelpers.GetOwnedSessionBundleAsync(_context, nurseProfileId, request.ExamSessionId, cancellationToken);

        return ExamMapping.ToSessionDto(
            updatedBundle.Session,
            updatedBundle.ExamTitle,
            updatedBundle.Questions,
            updatedBundle.Options,
            updatedBundle.Answers,
            now);
    }
}
