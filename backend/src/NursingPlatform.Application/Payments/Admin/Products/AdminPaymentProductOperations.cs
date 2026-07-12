using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Application.Payments.Admin.Products;

public class CreateAdminPaymentProductRequest
{
    public PaymentProductType Type { get; set; } = PaymentProductType.ExamAccess;
    public Guid ExamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long UnitAmountMinor { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAdminPaymentProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long UnitAmountMinor { get; set; }
}

public class ListAdminPaymentProductsQuery : IRequest<PaginatedResult<PaymentProductDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ExamId { get; set; }
    public bool? IsActive { get; set; }
}

public class GetAdminPaymentProductQuery : IRequest<PaymentProductDto>
{
    public Guid Id { get; set; }
}

public class CreateAdminPaymentProductCommand : IRequest<PaymentProductDto>
{
    public CreateAdminPaymentProductRequest Request { get; set; } = new();
}

public class UpdateAdminPaymentProductCommand : IRequest<PaymentProductDto>
{
    public Guid Id { get; set; }
    public UpdateAdminPaymentProductRequest Request { get; set; } = new();
}

public class ArchiveAdminPaymentProductCommand : IRequest<PaymentProductDto>
{
    public Guid Id { get; set; }
}

public class RestoreAdminPaymentProductCommand : IRequest<PaymentProductDto>
{
    public Guid Id { get; set; }
}

public class ListAdminPaymentProductsQueryValidator : AbstractValidator<ListAdminPaymentProductsQuery>
{
    public ListAdminPaymentProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ExamId).NotEqual(Guid.Empty).When(x => x.ExamId.HasValue);
    }
}

public class GetAdminPaymentProductQueryValidator : AbstractValidator<GetAdminPaymentProductQuery>
{
    public GetAdminPaymentProductQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class CreateAdminPaymentProductCommandValidator : AbstractValidator<CreateAdminPaymentProductCommand>
{
    public CreateAdminPaymentProductCommandValidator()
    {
        RuleFor(x => x.Request.Type).Equal(PaymentProductType.ExamAccess);
        RuleFor(x => x.Request.ExamId).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(1000);
        RuleFor(x => x.Request.Currency).ValidCurrency();
        RuleFor(x => x.Request.UnitAmountMinor).GreaterThan(0);
    }
}

public class UpdateAdminPaymentProductCommandValidator : AbstractValidator<UpdateAdminPaymentProductCommand>
{
    public UpdateAdminPaymentProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(1000);
        RuleFor(x => x.Request.Currency).ValidCurrency();
        RuleFor(x => x.Request.UnitAmountMinor).GreaterThan(0);
    }
}

public class ArchiveAdminPaymentProductCommandValidator : AbstractValidator<ArchiveAdminPaymentProductCommand>
{
    public ArchiveAdminPaymentProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class RestoreAdminPaymentProductCommandValidator : AbstractValidator<RestoreAdminPaymentProductCommand>
{
    public RestoreAdminPaymentProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class ListAdminPaymentProductsQueryHandler : IRequestHandler<ListAdminPaymentProductsQuery, PaginatedResult<PaymentProductDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminPaymentProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<PaymentProductDto>> Handle(ListAdminPaymentProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PaymentProducts.AsQueryable();
        if (request.ExamId is not null)
        {
            query = query.Where(p => p.ExamId == request.ExamId);
        }

        if (request.IsActive is not null)
        {
            query = query.Where(p => p.IsActive == request.IsActive);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .Join(_context.Exams,
                product => product.ExamId,
                exam => exam.Id,
                (product, exam) => new { product, exam })
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

public class GetAdminPaymentProductQueryHandler : IRequestHandler<GetAdminPaymentProductQuery, PaymentProductDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminPaymentProductQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentProductDto> Handle(GetAdminPaymentProductQuery request, CancellationToken cancellationToken)
    {
        var row = await LoadProductRowAsync(_context, request.Id, cancellationToken);
        return PaymentMapping.ToProductDto(row.Product, row.ExamTitle);
    }

    internal static async Task<(PaymentProduct Product, string ExamTitle)> LoadProductRowAsync(
        IApplicationDbContext context,
        Guid id,
        CancellationToken cancellationToken)
    {
        var row = await context.PaymentProducts
            .Where(p => p.Id == id)
            .Join(context.Exams,
                product => product.ExamId,
                exam => exam.Id,
                (product, exam) => new { product, exam })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new KeyNotFoundException("Payment product was not found.");
        }

        return (row.product, row.exam.Title);
    }
}

public class CreateAdminPaymentProductCommandHandler : IRequestHandler<CreateAdminPaymentProductCommand, PaymentProductDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminPaymentProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentProductDto> Handle(CreateAdminPaymentProductCommand request, CancellationToken cancellationToken)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.Request.ExamId, cancellationToken);
        if (exam is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        if (exam.Status != ExamStatus.Published)
        {
            throw new InvalidOperationException("Payment products can only be created for published exams.");
        }

        var duplicate = await _context.PaymentProducts.AnyAsync(
            p => p.Type == PaymentProductType.ExamAccess && p.ExamId == request.Request.ExamId,
            cancellationToken);
        if (duplicate)
        {
            throw new InvalidOperationException("An exam access payment product already exists for this exam.");
        }

        var product = PaymentProduct.CreateExamAccess(
            request.Request.ExamId,
            request.Request.Name,
            request.Request.Description,
            PaymentMoneyValidator.NormalizeCurrency(request.Request.Currency),
            request.Request.UnitAmountMinor,
            request.Request.IsActive);

        _context.PaymentProducts.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return PaymentMapping.ToProductDto(product, exam.Title);
    }
}

public class UpdateAdminPaymentProductCommandHandler : IRequestHandler<UpdateAdminPaymentProductCommand, PaymentProductDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAdminPaymentProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentProductDto> Handle(UpdateAdminPaymentProductCommand request, CancellationToken cancellationToken)
    {
        var row = await GetAdminPaymentProductQueryHandler.LoadProductRowAsync(_context, request.Id, cancellationToken);
        row.Product.Name = request.Request.Name.Trim();
        row.Product.Description = string.IsNullOrWhiteSpace(request.Request.Description) ? null : request.Request.Description.Trim();
        row.Product.Currency = PaymentMoneyValidator.NormalizeCurrency(request.Request.Currency);
        row.Product.UnitAmountMinor = request.Request.UnitAmountMinor;

        await _context.SaveChangesAsync(cancellationToken);
        return PaymentMapping.ToProductDto(row.Product, row.ExamTitle);
    }
}

public class ArchiveAdminPaymentProductCommandHandler : IRequestHandler<ArchiveAdminPaymentProductCommand, PaymentProductDto>
{
    private readonly IApplicationDbContext _context;

    public ArchiveAdminPaymentProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentProductDto> Handle(ArchiveAdminPaymentProductCommand request, CancellationToken cancellationToken)
    {
        var row = await GetAdminPaymentProductQueryHandler.LoadProductRowAsync(_context, request.Id, cancellationToken);
        row.Product.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return PaymentMapping.ToProductDto(row.Product, row.ExamTitle);
    }
}

public class RestoreAdminPaymentProductCommandHandler : IRequestHandler<RestoreAdminPaymentProductCommand, PaymentProductDto>
{
    private readonly IApplicationDbContext _context;

    public RestoreAdminPaymentProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentProductDto> Handle(RestoreAdminPaymentProductCommand request, CancellationToken cancellationToken)
    {
        var row = await _context.PaymentProducts
            .Where(p => p.Id == request.Id)
            .Join(_context.Exams,
                product => product.ExamId,
                exam => exam.Id,
                (product, exam) => new { product, exam })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new KeyNotFoundException("Payment product was not found.");
        }

        if (row.exam.Status != ExamStatus.Published)
        {
            throw new InvalidOperationException("Payment product can only be restored when its exam is published.");
        }

        row.product.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);
        return PaymentMapping.ToProductDto(row.product, row.exam.Title);
    }
}
