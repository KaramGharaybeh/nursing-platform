using FluentValidation;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseLanguages;

public class UpdateNurseLanguagesCommandValidator : AbstractValidator<UpdateNurseLanguagesCommand>
{
    private static readonly string[] AllowedProficiencies =
    [
        "Beginner",
        "Intermediate",
        "Advanced",
        "Fluent",
        "Native"
    ];

    public UpdateNurseLanguagesCommandValidator()
    {
        RuleFor(x => x.Languages)
            .Must(languages => languages.Count <= 20)
            .WithMessage("A nurse can have at most 20 languages.");

        RuleFor(x => x.Languages)
            .Must(languages => languages.Select(l => l.LanguageId).Distinct().Count() == languages.Count)
            .WithMessage("Duplicate language ids are not allowed.");

        RuleForEach(x => x.Languages).ChildRules(language =>
        {
            language.RuleFor(x => x.LanguageId).NotEmpty();
            language.RuleFor(x => x.Proficiency)
                .Must(value => AllowedProficiencies.Contains(value))
                .WithMessage("Language proficiency is invalid.");
        });
    }
}
