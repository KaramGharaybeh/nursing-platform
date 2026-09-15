using FluentValidation.TestHelper;
using NursingPlatform.Application.Nurses.Commands.CreateNurseExperience;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class NurseExperienceCommandValidatorTests
{
    private readonly CreateNurseExperienceCommandValidator _validator = new();

    [Fact]
    public void Create_JobTitleAtContractMaximum_IsValid()
    {
        var command = new CreateNurseExperienceCommand
        {
            FacilityName = "General Hospital",
            JobTitle = new string('N', 200),
            StartDate = new DateOnly(2024, 1, 1)
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.JobTitle);
    }

    [Fact]
    public void Create_JobTitleBeyondContractMaximum_IsInvalid()
    {
        var command = new CreateNurseExperienceCommand
        {
            FacilityName = "General Hospital",
            JobTitle = new string('N', 201),
            StartDate = new DateOnly(2024, 1, 1)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.JobTitle);
    }

    [Fact]
    public void Create_EndDateBeforeStartDate_IsInvalid()
    {
        var command = new CreateNurseExperienceCommand
        {
            FacilityName = "General Hospital",
            JobTitle = "Registered Nurse",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2023, 12, 31)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.EndDate);
    }

    [Fact]
    public void Create_CurrentExperienceWithEndDate_IsInvalid()
    {
        var command = new CreateNurseExperienceCommand
        {
            FacilityName = "General Hospital",
            JobTitle = "Registered Nurse",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 6, 1),
            IsCurrent = true
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.EndDate);
    }
}
