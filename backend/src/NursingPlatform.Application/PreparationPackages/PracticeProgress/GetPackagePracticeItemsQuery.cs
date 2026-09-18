using FluentValidation;
using MediatR;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

namespace NursingPlatform.Application.PreparationPackages.PracticeProgress;

public sealed record GetPackagePracticeItemsQuery(Guid EntitlementId) : IRequest<PackagePracticeContentListDto>;

public sealed class GetPackagePracticeItemsQueryValidator : AbstractValidator<GetPackagePracticeItemsQuery>
{
    public GetPackagePracticeItemsQueryValidator()
    {
        RuleFor(query => query.EntitlementId).NotEmpty();
    }
}
