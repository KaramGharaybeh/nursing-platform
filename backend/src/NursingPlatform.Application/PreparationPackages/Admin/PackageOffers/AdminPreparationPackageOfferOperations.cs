using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Common;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;
using System.Text.Json.Serialization;

namespace NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;

public class CreateAdminPreparationPackageOfferRequest
{
    public Guid PreparationPackageDefinitionId { get; set; }
    public Guid PreparationPackageVersionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long PriceAmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int AccessDurationDays { get; set; }
}

public class UpdateAdminPreparationPackageOfferRequest : CreateAdminPreparationPackageOfferRequest;

public class ListAdminPreparationPackageOffersQuery : IRequest<PaginatedResult<AdminPreparationPackageOfferDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? PreparationPackageDefinitionId { get; set; }
}

public class CreateAdminPreparationPackageOfferCommand : IRequest<AdminPreparationPackageOfferDto>
{
    public CreateAdminPreparationPackageOfferRequest Request { get; set; } = new();
}

public class UpdateAdminPreparationPackageOfferCommand : IRequest<AdminPreparationPackageOfferDto>
{
    public Guid Id { get; set; }
    public UpdateAdminPreparationPackageOfferRequest Request { get; set; } = new();
}

public class ActivateAdminPreparationPackageOfferCommand : IRequest<AdminPreparationPackageOfferDto>
{
    public Guid Id { get; set; }
}

public class DeactivateAdminPreparationPackageOfferCommand : IRequest<AdminPreparationPackageOfferDto>
{
    public Guid Id { get; set; }
}

public class ListAdminPreparationPackageOffersQueryValidator : AbstractValidator<ListAdminPreparationPackageOffersQuery>
{
    public ListAdminPreparationPackageOffersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.PreparationPackageDefinitionId).NotEqual(Guid.Empty).When(x => x.PreparationPackageDefinitionId.HasValue);
    }
}

public class CreateAdminPreparationPackageOfferCommandValidator : AbstractValidator<CreateAdminPreparationPackageOfferCommand>
{
    public CreateAdminPreparationPackageOfferCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new PreparationPackageOfferRequestValidator());
    }
}

public class UpdateAdminPreparationPackageOfferCommandValidator : AbstractValidator<UpdateAdminPreparationPackageOfferCommand>
{
    public UpdateAdminPreparationPackageOfferCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new PreparationPackageOfferRequestValidator());
    }
}

public class ActivateAdminPreparationPackageOfferCommandValidator : AbstractValidator<ActivateAdminPreparationPackageOfferCommand>
{
    public ActivateAdminPreparationPackageOfferCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeactivateAdminPreparationPackageOfferCommandValidator : AbstractValidator<DeactivateAdminPreparationPackageOfferCommand>
{
    public DeactivateAdminPreparationPackageOfferCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class PreparationPackageOfferRequestValidator : AbstractValidator<CreateAdminPreparationPackageOfferRequest>
{
    public PreparationPackageOfferRequestValidator()
    {
        RuleFor(x => x.PreparationPackageDefinitionId).NotEmpty();
        RuleFor(x => x.PreparationPackageVersionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Summary).MaximumLength(2000);
        RuleFor(x => x.PriceAmountMinor).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Matches("^[A-Z]{3}$");
        RuleFor(x => x.AccessDurationDays).GreaterThanOrEqualTo(1);
    }
}

public class CreateAdminPreparationPackageOfferCommandHandler : IRequestHandler<CreateAdminPreparationPackageOfferCommand, AdminPreparationPackageOfferDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAdminPreparationPackageOfferCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageOfferDto> Handle(CreateAdminPreparationPackageOfferCommand request, CancellationToken cancellationToken)
    {
        await ValidatePackageVersionAsync(request.Request.PreparationPackageDefinitionId, request.Request.PreparationPackageVersionId, cancellationToken);
        var offer = PreparationPackageOffer.CreateDraft(request.Request.PreparationPackageDefinitionId, request.Request.PreparationPackageVersionId, request.Request.Title, request.Request.Slug, request.Request.Summary, request.Request.PriceAmountMinor, request.Request.Currency, request.Request.AccessDurationDays);
        _context.PreparationPackageOffers.Add(offer);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageOfferDto(offer);
    }

    private async Task ValidatePackageVersionAsync(Guid definitionId, Guid versionId, CancellationToken cancellationToken)
    {
        var definitionExists = await _context.PreparationPackageDefinitions.AnyAsync(d => d.Id == definitionId, cancellationToken);
        if (!definitionExists) throw new KeyNotFoundException("Preparation package definition was not found.");
        var version = await _context.PreparationPackageVersions.FirstOrDefaultAsync(v => v.Id == versionId && v.PreparationPackageDefinitionId == definitionId, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package version was not found.");
        if (version.Status != PreparationPackageVersionStatus.Published)
        {
            throw new InvalidOperationException("Only published package versions can be referenced by an offer.");
        }
    }
}

public class UpdateAdminPreparationPackageOfferCommandHandler : IRequestHandler<UpdateAdminPreparationPackageOfferCommand, AdminPreparationPackageOfferDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateAdminPreparationPackageOfferCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageOfferDto> Handle(UpdateAdminPreparationPackageOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.PreparationPackageOffers.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package offer was not found.");
        await ValidatePackageVersionAsync(request.Request.PreparationPackageDefinitionId, request.Request.PreparationPackageVersionId, cancellationToken);
        offer.UpdateDraft(request.Request.PreparationPackageDefinitionId, request.Request.PreparationPackageVersionId, request.Request.Title, request.Request.Slug, request.Request.Summary, request.Request.PriceAmountMinor, request.Request.Currency, request.Request.AccessDurationDays);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageOfferDto(offer);
    }

    private async Task ValidatePackageVersionAsync(Guid definitionId, Guid versionId, CancellationToken cancellationToken)
    {
        var definitionExists = await _context.PreparationPackageDefinitions.AnyAsync(d => d.Id == definitionId, cancellationToken);
        if (!definitionExists) throw new KeyNotFoundException("Preparation package definition was not found.");
        var version = await _context.PreparationPackageVersions.FirstOrDefaultAsync(v => v.Id == versionId && v.PreparationPackageDefinitionId == definitionId, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package version was not found.");
        if (version.Status != PreparationPackageVersionStatus.Published)
        {
            throw new InvalidOperationException("Only published package versions can be referenced by an offer.");
        }
    }
}

public class ActivateAdminPreparationPackageOfferCommandHandler : IRequestHandler<ActivateAdminPreparationPackageOfferCommand, AdminPreparationPackageOfferDto>
{
    private readonly IApplicationDbContext _context;
    public ActivateAdminPreparationPackageOfferCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageOfferDto> Handle(ActivateAdminPreparationPackageOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.PreparationPackageOffers.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package offer was not found.");
        var version = await _context.PreparationPackageVersions
            .Include(v => v.Materials)
            .FirstOrDefaultAsync(v => v.Id == offer.PreparationPackageVersionId, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package version was not found.");
        if (version.Status != PreparationPackageVersionStatus.Published)
        {
            throw new InvalidOperationException("Only published package versions can be activated for sale.");
        }

        var eligibility = await new PreparationPackagePublicationValidator(_context)
            .ValidatePackageVersionAsync(version, cancellationToken);
        if (!eligibility.IsValid)
        {
            throw new InvalidOperationException(eligibility.Issues[0].Message);
        }

        if (offer.PriceAmountMinor < 0 || offer.Currency.Length != 3 || offer.AccessDurationDays < 1)
        {
            throw new InvalidOperationException("Package offer commercial configuration is invalid.");
        }

        var activeExists = await _context.PreparationPackageOffers.AnyAsync(o => o.Id != offer.Id && o.PreparationPackageDefinitionId == offer.PreparationPackageDefinitionId && o.Status == PreparationPackageOfferStatus.Active, cancellationToken);
        if (activeExists)
        {
            throw new InvalidOperationException("Only one active preparation package offer is allowed per package definition.");
        }

        offer.Activate(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageOfferDto(offer);
    }
}

public class DeactivateAdminPreparationPackageOfferCommandHandler : IRequestHandler<DeactivateAdminPreparationPackageOfferCommand, AdminPreparationPackageOfferDto>
{
    private readonly IApplicationDbContext _context;
    public DeactivateAdminPreparationPackageOfferCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageOfferDto> Handle(DeactivateAdminPreparationPackageOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.PreparationPackageOffers.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package offer was not found.");
        offer.Deactivate(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageOfferDto(offer);
    }
}

public class ListAdminPreparationPackageOffersQueryHandler : IRequestHandler<ListAdminPreparationPackageOffersQuery, PaginatedResult<AdminPreparationPackageOfferDto>>
{
    private readonly IApplicationDbContext _context;
    public ListAdminPreparationPackageOffersQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<PaginatedResult<AdminPreparationPackageOfferDto>> Handle(ListAdminPreparationPackageOffersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PreparationPackageOffers.AsQueryable();
        if (request.PreparationPackageDefinitionId.HasValue) query = query.Where(o => o.PreparationPackageDefinitionId == request.PreparationPackageDefinitionId.Value);
        var offers = await query.OrderBy(o => o.Title).ThenBy(o => o.Id).ToListAsync(cancellationToken);
        return new PaginatedResult<AdminPreparationPackageOfferDto> { Items = offers.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(PreparationPackageMapping.ToPackageOfferDto).ToList(), Page = request.Page, PageSize = request.PageSize, TotalCount = offers.Count };
    }
}
