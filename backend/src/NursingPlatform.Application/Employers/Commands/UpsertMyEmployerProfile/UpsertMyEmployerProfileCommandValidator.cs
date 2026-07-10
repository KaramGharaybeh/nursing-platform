using FluentValidation;

namespace NursingPlatform.Application.Employers.Commands.UpsertMyEmployerProfile;

public class UpsertMyEmployerProfileCommandValidator : AbstractValidator<UpsertMyEmployerProfileCommand>
{
    public UpsertMyEmployerProfileCommandValidator()
    {
        RuleFor(x => x.JobTitle).MaximumLength(160);
        RuleFor(x => x.Department).MaximumLength(160);
    }
}
