using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Application.Payments.Queries.ListMyPaymentOrders;

public class ListMyPaymentOrdersQuery : IRequest<PaginatedResult<PaymentOrderDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public PaymentOrderStatus? Status { get; set; }
}

public class ListMyPaymentOrdersQueryValidator : AbstractValidator<ListMyPaymentOrdersQuery>
{
    public ListMyPaymentOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}

public class ListMyPaymentOrdersQueryHandler : IRequestHandler<ListMyPaymentOrdersQuery, PaginatedResult<PaymentOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListMyPaymentOrdersQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaginatedResult<PaymentOrderDto>> Handle(ListMyPaymentOrdersQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await PaymentHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var now = DateTime.UtcNow;

        var ownedOrders = await _context.PaymentOrders
            .Where(o => o.NurseProfileId == nurseProfileId)
            .ToListAsync(cancellationToken);

        var changed = false;
        foreach (var order in ownedOrders)
        {
            changed |= order.ExpireIfPastDue(now);
        }

        if (changed)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        var filtered = ownedOrders.AsQueryable();
        if (request.Status is not null)
        {
            filtered = filtered.Where(o => o.Status == request.Status);
        }

        var ordered = filtered
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .ToList();

        var pageOrders = ordered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        var orderIds = pageOrders.Select(o => o.Id).ToList();
        var items = await _context.PaymentOrderItems
            .Where(i => orderIds.Contains(i.OrderId))
            .ToListAsync(cancellationToken);
        var itemsByOrder = items.GroupBy(i => i.OrderId).ToDictionary(g => g.Key, g => (IReadOnlyCollection<PaymentOrderItem>)g.ToList());

        return new PaginatedResult<PaymentOrderDto>
        {
            Items = pageOrders.Select(o => PaymentMapping.ToOrderDto(o, itemsByOrder.GetValueOrDefault(o.Id, []))).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = ordered.Count
        };
    }
}
