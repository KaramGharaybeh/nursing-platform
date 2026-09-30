using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Common;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Admin.PackageDefinitions;

public class CreateAdminPreparationPackageDefinitionRequest
{
    public Guid CountryId { get; set; }
    public Guid ExamCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateAdminPreparationPackageDefinitionRequest : CreateAdminPreparationPackageDefinitionRequest;

public class ListAdminPreparationPackageDefinitionsQuery : IRequest<PaginatedResult<AdminPreparationPackageDefinitionDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CountryId { get; set; }
    public Guid? ExamCategoryId { get; set; }
}

public class CreateAdminPreparationPackageDefinitionCommand : IRequest<AdminPreparationPackageDefinitionDto>
{
    public CreateAdminPreparationPackageDefinitionRequest Request { get; set; } = new();
}

public class UpdateAdminPreparationPackageDefinitionCommand : IRequest<AdminPreparationPackageDefinitionDto>
{
    public Guid Id { get; set; }
    public UpdateAdminPreparationPackageDefinitionRequest Request { get; set; } = new();
}

public class ListAdminPreparationPackageDefinitionsQueryValidator : AbstractValidator<ListAdminPreparationPackageDefinitionsQuery>
{
    public ListAdminPreparationPackageDefinitionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.CountryId).NotEqual(Guid.Empty).When(x => x.CountryId.HasValue);
        RuleFor(x => x.ExamCategoryId).NotEqual(Guid.Empty).When(x => x.ExamCategoryId.HasValue);
    }
}

public class CreateAdminPreparationPackageDefinitionCommandValidator : AbstractValidator<CreateAdminPreparationPackageDefinitionCommand>
{
    public CreateAdminPreparationPackageDefinitionCommandValidator()
    {
        RuleFor(x => x.Request.CountryId).NotEmpty();
        RuleFor(x => x.Request.ExamCategoryId).NotEmpty();
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.Description).MaximumLength(2000);
    }
}

public class UpdateAdminPreparationPackageDefinitionCommandValidator : AbstractValidator<UpdateAdminPreparationPackageDefinitionCommand>
{
    public UpdateAdminPreparationPackageDefinitionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.CountryId).NotEmpty();
        RuleFor(x => x.Request.ExamCategoryId).NotEmpty();
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.Description).MaximumLength(2000);
    }
}

public class CreateAdminPreparationPackageDefinitionCommandHandler : IRequestHandler<CreateAdminPreparationPackageDefinitionCommand, AdminPreparationPackageDefinitionDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAdminPreparationPackageDefinitionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageDefinitionDto> Handle(CreateAdminPreparationPackageDefinitionCommand request, CancellationToken cancellationToken)
    {
        var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == request.Request.CountryId, cancellationToken)
            ?? throw new KeyNotFoundException("Country was not found.");
        var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.Id == request.Request.ExamCategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("Exam category was not found.");
        if (category.CountryId != country.Id) throw new InvalidOperationException("Package category must belong to the selected country.");
        var definition = PreparationPackageDefinition.Create(country.Id, category.Id, request.Request.Title, request.Request.Slug, request.Request.Description);
        _context.PreparationPackageDefinitions.Add(definition);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageDefinitionDto(definition, country.Name, category.Name);
    }
}

public class UpdateAdminPreparationPackageDefinitionCommandHandler : IRequestHandler<UpdateAdminPreparationPackageDefinitionCommand, AdminPreparationPackageDefinitionDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateAdminPreparationPackageDefinitionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageDefinitionDto> Handle(UpdateAdminPreparationPackageDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await _context.PreparationPackageDefinitions.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package definition was not found.");
        var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == request.Request.CountryId, cancellationToken)
            ?? throw new KeyNotFoundException("Country was not found.");
        var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.Id == request.Request.ExamCategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("Exam category was not found.");
        if (category.CountryId != country.Id) throw new InvalidOperationException("Package category must belong to the selected country.");
        definition.Update(country.Id, category.Id, request.Request.Title, request.Request.Slug, request.Request.Description);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageDefinitionDto(definition, country.Name, category.Name);
    }
}

public class ListAdminPreparationPackageDefinitionsQueryHandler : IRequestHandler<ListAdminPreparationPackageDefinitionsQuery, PaginatedResult<AdminPreparationPackageDefinitionDto>>
{
    private readonly IApplicationDbContext _context;
    public ListAdminPreparationPackageDefinitionsQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<PaginatedResult<AdminPreparationPackageDefinitionDto>> Handle(ListAdminPreparationPackageDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PreparationPackageDefinitions.AsQueryable();
        if (request.CountryId.HasValue) query = query.Where(d => d.CountryId == request.CountryId.Value);
        if (request.ExamCategoryId.HasValue) query = query.Where(d => d.ExamCategoryId == request.ExamCategoryId.Value);
        var rows = await query
            .Join(_context.Countries, d => d.CountryId, c => c.Id, (definition, country) => new { definition, country })
            .Join(_context.ExamCategories, dc => dc.definition.ExamCategoryId, c => c.Id, (dc, category) => new { dc.definition, CountryName = dc.country.Name, CategoryName = category.Name })
            .OrderBy(r => r.CountryName)
            .ThenBy(r => r.CategoryName)
            .ThenBy(r => r.definition.Title)
            .ThenBy(r => r.definition.Id)
            .ToListAsync(cancellationToken);
        return new PaginatedResult<AdminPreparationPackageDefinitionDto> { Items = rows.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(r => PreparationPackageMapping.ToPackageDefinitionDto(r.definition, r.CountryName, r.CategoryName)).ToList(), Page = request.Page, PageSize = request.PageSize, TotalCount = rows.Count };
    }
}
