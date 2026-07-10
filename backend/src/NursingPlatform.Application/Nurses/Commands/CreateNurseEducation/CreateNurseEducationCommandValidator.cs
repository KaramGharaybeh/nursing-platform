using FluentValidation;

namespace NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;

public class CreateNurseEducationCommandValidator : AbstractValidator<CreateNurseEducationCommand>
{
    public CreateNurseEducationCommandValidator()
    {
        Include(new UpsertNurseEducationRequestValidator<CreateNurseEducationCommand>());
    }
}

public class UpsertNurseEducationRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : UpsertNurseEducationRequest
{
    public UpsertNurseEducationRequestValidator()
    {
        RuleFor(x => x.InstitutionName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Degree)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.FieldOfStudy).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
