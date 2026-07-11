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
}
