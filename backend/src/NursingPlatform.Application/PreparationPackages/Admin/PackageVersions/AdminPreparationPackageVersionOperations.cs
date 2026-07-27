using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.PreparationPackages.Common;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

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

public class CreateAdminPreparationPackageVersionCommandHandler : IRequestHandler<CreateAdminPreparationPackageVersionCommand, AdminPreparationPackageVersionDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAdminPreparationPackageVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageVersionDto> Handle(CreateAdminPreparationPackageVersionCommand request, CancellationToken cancellationToken)
    {
        var definitionExists = await _context.PreparationPackageDefinitions.AnyAsync(d => d.Id == request.PreparationPackageDefinitionId, cancellationToken);
        if (!definitionExists) throw new KeyNotFoundException("Preparation package definition was not found.");
        var version = PreparationPackageVersion.CreateDraft(request.PreparationPackageDefinitionId, request.Request.ExamVersionId, request.Request.ReportingProfilePublicationId, request.Request.PracticeCollectionVersionId);
        foreach (var material in request.Request.Materials.OrderBy(m => m.SortOrder))
        {
            version.AddMaterialVersion(material.StudyMaterialVersionId, material.SortOrder);
        }

        _context.PreparationPackageVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageVersionDto(version);
    }
}

public class GetAdminPreparationPackageVersionValidationQueryHandler : IRequestHandler<GetAdminPreparationPackageVersionValidationQuery, PackagePublicationValidationDto>
{
    private readonly IApplicationDbContext _context;
    public GetAdminPreparationPackageVersionValidationQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<PackagePublicationValidationDto> Handle(GetAdminPreparationPackageVersionValidationQuery request, CancellationToken cancellationToken)
    {
        var version = await _context.PreparationPackageVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PreparationPackageDefinitionId == request.PreparationPackageDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package version was not found.");
        return await new PreparationPackagePublicationValidator(_context).ValidatePackageVersionAsync(version, cancellationToken);
    }
}

public class PublishAdminPreparationPackageVersionCommandHandler : IRequestHandler<PublishAdminPreparationPackageVersionCommand, AdminPreparationPackageVersionDto>
{
    private readonly IApplicationDbContext _context;
    public PublishAdminPreparationPackageVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageVersionDto> Handle(PublishAdminPreparationPackageVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _context.PreparationPackageVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PreparationPackageDefinitionId == request.PreparationPackageDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package version was not found.");
        var validation = await new PreparationPackagePublicationValidator(_context).ValidatePackageVersionAsync(version, cancellationToken);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.Issues[0].Message);
        }

        version.Publish(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageVersionDto(version);
    }
}

public class RetireAdminPreparationPackageVersionCommandHandler : IRequestHandler<RetireAdminPreparationPackageVersionCommand, AdminPreparationPackageVersionDto>
{
    private readonly IApplicationDbContext _context;
    public RetireAdminPreparationPackageVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPreparationPackageVersionDto> Handle(RetireAdminPreparationPackageVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _context.PreparationPackageVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PreparationPackageDefinitionId == request.PreparationPackageDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package version was not found.");
        version.Retire(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPackageVersionDto(version);
    }
}
