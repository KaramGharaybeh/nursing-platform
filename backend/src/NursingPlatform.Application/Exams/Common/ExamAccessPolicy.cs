using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Application.Exams.Common;

public class ExamAccessPolicy : IExamAccessPolicy
{
    private readonly IApplicationDbContext _context;
    private readonly Func<DateTime> _utcNow;

    public ExamAccessPolicy(IApplicationDbContext context)
        : this(context, () => DateTime.UtcNow)
    {
    }

    public ExamAccessPolicy(IApplicationDbContext context, Func<DateTime> utcNow)
    {
        _context = context;
        _utcNow = utcNow;
    }

    public async Task AuthorizeStartAsync(Guid nurseProfileId, Guid examId, CancellationToken cancellationToken)
    {
        var examAccessState = await _context.Exams
            .AsNoTracking()
            .Where(exam => exam.Id == examId)
            .Select(exam => new
            {
                exam.IsFree,
                HasActivePaidExamAccessProduct = _context.PaymentProducts
                    .AsNoTracking()
                    .Any(product => product.ExamId == exam.Id
                        && exam.Status == ExamStatus.Published
                        && product.Type == PaymentProductType.ExamAccess
                        && product.IsActive
                        && product.UnitAmountMinor > 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var requiresGrant = examAccessState is not null
            && (!examAccessState.IsFree || examAccessState.HasActivePaidExamAccessProduct);

        if (!requiresGrant)
        {
            return;
        }

        var now = _utcNow();
        var hasActiveGrant = await _context.ExamAccessGrants
            .AsNoTracking()
            .AnyAsync(grant => grant.NurseProfileId == nurseProfileId
                && grant.ExamId == examId
                && (grant.ExpiresAt == null || grant.ExpiresAt > now),
                cancellationToken);

        if (!hasActiveGrant)
        {
            throw new ForbiddenAccessException("Exam access is required.");
        }
    }
}
