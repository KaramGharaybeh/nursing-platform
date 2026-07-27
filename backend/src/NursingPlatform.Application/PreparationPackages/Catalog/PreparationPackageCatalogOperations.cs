using FluentValidation;
using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.DTOs;

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
