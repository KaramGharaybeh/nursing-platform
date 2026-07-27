using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Common;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Admin.ReportingProfiles;

public class CreateAdminReportingProfileRequest
{
    public Guid ExamVersionId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ReportingProfileQuestionAssignmentRequest
{
    public Guid ExamQuestionId { get; set; }
    public Guid ReportingTopicId { get; set; }
}

public class PublishAdminReportingProfileRequest
{
    public List<ReportingProfileQuestionAssignmentRequest> Assignments { get; set; } = [];
}

public class ListAdminReportingProfilesQuery : IRequest<PaginatedResult<AdminReportingProfilePublicationDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ExamVersionId { get; set; }
}

public class GetAdminReportingProfileQuery : IRequest<AdminReportingProfilePublicationDto>
{
    public Guid Id { get; set; }
}

public class CreateAdminReportingProfileCommand : IRequest<AdminReportingProfilePublicationDto>
{
    public CreateAdminReportingProfileRequest Request { get; set; } = new();
}

public class PublishAdminReportingProfileCommand : IRequest<AdminReportingProfilePublicationDto>
{
    public Guid Id { get; set; }
    public PublishAdminReportingProfileRequest Request { get; set; } = new();
}

public class ListAdminReportingProfilesQueryValidator : AbstractValidator<ListAdminReportingProfilesQuery>
{
    public ListAdminReportingProfilesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ExamVersionId).NotEqual(Guid.Empty).When(x => x.ExamVersionId.HasValue);
    }
}

public class GetAdminReportingProfileQueryValidator : AbstractValidator<GetAdminReportingProfileQuery>
{
    public GetAdminReportingProfileQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class CreateAdminReportingProfileCommandValidator : AbstractValidator<CreateAdminReportingProfileCommand>
{
    public CreateAdminReportingProfileCommandValidator()
    {
        RuleFor(x => x.Request.ExamVersionId).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
    }
}

public class PublishAdminReportingProfileCommandValidator : AbstractValidator<PublishAdminReportingProfileCommand>
{
    public PublishAdminReportingProfileCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Assignments).NotEmpty();
        RuleForEach(x => x.Request.Assignments).ChildRules(assignment =>
        {
            assignment.RuleFor(x => x.ExamQuestionId).NotEmpty();
            assignment.RuleFor(x => x.ReportingTopicId).NotEmpty();
        });
    }
}

public class CreateAdminReportingProfileCommandHandler : IRequestHandler<CreateAdminReportingProfileCommand, AdminReportingProfilePublicationDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminReportingProfileCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<AdminReportingProfilePublicationDto> Handle(CreateAdminReportingProfileCommand request, CancellationToken cancellationToken)
    {
        var versionExists = await _context.ExamVersions.AnyAsync(v => v.Id == request.Request.ExamVersionId, cancellationToken);
        if (!versionExists)
        {
            throw new KeyNotFoundException("Exam version was not found.");
        }

        var profile = ReportingProfilePublication.CreateDraft(request.Request.ExamVersionId, request.Request.Name);
        _context.ReportingProfilePublications.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);
        return PreparationPackageMapping.ToReportingProfileDto(profile, profile.Assignments, new Dictionary<Guid, ReportingTopic>());
    }
}

public class PublishAdminReportingProfileCommandHandler : IRequestHandler<PublishAdminReportingProfileCommand, AdminReportingProfilePublicationDto>
{
    private readonly IApplicationDbContext _context;

    public PublishAdminReportingProfileCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<AdminReportingProfilePublicationDto> Handle(PublishAdminReportingProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.ReportingProfilePublications.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Reporting profile was not found.");
        var assignments = request.Request.Assignments.Select(a => (a.ExamQuestionId, a.ReportingTopicId)).ToList();
        await new PreparationPackagePublicationValidator(_context).ValidateReportingProfilePublicationAsync(profile, assignments, cancellationToken);
        foreach (var assignment in request.Request.Assignments.OrderBy(a => a.ExamQuestionId))
        {
            profile.AssignQuestion(assignment.ExamQuestionId, assignment.ReportingTopicId);
        }

        profile.Publish(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        var topics = await _context.ReportingTopics.Where(t => request.Request.Assignments.Select(a => a.ReportingTopicId).Contains(t.Id)).ToDictionaryAsync(t => t.Id, cancellationToken);
        return PreparationPackageMapping.ToReportingProfileDto(profile, profile.Assignments, topics);
    }
}

public class GetAdminReportingProfileQueryHandler : IRequestHandler<GetAdminReportingProfileQuery, AdminReportingProfilePublicationDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminReportingProfileQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<AdminReportingProfilePublicationDto> Handle(GetAdminReportingProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.ReportingProfilePublications.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Reporting profile was not found.");
        var topicIds = profile.Assignments.Select(a => a.ReportingTopicId).ToList();
        var topics = await _context.ReportingTopics.Where(t => topicIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, cancellationToken);
        return PreparationPackageMapping.ToReportingProfileDto(profile, profile.Assignments, topics);
    }
}

public class ListAdminReportingProfilesQueryHandler : IRequestHandler<ListAdminReportingProfilesQuery, PaginatedResult<AdminReportingProfilePublicationDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminReportingProfilesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<AdminReportingProfilePublicationDto>> Handle(ListAdminReportingProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ReportingProfilePublications.AsQueryable();
        if (request.ExamVersionId.HasValue)
        {
            query = query.Where(p => p.ExamVersionId == request.ExamVersionId.Value);
        }

        var profiles = await query.OrderBy(p => p.Name).ThenBy(p => p.Id).ToListAsync(cancellationToken);
        var items = profiles.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(p => PreparationPackageMapping.ToReportingProfileDto(p, p.Assignments, new Dictionary<Guid, ReportingTopic>())).ToList();
        return new PaginatedResult<AdminReportingProfilePublicationDto> { Items = items, Page = request.Page, PageSize = request.PageSize, TotalCount = profiles.Count };
    }
}
