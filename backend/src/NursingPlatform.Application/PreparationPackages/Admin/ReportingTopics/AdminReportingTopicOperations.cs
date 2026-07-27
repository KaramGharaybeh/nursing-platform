using FluentValidation;
using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.DTOs;

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
