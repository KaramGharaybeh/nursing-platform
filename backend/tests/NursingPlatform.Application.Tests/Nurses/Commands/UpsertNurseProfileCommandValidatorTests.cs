using FluentValidation.TestHelper;
using NursingPlatform.Application.Nurses.Commands.UpsertNurseProfile;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class UpsertNurseProfileCommandValidatorTests
{
    private readonly UpsertNurseProfileCommandValidator _validator = new();

    [Fact]
    public void Validate_YearsOfExperienceGreaterThan80_IsInvalid()
    {
        var command = new UpsertNurseProfileCommand { YearsOfExperience = 81 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.YearsOfExperience);
    }
}
