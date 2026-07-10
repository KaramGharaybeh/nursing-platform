using FluentValidation;

namespace NursingPlatform.Application.Nurses.Commands.CreateNurseExperience;

public class CreateNurseExperienceCommandValidator : AbstractValidator<CreateNurseExperienceCommand>
{
    public CreateNurseExperienceCommandValidator()
    {
        Include(new UpsertNurseExperienceRequestValidator<CreateNurseExperienceCommand>());
    }
}

public class UpsertNurseExperienceRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : UpsertNurseExperienceRequest
{
    public UpsertNurseExperienceRequestValidator()
    {
        RuleFor(x => x.FacilityName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.JobTitle)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .Null()
            .When(x => x.IsCurrent);
    }
}
