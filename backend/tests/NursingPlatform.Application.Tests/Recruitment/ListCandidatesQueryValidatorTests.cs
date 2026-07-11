using FluentValidation.TestHelper;
using NursingPlatform.Application.Recruitment.Queries.ListCandidates;

namespace NursingPlatform.Application.Tests.Recruitment;

public class ListCandidatesQueryValidatorTests
{
    private readonly ListCandidatesQueryValidator _validator = new();

    [Fact]
    public void Validate_PageLessThanOne_ShouldHaveError()
    {
        var query = new ListCandidatesQuery { Page = 0 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_PageSizeLessThanOne_ShouldHaveError()
    {
        var query = new ListCandidatesQuery { PageSize = 0 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_PageSizeGreaterThan100_ShouldHaveError()
    {
        var query = new ListCandidatesQuery { PageSize = 101 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_MinimumYearsOfExperienceLessThanZero_ShouldHaveError()
    {
        var query = new ListCandidatesQuery { MinimumYearsOfExperience = -1 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.MinimumYearsOfExperience);
    }

    [Fact]
    public void Validate_BlankSkill_ShouldHaveError()
    {
        var query = new ListCandidatesQuery { Skills = ["ICU", " , Triage"] };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Skills);
    }

    [Fact]
    public void Validate_MoreThanTwentyNormalizedSkills_ShouldHaveError()
    {
        var query = new ListCandidatesQuery
        {
            Skills = Enumerable.Range(1, 21).Select(i => $"Skill {i}").ToList()
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Skills);
    }

    [Fact]
    public void Validate_DuplicateNormalizedSkillsWithinLimit_ShouldNotHaveError()
    {
        var skills = Enumerable.Range(1, 20)
            .Select(i => $"Skill {i}")
            .Append(" skill 1 ")
            .ToList();
        var query = new ListCandidatesQuery { Skills = skills };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Skills);
    }

    [Fact]
    public void Validate_OverLengthSkill_ShouldHaveError()
    {
        var query = new ListCandidatesQuery { Skills = [new string('A', 101)] };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Skills);
    }
}
