using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Application.Payments.Commands.CompleteSandboxPaymentCheckout;

public class CompleteSandboxPaymentCheckoutCommand : IRequest<PaymentCompletionDto>
{
    public Guid CheckoutSessionId { get; set; }
}

public class CompleteSandboxPaymentCheckoutCommandValidator : AbstractValidator<CompleteSandboxPaymentCheckoutCommand>
{
    public CompleteSandboxPaymentCheckoutCommandValidator()
    {
        RuleFor(x => x.CheckoutSessionId).NotEmpty();
    }
}

public class CompleteSandboxPaymentCheckoutCommandHandler : IRequestHandler<CompleteSandboxPaymentCheckoutCommand, PaymentCompletionDto>
{
    private const int MaxCompleteAttempts = 2;
    private const string SandboxProviderName = "Sandbox";
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public CompleteSandboxPaymentCheckoutCommandHandler(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaymentCompletionDto> Handle(
        CompleteSandboxPaymentCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var nurseProfileId = await PaymentHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);

        for (var attempt = 1; attempt <= MaxCompleteAttempts; attempt++)
        {
            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var session = await _context.PaymentCheckoutSessions
                .Include(s => s.PaymentOrder)
                .ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(s => s.Id == request.CheckoutSessionId && s.NurseProfileId == nurseProfileId, cancellationToken);

            ValidateOwnedSandboxProviderPendingSession(session, nurseProfileId);
            var order = session!.PaymentOrder;
            var examIds = GetPurchasedExamIds(order).ToArray();

            if (order.Status == PaymentOrderStatus.Paid)
            {
                await transaction.CommitAsync(cancellationToken);
                return await ReloadPaidCompletionDtoAsync(request.CheckoutSessionId, nurseProfileId, cancellationToken);
            }

            if (session.ExpiresAt <= now)
            {
                session.ExpireIfPastDue(now);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                throw new InvalidOperationException("Payment checkout session has expired.");
            }

            if (order.Status != PaymentOrderStatus.PendingPayment)
            {
                throw new InvalidOperationException("Only pending payment orders can be completed.");
            }

            var existingGrantExamIds = await LoadEffectiveGrantExamIdsAsync(nurseProfileId, examIds, cancellationToken);
            var missingGrantExamIds = examIds.Except(existingGrantExamIds).ToArray();

            foreach (var examId in missingGrantExamIds)
            {
                _context.ExamAccessGrants.Add(new ExamAccessGrant
                {
                    Id = Guid.NewGuid(),
                    NurseProfileId = nurseProfileId,
                    ExamId = examId,
                    GrantedAt = now,
                    ExpiresAt = null,
                    Reason = "SandboxPaymentCompletion"
                });
            }

            try
            {
                var paidRows = await _context.ExecutePaymentOrderPaidTransitionAsync(order.Id, nurseProfileId, now, cancellationToken);
                if (paidRows == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    DetachTrackedPaymentCompletionState();
                    var paidOutcome = await LoadPaidOutcomeAsync(request.CheckoutSessionId, nurseProfileId, cancellationToken);
                    if (paidOutcome is not null)
                    {
                        return ToCompletionDto(paidOutcome.Order, paidOutcome.GrantedExamIds);
                    }

                    throw new InvalidOperationException("Only pending payment orders can be completed.");
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return await ReloadPaidCompletionDtoAsync(request.CheckoutSessionId, nurseProfileId, cancellationToken);
            }
            catch (DbUpdateException exception) when (_context.IsUniqueEffectiveExamAccessGrantViolation(exception))
            {
                await transaction.RollbackAsync(cancellationToken);
                DetachTrackedPaymentCompletionState();

                var paidOutcome = await LoadPaidOutcomeAsync(request.CheckoutSessionId, nurseProfileId, cancellationToken);
                if (paidOutcome is not null)
                {
                    return ToCompletionDto(paidOutcome.Order, paidOutcome.GrantedExamIds);
                }

                var pendingOutcome = await LoadPendingOutcomeAsync(request.CheckoutSessionId, nurseProfileId, cancellationToken);
                if (pendingOutcome is not null
                    && pendingOutcome.RequiredExamIds.All(id => pendingOutcome.EffectiveGrantExamIds.Contains(id))
                    && attempt < MaxCompleteAttempts)
                {
                    continue;
                }

                throw new InvalidOperationException("Payment fulfillment could not be completed safely.", exception);
            }
            catch (DbUpdateException exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                DetachTrackedPaymentCompletionState();
                throw new InvalidOperationException("Payment fulfillment could not be completed safely.", exception);
            }
        }

        throw new InvalidOperationException("Payment fulfillment could not be completed safely.");
    }

    private static void ValidateOwnedSandboxProviderPendingSession(PaymentCheckoutSession? session, Guid nurseProfileId)
    {
        if (session is null || session.PaymentOrder.NurseProfileId != nurseProfileId)
        {
            throw new KeyNotFoundException("Payment checkout session was not found.");
        }

        if (!string.Equals(session.ProviderName, SandboxProviderName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Sandbox payment completion is only available for Sandbox checkout sessions.");
        }

        if (session.Status != PaymentCheckoutSessionStatus.ProviderPending)
        {
            throw new InvalidOperationException("Only provider-pending Sandbox checkout sessions can be completed.");
        }
    }

    private async Task<PaymentCompletionDto> ReloadPaidCompletionDtoAsync(
        Guid checkoutSessionId,
        Guid nurseProfileId,
        CancellationToken cancellationToken)
    {
        var paidOutcome = await LoadPaidOutcomeAsync(checkoutSessionId, nurseProfileId, cancellationToken);
        if (paidOutcome is null)
        {
            throw new InvalidOperationException("Payment fulfillment could not be completed safely.");
        }

        return ToCompletionDto(paidOutcome.Order, paidOutcome.GrantedExamIds);
    }

    private async Task<CompletionOutcome?> LoadPaidOutcomeAsync(
        Guid checkoutSessionId,
        Guid nurseProfileId,
        CancellationToken cancellationToken)
    {
        var session = await LoadOwnedSandboxProviderPendingSessionNoTrackingAsync(checkoutSessionId, nurseProfileId, cancellationToken);
        if (session?.PaymentOrder.Status != PaymentOrderStatus.Paid)
        {
            return null;
        }

        var requiredExamIds = GetPurchasedExamIds(session.PaymentOrder).ToArray();
        var effectiveGrantExamIds = await LoadEffectiveGrantExamIdsAsync(nurseProfileId, requiredExamIds, cancellationToken);
        if (!requiredExamIds.All(id => effectiveGrantExamIds.Contains(id)))
        {
            return null;
        }

        return new CompletionOutcome(session.PaymentOrder, requiredExamIds);
    }

    private async Task<PendingCompletionOutcome?> LoadPendingOutcomeAsync(
        Guid checkoutSessionId,
        Guid nurseProfileId,
        CancellationToken cancellationToken)
    {
        var session = await LoadOwnedSandboxProviderPendingSessionNoTrackingAsync(checkoutSessionId, nurseProfileId, cancellationToken);
        if (session?.PaymentOrder.Status != PaymentOrderStatus.PendingPayment)
        {
            return null;
        }

        var requiredExamIds = GetPurchasedExamIds(session.PaymentOrder).ToArray();
        var effectiveGrantExamIds = await LoadEffectiveGrantExamIdsAsync(nurseProfileId, requiredExamIds, cancellationToken);
        return new PendingCompletionOutcome(requiredExamIds, effectiveGrantExamIds);
    }

    private async Task<PaymentCheckoutSession?> LoadOwnedSandboxProviderPendingSessionNoTrackingAsync(
        Guid checkoutSessionId,
        Guid nurseProfileId,
        CancellationToken cancellationToken)
    {
        return await _context.PaymentCheckoutSessions
            .AsNoTracking()
            .Include(s => s.PaymentOrder)
            .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(s => s.Id == checkoutSessionId
                && s.NurseProfileId == nurseProfileId
                && s.PaymentOrder.NurseProfileId == nurseProfileId
                && s.ProviderName == SandboxProviderName
                && s.Status == PaymentCheckoutSessionStatus.ProviderPending,
                cancellationToken);
    }

    private async Task<IReadOnlyList<Guid>> LoadEffectiveGrantExamIdsAsync(
        Guid nurseProfileId,
        IReadOnlyCollection<Guid> examIds,
        CancellationToken cancellationToken)
    {
        if (examIds.Count == 0)
        {
            return [];
        }

        return await _context.ExamAccessGrants
            .AsNoTracking()
            .Where(g => g.NurseProfileId == nurseProfileId
                && g.ExpiresAt == null
                && examIds.Contains(g.ExamId))
            .Select(g => g.ExamId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static IEnumerable<Guid> GetPurchasedExamIds(PaymentOrder order)
    {
        return order.Items
            .Where(i => i.ProductTypeSnapshot == PaymentProductType.ExamAccess && i.ExamIdSnapshot != Guid.Empty)
            .Select(i => i.ExamIdSnapshot)
            .Distinct()
            .OrderBy(id => id);
    }

    private static PaymentCompletionDto ToCompletionDto(PaymentOrder order, IReadOnlyList<Guid> examIds)
    {
        return new PaymentCompletionDto
        {
            PaymentOrderId = order.Id,
            OrderStatus = order.Status.ToString(),
            PaidAt = order.PaidAt,
            GrantedExamIds = examIds
        };
    }

    private void DetachTrackedPaymentCompletionState()
    {
        if (_context is not DbContext dbContext)
        {
            return;
        }

        var entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.Entity is ExamAccessGrant or PaymentOrder or PaymentCheckoutSession)
            .ToList();

        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    private sealed record CompletionOutcome(PaymentOrder Order, IReadOnlyList<Guid> GrantedExamIds);
    private sealed record PendingCompletionOutcome(IReadOnlyList<Guid> RequiredExamIds, IReadOnlyList<Guid> EffectiveGrantExamIds);
}
