using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Admin.Common;
using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Admin.Categories;

public class CreateAdminExamCategoryRequest
{
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateAdminExamCategoryRequest
{
    public Guid? CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public class ListAdminExamCategoriesQuery : IRequest<PaginatedResult<AdminExamCategoryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CountryId { get; set; }
    public bool? IsActive { get; set; }
}

public class GetAdminExamCategoryQuery : IRequest<AdminExamCategoryDto>
{
    public Guid Id { get; set; }
}

public class CreateAdminExamCategoryCommand : IRequest<AdminExamCategoryDto>
{
    public CreateAdminExamCategoryRequest Request { get; set; } = new();
}

public class UpdateAdminExamCategoryCommand : IRequest<AdminExamCategoryDto>
{
    public Guid Id { get; set; }
    public UpdateAdminExamCategoryRequest Request { get; set; } = new();
}

public class ArchiveAdminExamCategoryCommand : IRequest<AdminExamCategoryDto>
{
    public Guid Id { get; set; }
}

public class RestoreAdminExamCategoryCommand : IRequest<AdminExamCategoryDto>
{
    public Guid Id { get; set; }
}

public class DeleteAdminExamCategoryCommand : IRequest
{
    public Guid Id { get; set; }
}

public class ListAdminExamCategoriesQueryValidator : AbstractValidator<ListAdminExamCategoriesQuery>
{
    public ListAdminExamCategoriesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetAdminExamCategoryQueryValidator : AbstractValidator<GetAdminExamCategoryQuery>
{
    public GetAdminExamCategoryQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class CreateAdminExamCategoryCommandValidator : AbstractValidator<CreateAdminExamCategoryCommand>
{
    public CreateAdminExamCategoryCommandValidator()
    {
        RuleFor(x => x.Request.CountryId).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.Description).MaximumLength(1000);
    }
}

public class UpdateAdminExamCategoryCommandValidator : AbstractValidator<UpdateAdminExamCategoryCommand>
{
    public UpdateAdminExamCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.CountryId).NotEqual(Guid.Empty).When(x => x.Request.CountryId.HasValue);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.Description).MaximumLength(1000);
    }
}

public class ArchiveAdminExamCategoryCommandValidator : AbstractValidator<ArchiveAdminExamCategoryCommand>
{
    public ArchiveAdminExamCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class RestoreAdminExamCategoryCommandValidator : AbstractValidator<RestoreAdminExamCategoryCommand>
{
    public RestoreAdminExamCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteAdminExamCategoryCommandValidator : AbstractValidator<DeleteAdminExamCategoryCommand>
{
    public DeleteAdminExamCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class ListAdminExamCategoriesQueryHandler : IRequestHandler<ListAdminExamCategoriesQuery, PaginatedResult<AdminExamCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminExamCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<AdminExamCategoryDto>> Handle(ListAdminExamCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ExamCategories.AsQueryable();
        if (request.CountryId is not null)
        {
            query = query.Where(c => c.CountryId == request.CountryId);
        }

        if (request.IsActive is not null)
        {
            query = query.Where(c => c.IsActive == request.IsActive);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .Join(_context.Countries, c => c.CountryId, country => country.Id, (category, country) => new { category, country })
            .OrderBy(r => r.category.CountryId)
            .ThenBy(r => r.category.DisplayOrder)
            .ThenBy(r => r.category.Name)
            .ThenBy(r => r.category.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AdminExamCategoryDto>
        {
            Items = rows.Select(r => AdminExamMapping.ToCategoryDto(r.category, r.country.Name)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetAdminExamCategoryQueryHandler : IRequestHandler<GetAdminExamCategoryQuery, AdminExamCategoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminExamCategoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamCategoryDto> Handle(GetAdminExamCategoryQuery request, CancellationToken cancellationToken)
    {
        var row = await _context.ExamCategories
            .Where(c => c.Id == request.Id)
            .Join(_context.Countries, c => c.CountryId, country => country.Id, (category, country) => new { category, country })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new KeyNotFoundException("Exam category was not found.");
        }

        return AdminExamMapping.ToCategoryDto(row.category, row.country.Name);
    }
}

public class CreateAdminExamCategoryCommandHandler : IRequestHandler<CreateAdminExamCategoryCommand, AdminExamCategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminExamCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamCategoryDto> Handle(CreateAdminExamCategoryCommand request, CancellationToken cancellationToken)
    {
        var countryName = await _context.Countries
            .Where(c => c.Id == request.Request.CountryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (countryName is null)
        {
            throw new KeyNotFoundException("Country was not found.");
        }

        await EnsureSlugUniqueAsync(_context, request.Request.CountryId, request.Request.Slug, null, cancellationToken);

        var category = new ExamCategory
        {
            Id = Guid.NewGuid(),
            CountryId = request.Request.CountryId,
            Name = request.Request.Name.Trim(),
            Slug = request.Request.Slug.Trim(),
            Description = request.Request.Description?.Trim(),
            DisplayOrder = request.Request.DisplayOrder,
            IsActive = true
        };
        _context.ExamCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToCategoryDto(category, countryName);
    }

    internal static async Task EnsureSlugUniqueAsync(
        IApplicationDbContext context,
        Guid countryId,
        string slug,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        var exists = await context.ExamCategories.AnyAsync(c =>
                c.CountryId == countryId
                && c.Slug == slug.Trim()
                && (currentId == null || c.Id != currentId),
            cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("An exam category with this slug already exists for the country.");
        }
    }
}

public class UpdateAdminExamCategoryCommandHandler : IRequestHandler<UpdateAdminExamCategoryCommand, AdminExamCategoryDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAdminExamCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamCategoryDto> Handle(UpdateAdminExamCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category is null)
        {
            throw new KeyNotFoundException("Exam category was not found.");
        }

        if (request.Request.CountryId is not null && request.Request.CountryId.Value != category.CountryId)
        {
            throw new InvalidOperationException("Exam category country cannot be changed.");
        }

        await CreateAdminExamCategoryCommandHandler.EnsureSlugUniqueAsync(
            _context,
            category.CountryId,
            request.Request.Slug,
            category.Id,
            cancellationToken);

        category.Name = request.Request.Name.Trim();
        category.Slug = request.Request.Slug.Trim();
        category.Description = request.Request.Description?.Trim();
        category.DisplayOrder = request.Request.DisplayOrder;

        await _context.SaveChangesAsync(cancellationToken);
        var countryName = await _context.Countries.Where(c => c.Id == category.CountryId).Select(c => c.Name).FirstAsync(cancellationToken);
        return AdminExamMapping.ToCategoryDto(category, countryName);
    }
}

public class ArchiveAdminExamCategoryCommandHandler : IRequestHandler<ArchiveAdminExamCategoryCommand, AdminExamCategoryDto>
{
    private readonly IApplicationDbContext _context;

    public ArchiveAdminExamCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<AdminExamCategoryDto> Handle(ArchiveAdminExamCategoryCommand request, CancellationToken cancellationToken)
    {
        return SetActiveAsync(_context, request.Id, false, cancellationToken);
    }

    internal static async Task<AdminExamCategoryDto> SetActiveAsync(
        IApplicationDbContext context,
        Guid id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var category = await context.ExamCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            throw new KeyNotFoundException("Exam category was not found.");
        }

        category.IsActive = isActive;
        await context.SaveChangesAsync(cancellationToken);
        var countryName = await context.Countries.Where(c => c.Id == category.CountryId).Select(c => c.Name).FirstAsync(cancellationToken);
        return AdminExamMapping.ToCategoryDto(category, countryName);
    }
}

public class RestoreAdminExamCategoryCommandHandler : IRequestHandler<RestoreAdminExamCategoryCommand, AdminExamCategoryDto>
{
    private readonly IApplicationDbContext _context;

    public RestoreAdminExamCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<AdminExamCategoryDto> Handle(RestoreAdminExamCategoryCommand request, CancellationToken cancellationToken)
    {
        return ArchiveAdminExamCategoryCommandHandler.SetActiveAsync(_context, request.Id, true, cancellationToken);
    }
}

public class DeleteAdminExamCategoryCommandHandler : IRequestHandler<DeleteAdminExamCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAdminExamCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAdminExamCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category is null)
        {
            throw new KeyNotFoundException("Exam category was not found.");
        }

        var referenced = await _context.Exams.AnyAsync(e => e.ExamCategoryId == category.Id, cancellationToken);
        if (referenced)
        {
            throw new InvalidOperationException("Referenced exam categories cannot be deleted.");
        }

        _context.ExamCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
