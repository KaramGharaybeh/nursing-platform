using FluentValidation;
using MediatR;
using NursingPlatform.Application.PreparationPackages.DTOs;

namespace NursingPlatform.Application.PreparationPackages.Admin.PackageVersions;

public class PreparationPackageVersionMaterialRequest
{
    public Guid StudyMaterialVersionId { get; set; }
    public int SortOrder { get; set; }
}

public class CreateAdminPreparationPackageVersionRequest
{
    public Guid ExamVersionId { get; set; }
    public Guid ReportingProfilePublicationId { get; set; }
    public Guid PracticeCollectionVersionId { get; set; }
    public List<PreparationPackageVersionMaterialRequest> Materials { get; set; } = [];
}

public class CreateAdminPreparationPackageVersionCommand : IRequest<AdminPreparationPackageVersionDto>
{
    public Guid PreparationPackageDefinitionId { get; set; }
    public CreateAdminPreparationPackageVersionRequest Request { get; set; } = new();
}

public class GetAdminPreparationPackageVersionValidationQuery : IRequest<PackagePublicationValidationDto>
{
    public Guid PreparationPackageDefinitionId { get; set; }
    public Guid VersionId { get; set; }
}

public class PublishAdminPreparationPackageVersionCommand : IRequest<AdminPreparationPackageVersionDto>
{
    public Guid PreparationPackageDefinitionId { get; set; }
    public Guid VersionId { get; set; }
}

public class RetireAdminPreparationPackageVersionCommand : IRequest<AdminPreparationPackageVersionDto>
{
    public Guid PreparationPackageDefinitionId { get; set; }
    public Guid VersionId { get; set; }
}

public class CreateAdminPreparationPackageVersionCommandValidator : AbstractValidator<CreateAdminPreparationPackageVersionCommand>
{
    public CreateAdminPreparationPackageVersionCommandValidator()
    {
        RuleFor(x => x.PreparationPackageDefinitionId).NotEmpty();
        RuleFor(x => x.Request.ExamVersionId).NotEmpty();
        RuleFor(x => x.Request.ReportingProfilePublicationId).NotEmpty();
        RuleFor(x => x.Request.PracticeCollectionVersionId).NotEmpty();
        RuleFor(x => x.Request.Materials).NotEmpty();
        RuleFor(x => x.Request.Materials)
            .Must(materials => materials.Select(material => material.SortOrder).Distinct().Count() == materials.Count)
            .WithMessage("Material sort order must be unique within a package version.");
        RuleForEach(x => x.Request.Materials).SetValidator(new PreparationPackageVersionMaterialRequestValidator());
    }
}

public class GetAdminPreparationPackageVersionValidationQueryValidator : AbstractValidator<GetAdminPreparationPackageVersionValidationQuery>
{
    public GetAdminPreparationPackageVersionValidationQueryValidator()
    {
        RuleFor(x => x.PreparationPackageDefinitionId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class PublishAdminPreparationPackageVersionCommandValidator : AbstractValidator<PublishAdminPreparationPackageVersionCommand>
{
    public PublishAdminPreparationPackageVersionCommandValidator()
    {
        RuleFor(x => x.PreparationPackageDefinitionId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class RetireAdminPreparationPackageVersionCommandValidator : AbstractValidator<RetireAdminPreparationPackageVersionCommand>
{
    public RetireAdminPreparationPackageVersionCommandValidator()
    {
        RuleFor(x => x.PreparationPackageDefinitionId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class PreparationPackageVersionMaterialRequestValidator : AbstractValidator<PreparationPackageVersionMaterialRequest>
{
    public PreparationPackageVersionMaterialRequestValidator()
    {
        RuleFor(x => x.StudyMaterialVersionId).NotEmpty();
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(1);
    }
}
