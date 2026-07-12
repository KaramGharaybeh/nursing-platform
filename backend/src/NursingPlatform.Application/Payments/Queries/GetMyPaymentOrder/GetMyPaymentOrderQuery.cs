using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;

namespace NursingPlatform.Application.Payments.Queries.GetMyPaymentOrder;

public class GetMyPaymentOrderQuery : IRequest<PaymentOrderDto>
{
    public Guid Id { get; set; }
}

public class GetMyPaymentOrderQueryValidator : AbstractValidator<GetMyPaymentOrderQuery>
{
    public GetMyPaymentOrderQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetMyPaymentOrderQueryHandler : IRequestHandler<GetMyPaymentOrderQuery, PaymentOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetMyPaymentOrderQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaymentOrderDto> Handle(GetMyPaymentOrderQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await PaymentHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var order = await _context.PaymentOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.NurseProfileId == nurseProfileId, cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException("Payment order was not found.");
        }

        if (order.ExpireIfPastDue(DateTime.UtcNow))
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        var items = await _context.PaymentOrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync(cancellationToken);

        return PaymentMapping.ToOrderDto(order, items);
    }
}
