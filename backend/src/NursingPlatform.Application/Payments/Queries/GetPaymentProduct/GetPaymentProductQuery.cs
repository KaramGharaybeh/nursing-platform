using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Payments.Queries.GetPaymentProduct;

public class GetPaymentProductQuery : IRequest<PaymentProductDto>
{
    public Guid Id { get; set; }
}

public class GetPaymentProductQueryValidator : AbstractValidator<GetPaymentProductQuery>
{
    public GetPaymentProductQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetPaymentProductQueryHandler : IRequestHandler<GetPaymentProductQuery, PaymentProductDto>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentProductQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentProductDto> Handle(GetPaymentProductQuery request, CancellationToken cancellationToken)
    {
        var row = await _context.PaymentProducts
            .Where(p => p.Id == request.Id && p.IsActive)
            .Join(_context.Exams.Where(e => e.Status == ExamStatus.Published),
                product => product.ExamId,
                exam => exam.Id,
                (product, exam) => new { product, exam })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new KeyNotFoundException("Payment product was not found.");
        }

        return PaymentMapping.ToProductDto(row.product, row.exam.Title);
    }
}
