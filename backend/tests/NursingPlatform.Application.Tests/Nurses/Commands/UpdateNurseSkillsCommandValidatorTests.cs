using FluentValidation.TestHelper;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class UpdateNurseSkillsCommandValidatorTests
{
    private readonly UpdateNurseSkillsCommandValidator _validator = new();

    [Fact]
    public void UpdateSkills_NormalizedDuplicateNames_IsInvalid()
    {
        var command = new UpdateNurseSkillsCommand
        {
            Skills = ["Critical   Care", " critical care "]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Skills);
    }

    [Fact]
    public void UpdateSkills_MoreThan50Skills_IsInvalid()
    {
        var command = new UpdateNurseSkillsCommand
        {
            Skills = Enumerable.Range(0, 51).Select(i => $"Skill {i}").ToList()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Skills);
    }

    [Fact]
    public void UpdateSkills_EmptyOrWhitespaceName_IsInvalid()
    {
        var command = new UpdateNurseSkillsCommand { Skills = ["Triage", "   "] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Skills[1]");
    }

    [Fact]
    public void UpdateSkills_100CharacterName_IsValid()
    {
        var command = new UpdateNurseSkillsCommand { Skills = [new string('a', 100)] };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateSkills_101CharacterName_IsInvalid()
    {
        var command = new UpdateNurseSkillsCommand { Skills = ["Triage", new string('b', 101)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Skills[1]");
    }
}
