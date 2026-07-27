using FluentValidation;
using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.DTOs;

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
