using FluentValidation.TestHelper;
using NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class NurseEducationCommandValidatorTests
{
    private readonly CreateNurseEducationCommandValidator _validator = new();

    [Fact]
    public void CreateEducation_DegreeAtContractMaximum_IsValid()
    {
        var command = new CreateNurseEducationCommand
        {
            InstitutionName = "University of Nursing",
            Degree = new string('B', 200)
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Degree);
    }

    [Fact]
    public void CreateEducation_DegreeBeyondContractMaximum_IsInvalid()
    {
        var command = new CreateNurseEducationCommand
        {
            InstitutionName = "University of Nursing",
            Degree = new string('B', 201)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Degree);
    }

    [Fact]
    public void CreateEducation_FieldOfStudyBeyondContractMaximum_IsInvalid()
    {
        var command = new CreateNurseEducationCommand
        {
            InstitutionName = "University of Nursing",
            Degree = "Bachelor of Nursing",
            FieldOfStudy = new string('S', 201)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FieldOfStudy);
    }

    [Fact]
    public void CreateEducation_EndDateBeforeStartDate_IsInvalid()
    {
        var command = new CreateNurseEducationCommand
        {
            InstitutionName = "University of Nursing",
            Degree = "Bachelor of Nursing",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2023, 12, 31)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.EndDate);
    }
}
