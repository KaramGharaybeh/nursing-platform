using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Admin.Common;
using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Application.Exams.Admin.Versions;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Admin.Questions;

public class UpsertAdminExamQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public ExamQuestionType QuestionType { get; set; } = ExamQuestionType.SingleBestAnswer;
    public int Points { get; set; } = 1;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ListAdminExamQuestionsQuery : IRequest<List<AdminExamQuestionDto>>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
}

public class GetAdminExamQuestionQuery : IRequest<AdminExamQuestionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
}

public class CreateAdminExamQuestionCommand : IRequest<AdminExamQuestionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public UpsertAdminExamQuestionRequest Request { get; set; } = new();
}

public class UpdateAdminExamQuestionCommand : IRequest<AdminExamQuestionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
    public UpsertAdminExamQuestionRequest Request { get; set; } = new();
}

public class DeactivateAdminExamQuestionCommand : IRequest<AdminExamQuestionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
}

public class DeleteAdminExamQuestionCommand : IRequest
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
}

public class UpsertAdminExamQuestionRequestValidator : AbstractValidator<UpsertAdminExamQuestionRequest>
{
    public UpsertAdminExamQuestionRequestValidator()
    {
        RuleFor(x => x.QuestionText).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Explanation).MaximumLength(4000);
        RuleFor(x => x.QuestionType).Equal(ExamQuestionType.SingleBestAnswer);
        RuleFor(x => x.Points).GreaterThan(0);
    }
}

public class CreateAdminExamQuestionCommandValidator : AbstractValidator<CreateAdminExamQuestionCommand>
{
    public CreateAdminExamQuestionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new UpsertAdminExamQuestionRequestValidator());
    }
}

public class UpdateAdminExamQuestionCommandValidator : AbstractValidator<UpdateAdminExamQuestionCommand>
{
    public UpdateAdminExamQuestionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new UpsertAdminExamQuestionRequestValidator());
    }
}

public class ListAdminExamQuestionsQueryValidator : AbstractValidator<ListAdminExamQuestionsQuery>
{
    public ListAdminExamQuestionsQueryValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}

public class GetAdminExamQuestionQueryValidator : AbstractValidator<GetAdminExamQuestionQuery>
{
    public GetAdminExamQuestionQueryValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
    }
}

public class DeactivateAdminExamQuestionCommandValidator : AbstractValidator<DeactivateAdminExamQuestionCommand>
{
    public DeactivateAdminExamQuestionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
    }
}

public class DeleteAdminExamQuestionCommandValidator : AbstractValidator<DeleteAdminExamQuestionCommand>
{
    public DeleteAdminExamQuestionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
    }
}

public class ListAdminExamQuestionsQueryHandler : IRequestHandler<ListAdminExamQuestionsQuery, List<AdminExamQuestionDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminExamQuestionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminExamQuestionDto>> Handle(ListAdminExamQuestionsQuery request, CancellationToken cancellationToken)
    {
        await GetAdminExamVersionQueryHandler.LoadVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        var questions = await _context.ExamQuestions
            .Where(q => q.ExamVersionId == request.VersionId)
            .OrderBy(q => q.DisplayOrder)
            .ThenBy(q => q.Id)
            .ToListAsync(cancellationToken);
        var questionIds = questions.Select(q => q.Id).ToList();
        var options = await _context.ExamAnswerOptions
            .Where(o => questionIds.Contains(o.ExamQuestionId))
            .ToListAsync(cancellationToken);

        return questions.Select(q => AdminExamMapping.ToQuestionDto(q, options.Where(o => o.ExamQuestionId == q.Id))).ToList();
    }
}

public class GetAdminExamQuestionQueryHandler : IRequestHandler<GetAdminExamQuestionQuery, AdminExamQuestionDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminExamQuestionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamQuestionDto> Handle(GetAdminExamQuestionQuery request, CancellationToken cancellationToken)
    {
        var question = await LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        var options = await _context.ExamAnswerOptions.Where(o => o.ExamQuestionId == question.Id).ToListAsync(cancellationToken);
        return AdminExamMapping.ToQuestionDto(question, options);
    }

    internal static async Task<ExamQuestion> LoadQuestionAsync(
        IApplicationDbContext context,
        Guid examId,
        Guid versionId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        await GetAdminExamVersionQueryHandler.LoadVersionAsync(context, examId, versionId, cancellationToken);
        var question = await context.ExamQuestions
            .FirstOrDefaultAsync(q => q.Id == questionId && q.ExamVersionId == versionId, cancellationToken);
        if (question is null)
        {
            throw new KeyNotFoundException("Exam question was not found.");
        }

        return question;
    }

    internal static async Task<ExamVersion> EnsureDraftVersionAsync(
        IApplicationDbContext context,
        Guid examId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var version = await GetAdminExamVersionQueryHandler.LoadVersionAsync(context, examId, versionId, cancellationToken);
        if (version.Status != ExamVersionStatus.Draft)
        {
            throw new InvalidOperationException("Published and retired exam versions are immutable.");
        }

        return version;
    }
}

public class CreateAdminExamQuestionCommandHandler : IRequestHandler<CreateAdminExamQuestionCommand, AdminExamQuestionDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminExamQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamQuestionDto> Handle(CreateAdminExamQuestionCommand request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.EnsureDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = request.VersionId,
            QuestionText = request.Request.QuestionText.Trim(),
            Explanation = request.Request.Explanation?.Trim(),
            QuestionType = request.Request.QuestionType,
            Points = request.Request.Points,
            DisplayOrder = request.Request.DisplayOrder,
            IsActive = request.Request.IsActive
        };
        _context.ExamQuestions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToQuestionDto(question, []);
    }
}

public class UpdateAdminExamQuestionCommandHandler : IRequestHandler<UpdateAdminExamQuestionCommand, AdminExamQuestionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAdminExamQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamQuestionDto> Handle(UpdateAdminExamQuestionCommand request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.EnsureDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        var question = await GetAdminExamQuestionQueryHandler.LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        question.QuestionText = request.Request.QuestionText.Trim();
        question.Explanation = request.Request.Explanation?.Trim();
        question.QuestionType = request.Request.QuestionType;
        question.Points = request.Request.Points;
        question.DisplayOrder = request.Request.DisplayOrder;
        question.IsActive = request.Request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        var options = await _context.ExamAnswerOptions.Where(o => o.ExamQuestionId == question.Id).ToListAsync(cancellationToken);
        return AdminExamMapping.ToQuestionDto(question, options);
    }
}

public class DeactivateAdminExamQuestionCommandHandler : IRequestHandler<DeactivateAdminExamQuestionCommand, AdminExamQuestionDto>
{
    private readonly IApplicationDbContext _context;

    public DeactivateAdminExamQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamQuestionDto> Handle(DeactivateAdminExamQuestionCommand request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.EnsureDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        var question = await GetAdminExamQuestionQueryHandler.LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        question.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        var options = await _context.ExamAnswerOptions.Where(o => o.ExamQuestionId == question.Id).ToListAsync(cancellationToken);
        return AdminExamMapping.ToQuestionDto(question, options);
    }
}

public class DeleteAdminExamQuestionCommandHandler : IRequestHandler<DeleteAdminExamQuestionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAdminExamQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAdminExamQuestionCommand request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.EnsureDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        var question = await GetAdminExamQuestionQueryHandler.LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        var referenced = await _context.ExamSessionQuestions.AnyAsync(q => q.ExamQuestionId == question.Id, cancellationToken);
        if (referenced)
        {
            throw new InvalidOperationException("Questions referenced by session snapshots cannot be deleted.");
        }

        var options = await _context.ExamAnswerOptions.Where(o => o.ExamQuestionId == question.Id).ToListAsync(cancellationToken);
        _context.ExamAnswerOptions.RemoveRange(options);
        _context.ExamQuestions.Remove(question);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
