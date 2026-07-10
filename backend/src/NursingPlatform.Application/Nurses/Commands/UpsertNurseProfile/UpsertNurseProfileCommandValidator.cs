using FluentValidation;

namespace NursingPlatform.Application.Nurses.Commands.UpsertNurseProfile;

public class UpsertNurseProfileCommandValidator : AbstractValidator<UpsertNurseProfileCommand>
{
    public UpsertNurseProfileCommandValidator()
    {
        RuleFor(x => x.Headline).MaximumLength(160);
        RuleFor(x => x.ProfessionalSummary).MaximumLength(2000);
        RuleFor(x => x.LicenseNumber).MaximumLength(100);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 80);
    }
}
