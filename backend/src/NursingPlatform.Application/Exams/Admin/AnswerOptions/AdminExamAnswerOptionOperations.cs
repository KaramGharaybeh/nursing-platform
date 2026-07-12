using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Exams.Admin.Common;
using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Application.Exams.Admin.Questions;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Admin.AnswerOptions;

public class UpsertAdminExamAnswerOptionRequest
{
    public string OptionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ListAdminExamAnswerOptionsQuery : IRequest<List<AdminExamAnswerOptionDto>>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
}

public class CreateAdminExamAnswerOptionCommand : IRequest<AdminExamAnswerOptionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
    public UpsertAdminExamAnswerOptionRequest Request { get; set; } = new();
}

public class UpdateAdminExamAnswerOptionCommand : IRequest<AdminExamAnswerOptionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
    public UpsertAdminExamAnswerOptionRequest Request { get; set; } = new();
}

public class DeactivateAdminExamAnswerOptionCommand : IRequest<AdminExamAnswerOptionDto>
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
}

public class DeleteAdminExamAnswerOptionCommand : IRequest
{
    public Guid ExamId { get; set; }
    public Guid VersionId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
}

public class UpsertAdminExamAnswerOptionRequestValidator : AbstractValidator<UpsertAdminExamAnswerOptionRequest>
{
    public UpsertAdminExamAnswerOptionRequestValidator()
    {
        RuleFor(x => x.OptionText).NotEmpty().MaximumLength(2000);
    }
}

public class CreateAdminExamAnswerOptionCommandValidator : AbstractValidator<CreateAdminExamAnswerOptionCommand>
{
    public CreateAdminExamAnswerOptionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new UpsertAdminExamAnswerOptionRequestValidator());
    }
}

public class UpdateAdminExamAnswerOptionCommandValidator : AbstractValidator<UpdateAdminExamAnswerOptionCommand>
{
    public UpdateAdminExamAnswerOptionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.OptionId).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new UpsertAdminExamAnswerOptionRequestValidator());
    }
}

public class ListAdminExamAnswerOptionsQueryValidator : AbstractValidator<ListAdminExamAnswerOptionsQuery>
{
    public ListAdminExamAnswerOptionsQueryValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
    }
}

public class DeactivateAdminExamAnswerOptionCommandValidator : AbstractValidator<DeactivateAdminExamAnswerOptionCommand>
{
    public DeactivateAdminExamAnswerOptionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.OptionId).NotEmpty();
    }
}

public class DeleteAdminExamAnswerOptionCommandValidator : AbstractValidator<DeleteAdminExamAnswerOptionCommand>
{
    public DeleteAdminExamAnswerOptionCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.OptionId).NotEmpty();
    }
}

public class ListAdminExamAnswerOptionsQueryHandler : IRequestHandler<ListAdminExamAnswerOptionsQuery, List<AdminExamAnswerOptionDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminExamAnswerOptionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminExamAnswerOptionDto>> Handle(ListAdminExamAnswerOptionsQuery request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        var options = await _context.ExamAnswerOptions
            .Where(o => o.ExamQuestionId == request.QuestionId)
            .OrderBy(o => o.DisplayOrder)
            .ThenBy(o => o.Id)
            .ToListAsync(cancellationToken);
        return options.Select(AdminExamMapping.ToOptionDto).ToList();
    }
}

public class CreateAdminExamAnswerOptionCommandHandler : IRequestHandler<CreateAdminExamAnswerOptionCommand, AdminExamAnswerOptionDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminExamAnswerOptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamAnswerOptionDto> Handle(CreateAdminExamAnswerOptionCommand request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.EnsureDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        await GetAdminExamQuestionQueryHandler.LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        var option = new ExamAnswerOption
        {
            Id = Guid.NewGuid(),
            ExamQuestionId = request.QuestionId,
            OptionText = request.Request.OptionText.Trim(),
            DisplayOrder = request.Request.DisplayOrder,
            IsCorrect = request.Request.IsCorrect,
            IsActive = request.Request.IsActive
        };
        _context.ExamAnswerOptions.Add(option);
        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToOptionDto(option);
    }
}

public class UpdateAdminExamAnswerOptionCommandHandler : IRequestHandler<UpdateAdminExamAnswerOptionCommand, AdminExamAnswerOptionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAdminExamAnswerOptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamAnswerOptionDto> Handle(UpdateAdminExamAnswerOptionCommand request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.EnsureDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        await GetAdminExamQuestionQueryHandler.LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        var option = await LoadOptionAsync(_context, request.QuestionId, request.OptionId, cancellationToken);
        option.OptionText = request.Request.OptionText.Trim();
        option.DisplayOrder = request.Request.DisplayOrder;
        option.IsCorrect = request.Request.IsCorrect;
        option.IsActive = request.Request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToOptionDto(option);
    }

    internal static async Task<ExamAnswerOption> LoadOptionAsync(
        IApplicationDbContext context,
        Guid questionId,
        Guid optionId,
        CancellationToken cancellationToken)
    {
        var option = await context.ExamAnswerOptions
            .FirstOrDefaultAsync(o => o.Id == optionId && o.ExamQuestionId == questionId, cancellationToken);
        if (option is null)
        {
            throw new KeyNotFoundException("Exam answer option was not found.");
        }

        return option;
    }
}

public class DeactivateAdminExamAnswerOptionCommandHandler : IRequestHandler<DeactivateAdminExamAnswerOptionCommand, AdminExamAnswerOptionDto>
{
    private readonly IApplicationDbContext _context;

    public DeactivateAdminExamAnswerOptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamAnswerOptionDto> Handle(DeactivateAdminExamAnswerOptionCommand request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.EnsureDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        await GetAdminExamQuestionQueryHandler.LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        var option = await UpdateAdminExamAnswerOptionCommandHandler.LoadOptionAsync(_context, request.QuestionId, request.OptionId, cancellationToken);
        option.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToOptionDto(option);
    }
}

public class DeleteAdminExamAnswerOptionCommandHandler : IRequestHandler<DeleteAdminExamAnswerOptionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAdminExamAnswerOptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAdminExamAnswerOptionCommand request, CancellationToken cancellationToken)
    {
        await GetAdminExamQuestionQueryHandler.EnsureDraftVersionAsync(_context, request.ExamId, request.VersionId, cancellationToken);
        await GetAdminExamQuestionQueryHandler.LoadQuestionAsync(_context, request.ExamId, request.VersionId, request.QuestionId, cancellationToken);
        var option = await UpdateAdminExamAnswerOptionCommandHandler.LoadOptionAsync(_context, request.QuestionId, request.OptionId, cancellationToken);
        var referenced = await _context.ExamSessionAnswerOptions.AnyAsync(o => o.ExamAnswerOptionId == option.Id, cancellationToken);
        if (referenced)
        {
            throw new InvalidOperationException("Answer options referenced by session snapshots cannot be deleted.");
        }

        _context.ExamAnswerOptions.Remove(option);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
