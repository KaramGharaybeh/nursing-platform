using FluentValidation;
using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.DTOs;

namespace NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;

public class CreateAdminPreparationPackageOfferRequest
{
    public Guid PreparationPackageDefinitionId { get; set; }
    public Guid PreparationPackageVersionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
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
