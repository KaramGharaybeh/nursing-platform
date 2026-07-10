using FluentValidation.TestHelper;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerProfile;

namespace NursingPlatform.Application.Tests.Employers;

public class UpsertMyEmployerProfileCommandValidatorTests
{
    private readonly UpsertMyEmployerProfileCommandValidator _validator = new();

    [Fact]
    public void Validator_RejectsJobTitleLongerThan160()
    {
        var command = new UpsertMyEmployerProfileCommand { JobTitle = new string('A', 161) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.JobTitle);
    }

    [Fact]
    public void Validator_RejectsDepartmentLongerThan160()
    {
        var command = new UpsertMyEmployerProfileCommand { Department = new string('A', 161) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Department);
    }
}
