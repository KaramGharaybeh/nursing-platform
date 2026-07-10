using FluentValidation.TestHelper;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseLanguages;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class UpdateNurseLanguagesCommandValidatorTests
{
    private readonly UpdateNurseLanguagesCommandValidator _validator = new();

    [Fact]
    public void UpdateLanguages_DuplicateLanguageIds_IsInvalid()
    {
        var languageId = Guid.NewGuid();
        var command = new UpdateNurseLanguagesCommand
        {
            Languages =
            [
                new UpdateNurseLanguageRequest { LanguageId = languageId, Proficiency = "Fluent" },
                new UpdateNurseLanguageRequest { LanguageId = languageId, Proficiency = "Native" }
            ]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Languages);
    }

    [Fact]
    public void UpdateLanguages_MoreThan20Languages_IsInvalid()
    {
        var command = new UpdateNurseLanguagesCommand
        {
            Languages = Enumerable.Range(0, 21)
                .Select(_ => new UpdateNurseLanguageRequest { LanguageId = Guid.NewGuid(), Proficiency = "Intermediate" })
                .ToList()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Languages);
    }

    [Fact]
    public void UpdateLanguages_InvalidProficiency_IsInvalid()
    {
        var command = new UpdateNurseLanguagesCommand
        {
            Languages =
            [
                new UpdateNurseLanguageRequest { LanguageId = Guid.NewGuid(), Proficiency = "Conversational" }
            ]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Languages[0].Proficiency");
    }
}
