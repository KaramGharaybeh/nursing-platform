using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Common;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Admin.ReportingTopics;

public class CreateAdminReportingTopicRequest
{
    public Guid ExamCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class UpdateAdminReportingTopicRequest : CreateAdminReportingTopicRequest;

public class ListAdminReportingTopicsQuery : IRequest<PaginatedResult<AdminReportingTopicDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ExamCategoryId { get; set; }
}

public class CreateAdminReportingTopicCommand : IRequest<AdminReportingTopicDto>
{
    public CreateAdminReportingTopicRequest Request { get; set; } = new();
}

public class UpdateAdminReportingTopicCommand : IRequest<AdminReportingTopicDto>
{
    public Guid Id { get; set; }
    public UpdateAdminReportingTopicRequest Request { get; set; } = new();
}

public class ArchiveAdminReportingTopicCommand : IRequest<AdminReportingTopicDto>
{
    public Guid Id { get; set; }
}

public class ListAdminReportingTopicsQueryValidator : AbstractValidator<ListAdminReportingTopicsQuery>
{
    public ListAdminReportingTopicsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ExamCategoryId).NotEqual(Guid.Empty).When(x => x.ExamCategoryId.HasValue);
    }
}

public class CreateAdminReportingTopicCommandValidator : AbstractValidator<CreateAdminReportingTopicCommand>
{
    public CreateAdminReportingTopicCommandValidator()
    {
        RuleFor(x => x.Request.ExamCategoryId).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
    }
}

public class UpdateAdminReportingTopicCommandValidator : AbstractValidator<UpdateAdminReportingTopicCommand>
{
    public UpdateAdminReportingTopicCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.ExamCategoryId).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
    }
}

public class ArchiveAdminReportingTopicCommandValidator : AbstractValidator<ArchiveAdminReportingTopicCommand>
{
    public ArchiveAdminReportingTopicCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class CreateAdminReportingTopicCommandHandler : IRequestHandler<CreateAdminReportingTopicCommand, AdminReportingTopicDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminReportingTopicCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<AdminReportingTopicDto> Handle(CreateAdminReportingTopicCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.Id == request.Request.ExamCategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("Exam category was not found.");
        var topic = ReportingTopic.Create(category.Id, request.Request.Name, request.Request.Slug, null);
        _context.ReportingTopics.Add(topic);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToReportingTopicDto(topic, category.Name);
    }
}

public class UpdateAdminReportingTopicCommandHandler : IRequestHandler<UpdateAdminReportingTopicCommand, AdminReportingTopicDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAdminReportingTopicCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<AdminReportingTopicDto> Handle(UpdateAdminReportingTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ReportingTopics.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Reporting topic was not found.");
        var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.Id == request.Request.ExamCategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("Exam category was not found.");
        topic.Update(category.Id, request.Request.Name, request.Request.Slug, null);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToReportingTopicDto(topic, category.Name);
    }
}

public class ArchiveAdminReportingTopicCommandHandler : IRequestHandler<ArchiveAdminReportingTopicCommand, AdminReportingTopicDto>
{
    private readonly IApplicationDbContext _context;

    public ArchiveAdminReportingTopicCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<AdminReportingTopicDto> Handle(ArchiveAdminReportingTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ReportingTopics.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Reporting topic was not found.");
        topic.Archive();
        await _context.SaveChangesAsync(cancellationToken);
        var categoryName = await _context.ExamCategories.Where(c => c.Id == topic.ExamCategoryId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
        return PreparationPackageMapping.ToReportingTopicDto(topic, categoryName);
    }
}

public class ListAdminReportingTopicsQueryHandler : IRequestHandler<ListAdminReportingTopicsQuery, PaginatedResult<AdminReportingTopicDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminReportingTopicsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<AdminReportingTopicDto>> Handle(ListAdminReportingTopicsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ReportingTopics.AsQueryable();
        if (request.ExamCategoryId.HasValue)
        {
            query = query.Where(t => t.ExamCategoryId == request.ExamCategoryId.Value);
        }

        var rows = await query
            .Join(_context.ExamCategories, t => t.ExamCategoryId, c => c.Id, (topic, category) => new { topic, category.Name })
            .OrderBy(r => r.Name)
            .ThenBy(r => r.topic.Name)
            .ThenBy(r => r.topic.Id)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AdminReportingTopicDto>
        {
            Items = rows.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(r => PreparationPackageMapping.ToReportingTopicDto(r.topic, r.Name)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count
        };
    }
}
