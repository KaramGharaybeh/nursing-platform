using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Common;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Catalog;

public class ListPreparationPackageOffersQuery : IRequest<PaginatedResult<PreparationPackageOfferListItemDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CountryId { get; set; }
    public Guid? ExamCategoryId { get; set; }
}

public class GetPreparationPackageOfferQuery : IRequest<PreparationPackageOfferDetailDto>
{
    public string Slug { get; set; } = string.Empty;
}

public class ListPreparationPackageOffersQueryValidator : AbstractValidator<ListPreparationPackageOffersQuery>
{
    public ListPreparationPackageOffersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.CountryId).NotEqual(Guid.Empty).When(x => x.CountryId.HasValue);
        RuleFor(x => x.ExamCategoryId).NotEqual(Guid.Empty).When(x => x.ExamCategoryId.HasValue);
    }
}

public class GetPreparationPackageOfferQueryValidator : AbstractValidator<GetPreparationPackageOfferQuery>
{
    public GetPreparationPackageOfferQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(160);
    }
}

public class ListPreparationPackageOffersQueryHandler : IRequestHandler<ListPreparationPackageOffersQuery, PaginatedResult<PreparationPackageOfferListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public ListPreparationPackageOffersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<PreparationPackageOfferListItemDto>> Handle(ListPreparationPackageOffersQuery request, CancellationToken cancellationToken)
    {
        var items = await LoadSafeCatalogOffersAsync(_context, request.CountryId, request.ExamCategoryId, null, cancellationToken);
        return new PaginatedResult<PreparationPackageOfferListItemDto>
        {
            Items = items.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = items.Count
        };
    }

    internal static async Task<List<PreparationPackageOfferListItemDto>> LoadSafeCatalogOffersAsync(
        IApplicationDbContext context,
        Guid? countryId,
        Guid? examCategoryId,
        string? slug,
        CancellationToken cancellationToken)
    {
        var offers = (await context.PreparationPackageOffers
            .OrderBy(o => o.Title)
            .ThenBy(o => o.Id)
            .ToListAsync(cancellationToken))
            .Where(o => o.Status == PreparationPackageOfferStatus.Active)
            .ToList();

        if (!string.IsNullOrWhiteSpace(slug))
        {
            offers = offers.Where(o => o.Slug == slug).ToList();
        }

        var items = new List<PreparationPackageOfferListItemDto>();
        foreach (var offer in offers)
        {
            var packageVersion = await context.PreparationPackageVersions
                .Include(v => v.Materials)
                .FirstOrDefaultAsync(v => v.Id == offer.PreparationPackageVersionId, cancellationToken);
            if (packageVersion is null || packageVersion.Status != PreparationPackageVersionStatus.Published)
            {
                continue;
            }

            var validation = await new PreparationPackagePublicationValidator(context).ValidatePackageVersionAsync(packageVersion, cancellationToken);
            if (!validation.IsValid)
            {
                continue;
            }

            var definition = await context.PreparationPackageDefinitions.FirstOrDefaultAsync(d => d.Id == offer.PreparationPackageDefinitionId, cancellationToken);
            if (definition is null)
            {
                continue;
            }

            if (countryId.HasValue && definition.CountryId != countryId.Value)
            {
                continue;
            }

            if (examCategoryId.HasValue && definition.ExamCategoryId != examCategoryId.Value)
            {
                continue;
            }

            var examVersion = await context.ExamVersions.FirstOrDefaultAsync(v => v.Id == packageVersion.ExamVersionId, cancellationToken);
            if (examVersion is null || examVersion.Status.ToString() != "Published")
            {
                continue;
            }

            var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examVersion.ExamId, cancellationToken);
            var countryName = await context.Countries.Where(c => c.Id == definition.CountryId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
            var categoryName = await context.ExamCategories.Where(c => c.Id == definition.ExamCategoryId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
            var practiceVersion = await context.PracticeCollectionVersions.FirstOrDefaultAsync(v => v.Id == packageVersion.PracticeCollectionVersionId, cancellationToken);
            items.Add(new PreparationPackageOfferListItemDto
            {
                Id = offer.Id,
                Title = offer.Title,
                Slug = offer.Slug,
                Summary = offer.Summary,
                CountryId = definition.CountryId,
                CountryName = countryName,
                ExamCategoryId = definition.ExamCategoryId,
                ExamCategoryName = categoryName,
                ExamId = exam?.Id ?? Guid.Empty,
                ExamTitle = exam?.Title ?? string.Empty,
                MaterialCount = packageVersion.Materials.Count,
                PracticeItemCount = practiceVersion?.Items.Count ?? 0,
                AccessDurationDays = offer.AccessDurationDays,
                PriceAmountMinor = offer.PriceAmountMinor,
                Currency = offer.Currency
            });
        }

        return items
            .OrderBy(i => i.CountryName)
            .ThenBy(i => i.ExamCategoryName)
            .ThenBy(i => i.Title)
            .ThenBy(i => i.Id)
            .ToList();
    }
}

public class GetPreparationPackageOfferQueryHandler : IRequestHandler<GetPreparationPackageOfferQuery, PreparationPackageOfferDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetPreparationPackageOfferQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PreparationPackageOfferDetailDto> Handle(GetPreparationPackageOfferQuery request, CancellationToken cancellationToken)
    {
        var item = (await ListPreparationPackageOffersQueryHandler.LoadSafeCatalogOffersAsync(_context, null, null, request.Slug, cancellationToken)).FirstOrDefault()
            ?? throw new KeyNotFoundException("Preparation package offer was not found.");
        return new PreparationPackageOfferDetailDto
        {
            Id = item.Id,
            Title = item.Title,
            Slug = item.Slug,
            Summary = item.Summary,
            CountryId = item.CountryId,
            CountryName = item.CountryName,
            ExamCategoryId = item.ExamCategoryId,
            ExamCategoryName = item.ExamCategoryName,
            ExamId = item.ExamId,
            ExamTitle = item.ExamTitle,
            MaterialCount = item.MaterialCount,
            PracticeItemCount = item.PracticeItemCount,
            AccessDurationDays = item.AccessDurationDays,
            PriceAmountMinor = item.PriceAmountMinor,
            Currency = item.Currency,
            Components =
            [
                new PreparationPackageCatalogComponentSummaryDto { Name = "Study materials", Count = item.MaterialCount, Summary = "Published study material versions included in this package." },
                new PreparationPackageCatalogComponentSummaryDto { Name = "Practice collection", Count = item.PracticeItemCount, Summary = "Independent practice items included in this package." },
                new PreparationPackageCatalogComponentSummaryDto { Name = "Mock exam", Count = 1, Summary = item.ExamTitle }
            ]
        };
    }
}
