using FluentValidation;
using MediatR;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

namespace NursingPlatform.Application.PreparationPackages.PracticeProgress;

public sealed record GetPackagePracticeProgressQuery(Guid EntitlementId) : IRequest<PackagePracticeProgressSummaryDto>;

public sealed class GetPackagePracticeProgressQueryValidator : AbstractValidator<GetPackagePracticeProgressQuery>
{
    public GetPackagePracticeProgressQueryValidator()
    {
        RuleFor(query => query.EntitlementId).NotEmpty();
    }
}
