using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;

public class CreateMyPaymentOrderCommand : IRequest<PaymentOrderDto>
{
    public CreatePaymentOrderRequest Request { get; set; } = new();
}

public class CreateMyPaymentOrderCommandValidator : AbstractValidator<CreateMyPaymentOrderCommand>
{
    public CreateMyPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Request.ProductId).NotEmpty();
    }
}

public class CreateMyPaymentOrderCommandHandler : IRequestHandler<CreateMyPaymentOrderCommand, PaymentOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public CreateMyPaymentOrderCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaymentOrderDto> Handle(CreateMyPaymentOrderCommand request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await PaymentHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);

        var row = await _context.PaymentProducts
            .Where(p => p.Id == request.Request.ProductId)
            .Join(_context.Exams,
                product => product.ExamId,
                exam => exam.Id,
                (product, exam) => new { product, exam })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new KeyNotFoundException("Payment product was not found.");
        }

        if (!row.product.IsActive)
        {
            throw new InvalidOperationException("Payment product is not active.");
        }

        if (row.exam.Status != ExamStatus.Published)
        {
            throw new InvalidOperationException("Payment product is not currently purchasable.");
        }

        var item = PaymentOrderItem.CreateSnapshot(row.product);
        var order = PaymentOrder.CreatePending(nurseProfileId, item, DateTime.UtcNow);

        _context.PaymentOrders.Add(order);
        _context.PaymentOrderItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return PaymentMapping.ToOrderDto(order, [item]);
    }
}
