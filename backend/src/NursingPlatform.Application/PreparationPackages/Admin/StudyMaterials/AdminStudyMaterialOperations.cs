using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Common;
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

public class CreateAdminStudyMaterialCommandHandler : IRequestHandler<CreateAdminStudyMaterialCommand, AdminStudyMaterialDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAdminStudyMaterialCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminStudyMaterialDto> Handle(CreateAdminStudyMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = StudyMaterial.Create(request.Request.Title, request.Request.Slug, request.Request.Description);
        _context.StudyMaterials.Add(material);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToStudyMaterialDto(material);
    }
}

public class ListAdminStudyMaterialsQueryHandler : IRequestHandler<ListAdminStudyMaterialsQuery, PaginatedResult<AdminStudyMaterialDto>>
{
    private readonly IApplicationDbContext _context;
    public ListAdminStudyMaterialsQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<PaginatedResult<AdminStudyMaterialDto>> Handle(ListAdminStudyMaterialsQuery request, CancellationToken cancellationToken)
    {
        var materials = await _context.StudyMaterials.OrderBy(m => m.Title).ThenBy(m => m.Id).ToListAsync(cancellationToken);
        return new PaginatedResult<AdminStudyMaterialDto> { Items = materials.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(PreparationPackageMapping.ToStudyMaterialDto).ToList(), Page = request.Page, PageSize = request.PageSize, TotalCount = materials.Count };
    }
}

public class CreateAdminStudyMaterialVersionCommandHandler : IRequestHandler<CreateAdminStudyMaterialVersionCommand, AdminStudyMaterialVersionDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAdminStudyMaterialVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminStudyMaterialVersionDto> Handle(CreateAdminStudyMaterialVersionCommand request, CancellationToken cancellationToken)
    {
        var materialExists = await _context.StudyMaterials.AnyAsync(m => m.Id == request.StudyMaterialId, cancellationToken);
        if (!materialExists) throw new KeyNotFoundException("Study material was not found.");
        await StudyMaterialTopicValidator.EnsureTopicsExistAndAreActiveAsync(_context, request.Request.ReportingTopicIds, cancellationToken);
        var nextVersion = await _context.StudyMaterialVersions.Where(v => v.StudyMaterialId == request.StudyMaterialId).Select(v => v.VersionNumber).DefaultIfEmpty().MaxAsync(cancellationToken) + 1;
        var version = StudyMaterialVersion.CreateDraft(request.StudyMaterialId, request.Request.MaterialType, request.Request.FormattedTextContent, request.Request.FileStorageKey, request.Request.ExternalUrl, request.Request.VideoUrl, request.Request.ReportingTopicIds, nextVersion);
        _context.StudyMaterialVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToStudyMaterialVersionDto(version);
    }
}

public class UpdateAdminStudyMaterialVersionCommandHandler : IRequestHandler<UpdateAdminStudyMaterialVersionCommand, AdminStudyMaterialVersionDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateAdminStudyMaterialVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminStudyMaterialVersionDto> Handle(UpdateAdminStudyMaterialVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _context.StudyMaterialVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.StudyMaterialId == request.StudyMaterialId, cancellationToken)
            ?? throw new KeyNotFoundException("Study material version was not found.");
        await StudyMaterialTopicValidator.EnsureTopicsExistAndAreActiveAsync(_context, request.Request.ReportingTopicIds, cancellationToken);
        version.UpdateDraftContent(request.Request.MaterialType, request.Request.FormattedTextContent, request.Request.FileStorageKey, request.Request.ExternalUrl, request.Request.VideoUrl, request.Request.ReportingTopicIds);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToStudyMaterialVersionDto(version);
    }
}

public class PublishAdminStudyMaterialVersionCommandHandler : IRequestHandler<PublishAdminStudyMaterialVersionCommand, AdminStudyMaterialVersionDto>
{
    private readonly IApplicationDbContext _context;
    public PublishAdminStudyMaterialVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminStudyMaterialVersionDto> Handle(PublishAdminStudyMaterialVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _context.StudyMaterialVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.StudyMaterialId == request.StudyMaterialId, cancellationToken)
            ?? throw new KeyNotFoundException("Study material version was not found.");
        if (!version.Topics.Any()) throw new InvalidOperationException("A material version must map to at least one reporting topic.");
        var topicIds = version.Topics.Select(t => t.ReportingTopicId).ToList();
        var existingTopicCount = await _context.ReportingTopics.CountAsync(t => topicIds.Contains(t.Id) && t.IsActive, cancellationToken);
        if (existingTopicCount != topicIds.Distinct().Count()) throw new InvalidOperationException("Material version topics must exist and be active.");
        version.Publish(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToStudyMaterialVersionDto(version);
    }
}

internal static class StudyMaterialTopicValidator
{
    public static async Task EnsureTopicsExistAndAreActiveAsync(
        IApplicationDbContext context,
        IEnumerable<Guid> reportingTopicIds,
        CancellationToken cancellationToken)
    {
        var topicIds = reportingTopicIds.Distinct().ToList();
        var existingTopicCount = await context.ReportingTopics.CountAsync(
            topic => topicIds.Contains(topic.Id) && topic.IsActive,
            cancellationToken);

        if (existingTopicCount != topicIds.Count)
        {
            throw new InvalidOperationException("Material version topics must exist and be active.");
        }
    }
}

public class RetireAdminStudyMaterialVersionCommandHandler : IRequestHandler<RetireAdminStudyMaterialVersionCommand, AdminStudyMaterialVersionDto>
{
    private readonly IApplicationDbContext _context;
    public RetireAdminStudyMaterialVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminStudyMaterialVersionDto> Handle(RetireAdminStudyMaterialVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _context.StudyMaterialVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.StudyMaterialId == request.StudyMaterialId, cancellationToken)
            ?? throw new KeyNotFoundException("Study material version was not found.");
        version.Retire(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToStudyMaterialVersionDto(version);
    }
}
