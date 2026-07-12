using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Payments.Queries.ListPaymentProducts;

public class ListPaymentProductsQuery : IRequest<PaginatedResult<PaymentProductDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ExamId { get; set; }
}

public class ListPaymentProductsQueryValidator : AbstractValidator<ListPaymentProductsQuery>
{
    public ListPaymentProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ExamId).NotEqual(Guid.Empty).When(x => x.ExamId.HasValue);
    }
}

public class ListPaymentProductsQueryHandler : IRequestHandler<ListPaymentProductsQuery, PaginatedResult<PaymentProductDto>>
{
    private readonly IApplicationDbContext _context;

    public ListPaymentProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<PaymentProductDto>> Handle(ListPaymentProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PaymentProducts
            .Where(p => p.IsActive)
            .Join(_context.Exams.Where(e => e.Status == ExamStatus.Published),
                product => product.ExamId,
                exam => exam.Id,
                (product, exam) => new { product, exam });

        if (request.ExamId is not null)
        {
            query = query.Where(r => r.product.ExamId == request.ExamId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(r => r.product.Name)
            .ThenBy(r => r.product.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PaymentProductDto>
        {
            Items = rows.Select(r => PaymentMapping.ToProductDto(r.product, r.exam.Title)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
