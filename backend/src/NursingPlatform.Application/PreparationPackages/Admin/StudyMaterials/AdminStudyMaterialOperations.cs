using FluentValidation;
using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Admin.StudyMaterials;

public class CreateAdminStudyMaterialRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateAdminStudyMaterialVersionRequest
{
    public StudyMaterialType MaterialType { get; set; }
    public string? FormattedTextContent { get; set; }
    public string? FileStorageKey { get; set; }
    public string? ExternalUrl { get; set; }
    public string? VideoUrl { get; set; }
    public List<Guid> ReportingTopicIds { get; set; } = [];
}

public class UpdateAdminStudyMaterialVersionRequest : CreateAdminStudyMaterialVersionRequest;

public class ListAdminStudyMaterialsQuery : IRequest<PaginatedResult<AdminStudyMaterialDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateAdminStudyMaterialCommand : IRequest<AdminStudyMaterialDto>
{
    public CreateAdminStudyMaterialRequest Request { get; set; } = new();
}

public class CreateAdminStudyMaterialVersionCommand : IRequest<AdminStudyMaterialVersionDto>
{
    public Guid StudyMaterialId { get; set; }
    public CreateAdminStudyMaterialVersionRequest Request { get; set; } = new();
}

public class UpdateAdminStudyMaterialVersionCommand : IRequest<AdminStudyMaterialVersionDto>
{
    public Guid StudyMaterialId { get; set; }
    public Guid VersionId { get; set; }
    public UpdateAdminStudyMaterialVersionRequest Request { get; set; } = new();
}

public class PublishAdminStudyMaterialVersionCommand : IRequest<AdminStudyMaterialVersionDto>
{
    public Guid StudyMaterialId { get; set; }
    public Guid VersionId { get; set; }
}

public class RetireAdminStudyMaterialVersionCommand : IRequest<AdminStudyMaterialVersionDto>
{
    public Guid StudyMaterialId { get; set; }
    public Guid VersionId { get; set; }
}

public class ListAdminStudyMaterialsQueryValidator : AbstractValidator<ListAdminStudyMaterialsQuery>
{
    public ListAdminStudyMaterialsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class CreateAdminStudyMaterialCommandValidator : AbstractValidator<CreateAdminStudyMaterialCommand>
{
    public CreateAdminStudyMaterialCommandValidator()
    {
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.Description).MaximumLength(2000);
    }
}

public class CreateAdminStudyMaterialVersionCommandValidator : AbstractValidator<CreateAdminStudyMaterialVersionCommand>
{
    public CreateAdminStudyMaterialVersionCommandValidator()
    {
        RuleFor(x => x.StudyMaterialId).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new StudyMaterialVersionRequestValidator());
    }
}

public class UpdateAdminStudyMaterialVersionCommandValidator : AbstractValidator<UpdateAdminStudyMaterialVersionCommand>
{
    public UpdateAdminStudyMaterialVersionCommandValidator()
    {
        RuleFor(x => x.StudyMaterialId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new StudyMaterialVersionRequestValidator());
    }
}

public class PublishAdminStudyMaterialVersionCommandValidator : AbstractValidator<PublishAdminStudyMaterialVersionCommand>
{
    public PublishAdminStudyMaterialVersionCommandValidator()
    {
        RuleFor(x => x.StudyMaterialId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class RetireAdminStudyMaterialVersionCommandValidator : AbstractValidator<RetireAdminStudyMaterialVersionCommand>
{
    public RetireAdminStudyMaterialVersionCommandValidator()
    {
        RuleFor(x => x.StudyMaterialId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class StudyMaterialVersionRequestValidator : AbstractValidator<CreateAdminStudyMaterialVersionRequest>
{
    public StudyMaterialVersionRequestValidator()
    {
        RuleFor(x => x.MaterialType).IsInEnum();
        RuleFor(x => x.ReportingTopicIds).NotEmpty();
        RuleForEach(x => x.ReportingTopicIds).NotEmpty();

        When(x => x.MaterialType == StudyMaterialType.FormattedText, () =>
        {
            RuleFor(x => x.FormattedTextContent).NotEmpty().MaximumLength(20000);
            RuleFor(x => x.FileStorageKey).Empty();
            RuleFor(x => x.ExternalUrl).Empty();
            RuleFor(x => x.VideoUrl).Empty();
        });

        When(x => x.MaterialType == StudyMaterialType.File, () =>
        {
            RuleFor(x => x.FileStorageKey).NotEmpty().MaximumLength(1024);
            RuleFor(x => x.FormattedTextContent).Empty();
            RuleFor(x => x.ExternalUrl).Empty();
            RuleFor(x => x.VideoUrl).Empty();
        });

        When(x => x.MaterialType == StudyMaterialType.ExternalLink, () =>
        {
            RuleFor(x => x.ExternalUrl).NotEmpty().MaximumLength(2048);
            RuleFor(x => x.FormattedTextContent).Empty();
            RuleFor(x => x.FileStorageKey).Empty();
            RuleFor(x => x.VideoUrl).Empty();
        });

        When(x => x.MaterialType == StudyMaterialType.Video, () =>
        {
            RuleFor(x => x.VideoUrl).NotEmpty().MaximumLength(2048);
            RuleFor(x => x.FormattedTextContent).Empty();
            RuleFor(x => x.FileStorageKey).Empty();
            RuleFor(x => x.ExternalUrl).Empty();
        });
    }
}
