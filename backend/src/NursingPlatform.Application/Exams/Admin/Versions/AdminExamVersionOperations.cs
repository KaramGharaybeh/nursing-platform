using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Admin.Common;
using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Admin.Versions;

public class ListAdminExamVersionsQuery : IRequest<List<AdminExamVersionDto>>
{
    public Guid ExamId { get; set; }
}

public class GetAdminExamVersionQuery : IRequest<AdminExamVersionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
}

public class CreateAdminDraftExamVersionCommand : IRequest<AdminExamVersionDto>
{
    public Guid ExamId { get; set; }
}

public class ValidateAdminDraftExamVersionCommand : IRequest<AdminExamVersionValidationDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
}

public class PublishAdminDraftExamVersionCommand : IRequest<AdminExamVersionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
}

public class RetireAdminExamVersionCommand : IRequest<AdminExamVersionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
}

public class DeleteAdminDraftExamVersionCommand : IRequest
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
}

public class ListAdminExamVersionsQueryValidator : AbstractValidator<ListAdminExamVersionsQuery>
{
    public ListAdminExamVersionsQueryValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
    }
}

public class GetAdminExamVersionQueryValidator : AbstractValidator<GetAdminExamVersionQuery>
{
    public GetAdminExamVersionQueryValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class CreateAdminDraftExamVersionCommandValidator : AbstractValidator<CreateAdminDraftExamVersionCommand>
{
    public CreateAdminDraftExamVersionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
    }
}

public class ValidateAdminDraftExamVersionCommandValidator : AbstractValidator<ValidateAdminDraftExamVersionCommand>
{
    public ValidateAdminDraftExamVersionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class PublishAdminDraftExamVersionCommandValidator : AbstractValidator<PublishAdminDraftExamVersionCommand>
{
    public PublishAdminDraftExamVersionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class RetireAdminExamVersionCommandValidator : AbstractValidator<RetireAdminExamVersionCommand>
{
    public RetireAdminExamVersionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class DeleteAdminDraftExamVersionCommandValidator : AbstractValidator<DeleteAdminDraftExamVersionCommand>
{
    public DeleteAdminDraftExamVersionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class ListAdminExamVersionsQueryHandler : IRequestHandler<ListAdminExamVersionsQuery, List<AdminExamVersionDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminExamVersionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminExamVersionDto>> Handle(ListAdminExamVersionsQuery request, CancellationToken cancellationToken)
    {
        await EnsureExamExistsAsync(_context, request.ExamId, cancellationToken);

        var versions = await _context.ExamVersions
            .Where(v => v.ExamId == request.ExamId)
            .OrderByDescending(v => v.VersionNumber)
            .ThenBy(v => v.Id)
            .ToListAsync(cancellationToken);

        return versions.Select(AdminExamMapping.ToVersionDto).ToList();
    }

    internal static async Task<Exam> EnsureExamExistsAsync(
        IApplicationDbContext context,
        Guid examId,
        CancellationToken cancellationToken)
    {
        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId, cancellationToken);
        if (exam is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        return exam;
    }
}

public class GetAdminExamVersionQueryHandler : IRequestHandler<GetAdminExamVersionQuery, AdminExamVersionDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminExamVersionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamVersionDto> Handle(GetAdminExamVersionQuery request, CancellationToken cancellationToken)
    {
        var version = await LoadVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        return AdminExamMapping.ToVersionDto(version);
    }

    internal static async Task<ExamVersion> LoadVersionAsync(
        IApplicationDbContext context,
        Guid examId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var version = await context.ExamVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.ExamId == examId, cancellationToken);

        if (version is null)
        {
            throw new KeyNotFoundException("Exam version was not found.");
        }

        return version;
    }
}

public class CreateAdminDraftExamVersionCommandHandler : IRequestHandler<CreateAdminDraftExamVersionCommand, AdminExamVersionDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminDraftExamVersionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamVersionDto> Handle(CreateAdminDraftExamVersionCommand request, CancellationToken cancellationToken)
    {
        var exam = await ListAdminExamVersionsQueryHandler.EnsureExamExistsAsync(_context, request.ExamId, cancellationToken);
        if (exam.Status == ExamStatus.Archived)
        {
            throw new InvalidOperationException("Archived exams cannot create draft versions.");
        }

        var nextVersionNumber = await _context.ExamVersions
            .Where(v => v.ExamId == request.ExamId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync(cancellationToken) ?? 0;

        var version = new ExamVersion
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            VersionNumber = nextVersionNumber + 1,
            Status = ExamVersionStatus.Draft
        };
        _context.ExamVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToVersionDto(version);
    }
}

public class ValidateAdminDraftExamVersionCommandHandler : IRequestHandler<ValidateAdminDraftExamVersionCommand, AdminExamVersionValidationDto>
{
    private readonly IApplicationDbContext _context;

    public ValidateAdminDraftExamVersionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<AdminExamVersionValidationDto> Handle(ValidateAdminDraftExamVersionCommand request, CancellationToken cancellationToken)
    {
        return AdminExamContentValidator.ValidateDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
    }
}

public class PublishAdminDraftExamVersionCommandHandler : IRequestHandler<PublishAdminDraftExamVersionCommand, AdminExamVersionDto>
{
    private readonly IApplicationDbContext _context;

    public PublishAdminDraftExamVersionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamVersionDto> Handle(PublishAdminDraftExamVersionCommand request, CancellationToken cancellationToken)
    {
        var exam = await ListAdminExamVersionsQueryHandler.EnsureExamExistsAsync(_context, request.ExamId, cancellationToken);
        var version = await GetAdminExamVersionQueryHandler.LoadVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        if (version.Status != ExamVersionStatus.Draft)
        {
            throw new InvalidOperationException("Only draft exam versions can be published.");
        }

        var validation = await AdminExamContentValidator.ValidateDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join(" ", validation.Errors));
        }

        var now = DateTime.UtcNow;
        version.Status = ExamVersionStatus.Published;
        version.QuestionCount = validation.QuestionCount;
        version.TotalPoints = validation.TotalPoints;
        version.PublishedAt = now;
        if (exam.Status != ExamStatus.Published)
        {
            exam.Status = ExamStatus.Published;
            exam.PublishedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToVersionDto(version);
    }
}

public class RetireAdminExamVersionCommandHandler : IRequestHandler<RetireAdminExamVersionCommand, AdminExamVersionDto>
{
    private readonly IApplicationDbContext _context;

    public RetireAdminExamVersionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamVersionDto> Handle(RetireAdminExamVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await GetAdminExamVersionQueryHandler.LoadVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        if (version.Status != ExamVersionStatus.Published)
        {
            throw new InvalidOperationException("Only published exam versions can be retired.");
        }

        version.Status = ExamVersionStatus.Retired;
        version.RetiredAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToVersionDto(version);
    }
}

public class DeleteAdminDraftExamVersionCommandHandler : IRequestHandler<DeleteAdminDraftExamVersionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAdminDraftExamVersionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAdminDraftExamVersionCommand request, CancellationToken cancellationToken)
    {
        var version = await GetAdminExamVersionQueryHandler.LoadVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        if (version.Status != ExamVersionStatus.Draft)
        {
            throw new InvalidOperationException("Only draft exam versions can be deleted.");
        }

        var hasSessions = await _context.ExamSessions.AnyAsync(s => s.ExamVersionId == version.Id, cancellationToken);
        if (hasSessions)
        {
            throw new InvalidOperationException("Exam versions with sessions cannot be deleted.");
        }

        var questionIds = await _context.ExamQuestions
            .Where(q => q.ExamVersionId == version.Id)
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);
        var options = await _context.ExamAnswerOptions.Where(o => questionIds.Contains(o.ExamQuestionId)).ToListAsync(cancellationToken);
        var questions = await _context.ExamQuestions.Where(q => q.ExamVersionId == version.Id).ToListAsync(cancellationToken);
        _context.ExamAnswerOptions.RemoveRange(options);
        _context.ExamQuestions.RemoveRange(questions);
        _context.ExamVersions.Remove(version);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
