using FluentValidation;
using MediatR;
using NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs;

namespace NursingPlatform.Application.PreparationPackages.ExamSessions.GetMyPackageExamSessionState;

public sealed record GetMyPackageExamSessionStateQuery(Guid EntitlementId) : IRequest<PackageExamSessionStateDto>;

public sealed class GetMyPackageExamSessionStateQueryValidator : AbstractValidator<GetMyPackageExamSessionStateQuery>
{
    public GetMyPackageExamSessionStateQueryValidator()
    {
        RuleFor(query => query.EntitlementId).NotEmpty();
    }
}
