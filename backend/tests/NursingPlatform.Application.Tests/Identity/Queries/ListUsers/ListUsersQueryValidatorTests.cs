using FluentValidation.TestHelper;
using NursingPlatform.Application.Identity.Queries.ListUsers;

namespace NursingPlatform.Application.Tests.Identity.Queries.ListUsers;

public class ListUsersQueryValidatorTests
{
    private readonly ListUsersQueryValidator _validator = new();

    [Fact]
    public void Validate_PageLessThanOne_ShouldHaveError()
    {
        var query = new ListUsersQuery { Page = 0 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_PageSizeLessThanOne_ShouldHaveError()
    {
        var query = new ListUsersQuery { PageSize = 0 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_PageSizeGreaterThan100_ShouldHaveError()
    {
        var query = new ListUsersQuery { PageSize = 101 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_InvalidSort_ShouldHaveError()
    {
        var query = new ListUsersQuery { Sort = "invalid" };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Sort);
    }

    [Fact]
    public void Validate_NullSort_ShouldNotHaveError()
    {
        var query = new ListUsersQuery { Sort = null };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Sort);
    }

    [Fact]
    public void Validate_EmptySort_ShouldNotHaveError()
    {
        var query = new ListUsersQuery { Sort = "" };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Sort);
    }

    [Theory]
    [InlineData("email")]
    [InlineData("-email")]
    [InlineData("firstName")]
    [InlineData("-firstName")]
    [InlineData("lastName")]
    [InlineData("-lastName")]
    [InlineData("createdAt")]
    [InlineData("-createdAt")]
    [InlineData("lastLoginAt")]
    [InlineData("-lastLoginAt")]
    public void Validate_ValidSortValues_ShouldNotHaveError(string sort)
    {
        var query = new ListUsersQuery { Sort = sort };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Sort);
    }
}
