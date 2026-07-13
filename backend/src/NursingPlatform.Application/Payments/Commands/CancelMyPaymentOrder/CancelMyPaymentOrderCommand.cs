using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Application.Payments.Commands.CancelMyPaymentOrder;

public class CancelMyPaymentOrderCommand : IRequest<PaymentOrderDto>
{
    public Guid Id { get; set; }
}

public class CancelMyPaymentOrderCommandValidator : AbstractValidator<CancelMyPaymentOrderCommand>
{
    public CancelMyPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class CancelMyPaymentOrderCommandHandler : IRequestHandler<CancelMyPaymentOrderCommand, PaymentOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public CancelMyPaymentOrderCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaymentOrderDto> Handle(CancelMyPaymentOrderCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var nurseProfileId = await PaymentHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var order = await _context.PaymentOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.NurseProfileId == nurseProfileId, cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException("Payment order was not found.");
        }

        var now = DateTime.UtcNow;

        if (order.ExpireIfPastDue(now))
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new InvalidOperationException("Payment order has expired.");
        }

        var checkoutSessions = await _context.PaymentCheckoutSessions
            .Where(s => s.PaymentOrderId == order.Id
                && s.NurseProfileId == nurseProfileId
                && (s.Status == PaymentCheckoutSessionStatus.Created
                    || s.Status == PaymentCheckoutSessionStatus.ProviderPending))
            .ToListAsync(cancellationToken);

        var hasActiveCheckout = checkoutSessions.Any(s => s.ExpiresAt > now);

        if (hasActiveCheckout)
        {
            throw new InvalidOperationException("CheckoutInProgress");
        }

        foreach (var staleCheckoutSession in checkoutSessions)
        {
            staleCheckoutSession.ExpireIfPastDue(now);
        }

        order.Cancel(now);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var items = await _context.PaymentOrderItems.Where(i => i.OrderId == order.Id).ToListAsync(cancellationToken);
        return PaymentMapping.ToOrderDto(order, items);
    }
}
