using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Common;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Admin.PracticeCollections;

public class CreateAdminPracticeCollectionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpsertAdminPracticeAnswerOptionRequest
{
    public string OptionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCorrect { get; set; }
}

public class UpsertAdminPracticeItemRequest
{
    public Guid ReportingTopicId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string ImmediateFeedback { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<UpsertAdminPracticeAnswerOptionRequest> AnswerOptions { get; set; } = [];
}

public class CreateAdminPracticeCollectionVersionRequest
{
    public List<UpsertAdminPracticeItemRequest> Items { get; set; } = [];
}

public class UpdateAdminPracticeCollectionVersionRequest : CreateAdminPracticeCollectionVersionRequest;

public class ListAdminPracticeCollectionsQuery : IRequest<PaginatedResult<AdminPracticeCollectionDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateAdminPracticeCollectionCommand : IRequest<AdminPracticeCollectionDto>
{
    public CreateAdminPracticeCollectionRequest Request { get; set; } = new();
}

public class CreateAdminPracticeCollectionVersionCommand : IRequest<AdminPracticeCollectionVersionDto>
{
    public Guid PracticeCollectionId { get; set; }
    public CreateAdminPracticeCollectionVersionRequest Request { get; set; } = new();
}

public class UpdateAdminPracticeCollectionVersionCommand : IRequest<AdminPracticeCollectionVersionDto>
{
    public Guid PracticeCollectionId { get; set; }
    public Guid VersionId { get; set; }
    public UpdateAdminPracticeCollectionVersionRequest Request { get; set; } = new();
}

public class PublishAdminPracticeCollectionVersionCommand : IRequest<AdminPracticeCollectionVersionDto>
{
    public Guid PracticeCollectionId { get; set; }
    public Guid VersionId { get; set; }
}

public class RetireAdminPracticeCollectionVersionCommand : IRequest<AdminPracticeCollectionVersionDto>
{
    public Guid PracticeCollectionId { get; set; }
    public Guid VersionId { get; set; }
}

public class ListAdminPracticeCollectionsQueryValidator : AbstractValidator<ListAdminPracticeCollectionsQuery>
{
    public ListAdminPracticeCollectionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class CreateAdminPracticeCollectionCommandValidator : AbstractValidator<CreateAdminPracticeCollectionCommand>
{
    public CreateAdminPracticeCollectionCommandValidator()
    {
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.Description).MaximumLength(2000);
    }
}

public class CreateAdminPracticeCollectionVersionCommandValidator : AbstractValidator<CreateAdminPracticeCollectionVersionCommand>
{
    public CreateAdminPracticeCollectionVersionCommandValidator()
    {
        RuleFor(x => x.PracticeCollectionId).NotEmpty();
        RuleFor(x => x.Request.Items).NotEmpty();
        RuleForEach(x => x.Request.Items).SetValidator(new UpsertAdminPracticeItemRequestValidator());
    }
}

public class UpdateAdminPracticeCollectionVersionCommandValidator : AbstractValidator<UpdateAdminPracticeCollectionVersionCommand>
{
    public UpdateAdminPracticeCollectionVersionCommandValidator()
    {
        RuleFor(x => x.PracticeCollectionId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.Request.Items).NotEmpty();
        RuleForEach(x => x.Request.Items).SetValidator(new UpsertAdminPracticeItemRequestValidator());
    }
}

public class PublishAdminPracticeCollectionVersionCommandValidator : AbstractValidator<PublishAdminPracticeCollectionVersionCommand>
{
    public PublishAdminPracticeCollectionVersionCommandValidator()
    {
        RuleFor(x => x.PracticeCollectionId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class RetireAdminPracticeCollectionVersionCommandValidator : AbstractValidator<RetireAdminPracticeCollectionVersionCommand>
{
    public RetireAdminPracticeCollectionVersionCommandValidator()
    {
        RuleFor(x => x.PracticeCollectionId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class UpsertAdminPracticeItemRequestValidator : AbstractValidator<UpsertAdminPracticeItemRequest>
{
    public UpsertAdminPracticeItemRequestValidator()
    {
        RuleFor(x => x.ReportingTopicId).NotEmpty();
        RuleFor(x => x.Prompt).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ImmediateFeedback).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(1);
        RuleFor(x => x.AnswerOptions)
            .Must(options => options.Count >= 2)
            .WithMessage("At least two practice answer options are required.");
        RuleFor(x => x.AnswerOptions)
            .Must(options => options.Count(option => option.IsCorrect) == 1)
            .WithMessage("Exactly one correct practice answer option is required.");
        RuleFor(x => x.AnswerOptions)
            .Must(options => options.Select(option => option.DisplayOrder).Distinct().Count() == options.Count)
            .WithMessage("Practice answer option display order must be unique.");
        RuleForEach(x => x.AnswerOptions).SetValidator(new UpsertAdminPracticeAnswerOptionRequestValidator());
    }
}

public class UpsertAdminPracticeAnswerOptionRequestValidator : AbstractValidator<UpsertAdminPracticeAnswerOptionRequest>
{
    public UpsertAdminPracticeAnswerOptionRequestValidator()
    {
        RuleFor(x => x.OptionText).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(1);
    }
}

public class CreateAdminPracticeCollectionCommandHandler : IRequestHandler<CreateAdminPracticeCollectionCommand, AdminPracticeCollectionDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAdminPracticeCollectionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPracticeCollectionDto> Handle(CreateAdminPracticeCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = PracticeCollection.Create(request.Request.Title, request.Request.Slug, request.Request.Description);
        _context.PracticeCollections.Add(collection);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPracticeCollectionDto(collection);
    }
}

public class ListAdminPracticeCollectionsQueryHandler : IRequestHandler<ListAdminPracticeCollectionsQuery, PaginatedResult<AdminPracticeCollectionDto>>
{
    private readonly IApplicationDbContext _context;
    public ListAdminPracticeCollectionsQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<PaginatedResult<AdminPracticeCollectionDto>> Handle(ListAdminPracticeCollectionsQuery request, CancellationToken cancellationToken)
    {
        var collections = await _context.PracticeCollections.OrderBy(c => c.Title).ThenBy(c => c.Id).ToListAsync(cancellationToken);
        return new PaginatedResult<AdminPracticeCollectionDto> { Items = collections.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(PreparationPackageMapping.ToPracticeCollectionDto).ToList(), Page = request.Page, PageSize = request.PageSize, TotalCount = collections.Count };
    }
}

public class CreateAdminPracticeCollectionVersionCommandHandler : IRequestHandler<CreateAdminPracticeCollectionVersionCommand, AdminPracticeCollectionVersionDto>
{
    private readonly IApplicationDbContext _context;
    public CreateAdminPracticeCollectionVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPracticeCollectionVersionDto> Handle(CreateAdminPracticeCollectionVersionCommand request, CancellationToken cancellationToken)
    {
        var collectionExists = await _context.PracticeCollections.AnyAsync(c => c.Id == request.PracticeCollectionId, cancellationToken);
        if (!collectionExists) throw new KeyNotFoundException("Practice collection was not found.");
        await PracticeItemTopicValidator.EnsureTopicsExistAndAreActiveAsync(_context, request.Request.Items.Select(item => item.ReportingTopicId), cancellationToken);
        var nextVersion = await _context.PracticeCollectionVersions.Where(v => v.PracticeCollectionId == request.PracticeCollectionId).Select(v => v.VersionNumber).DefaultIfEmpty().MaxAsync(cancellationToken) + 1;
        var version = PracticeCollectionVersion.CreateDraft(request.PracticeCollectionId, nextVersion);
        AddPracticeItems(version, request.Request.Items);
        _context.PracticeCollectionVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPracticeCollectionVersionDto(version);
    }

    internal static void AddPracticeItems(PracticeCollectionVersion version, IEnumerable<UpsertAdminPracticeItemRequest> items)
    {
        foreach (var itemRequest in items.OrderBy(i => i.DisplayOrder))
        {
            var item = PracticeItem.Create(itemRequest.ReportingTopicId, itemRequest.Prompt, itemRequest.ImmediateFeedback, itemRequest.DisplayOrder);
            foreach (var option in itemRequest.AnswerOptions.OrderBy(o => o.DisplayOrder))
            {
                item.AddAnswerOption(option.OptionText, option.IsCorrect, option.DisplayOrder);
            }

            version.AddPracticeItem(item);
        }
    }
}

public class UpdateAdminPracticeCollectionVersionCommandHandler : IRequestHandler<UpdateAdminPracticeCollectionVersionCommand, AdminPracticeCollectionVersionDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateAdminPracticeCollectionVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPracticeCollectionVersionDto> Handle(UpdateAdminPracticeCollectionVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _context.PracticeCollectionVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PracticeCollectionId == request.PracticeCollectionId, cancellationToken)
            ?? throw new KeyNotFoundException("Practice collection version was not found.");
        await PracticeItemTopicValidator.EnsureTopicsExistAndAreActiveAsync(_context, request.Request.Items.Select(item => item.ReportingTopicId), cancellationToken);
        var replacement = PracticeCollectionVersion.CreateDraft(version.PracticeCollectionId, version.VersionNumber);
        CreateAdminPracticeCollectionVersionCommandHandler.AddPracticeItems(replacement, request.Request.Items);
        version.ReplaceDraftItems(replacement.Items);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPracticeCollectionVersionDto(version);
    }
}

internal static class PracticeItemTopicValidator
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
            throw new InvalidOperationException("Practice item topics must exist and be active.");
        }
    }
}

public class PublishAdminPracticeCollectionVersionCommandHandler : IRequestHandler<PublishAdminPracticeCollectionVersionCommand, AdminPracticeCollectionVersionDto>
{
    private readonly IApplicationDbContext _context;
    public PublishAdminPracticeCollectionVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPracticeCollectionVersionDto> Handle(PublishAdminPracticeCollectionVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _context.PracticeCollectionVersions
            .Include(v => v.Items)
            .ThenInclude(i => i.AnswerOptions)
            .FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PracticeCollectionId == request.PracticeCollectionId, cancellationToken)
            ?? throw new KeyNotFoundException("Practice collection version was not found.");
        foreach (var topicId in version.Items.Select(i => i.ReportingTopicId).Distinct())
        {
            var exists = await _context.ReportingTopics.AnyAsync(t => t.Id == topicId && t.IsActive, cancellationToken);
            if (!exists) throw new InvalidOperationException("Practice item topics must exist and be active.");
        }

        version.Publish(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPracticeCollectionVersionDto(version);
    }
}

public class RetireAdminPracticeCollectionVersionCommandHandler : IRequestHandler<RetireAdminPracticeCollectionVersionCommand, AdminPracticeCollectionVersionDto>
{
    private readonly IApplicationDbContext _context;
    public RetireAdminPracticeCollectionVersionCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task<AdminPracticeCollectionVersionDto> Handle(RetireAdminPracticeCollectionVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await _context.PracticeCollectionVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PracticeCollectionId == request.PracticeCollectionId, cancellationToken)
            ?? throw new KeyNotFoundException("Practice collection version was not found.");
        version.Retire(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToPracticeCollectionVersionDto(version);
    }
}
