using FluentValidation;
using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.DTOs;

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
