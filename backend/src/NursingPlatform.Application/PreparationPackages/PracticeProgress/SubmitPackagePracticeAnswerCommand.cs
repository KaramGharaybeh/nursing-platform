using FluentValidation;
using MediatR;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

namespace NursingPlatform.Application.PreparationPackages.PracticeProgress;

public sealed class SubmitPackagePracticeAnswerRequest
{
    public Guid SelectedPracticeAnswerOptionId { get; init; }
}

public sealed record SubmitPackagePracticeAnswerCommand(
    Guid EntitlementId,
    Guid PracticeItemId,
    SubmitPackagePracticeAnswerRequest Request) : IRequest<PackagePracticeAnswerSubmissionDto>;

public sealed class SubmitPackagePracticeAnswerRequestValidator : AbstractValidator<SubmitPackagePracticeAnswerRequest>
{
    public SubmitPackagePracticeAnswerRequestValidator()
    {
        RuleFor(request => request.SelectedPracticeAnswerOptionId).NotEmpty();
    }
}

public sealed class SubmitPackagePracticeAnswerCommandValidator : AbstractValidator<SubmitPackagePracticeAnswerCommand>
{
    public SubmitPackagePracticeAnswerCommandValidator()
    {
        RuleFor(command => command.EntitlementId).NotEmpty();
        RuleFor(command => command.PracticeItemId).NotEmpty();
        RuleFor(command => command.Request).NotNull();
        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request).SetValidator(new SubmitPackagePracticeAnswerRequestValidator());
        });
    }
}
