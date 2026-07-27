using FluentValidation;
using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.DTOs;

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
