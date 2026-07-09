using FluentValidation.TestHelper;
using NursingPlatform.Application.Identity.Queries.GetUser;

namespace NursingPlatform.Application.Tests.Identity.Queries.GetUser;

public class GetUserQueryValidatorTests
{
    private readonly GetUserQueryValidator _validator = new();

    [Fact]
    public void Validate_EmptyUserId_ShouldHaveError()
    {
        var query = new GetUserQuery { UserId = Guid.Empty };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_NonEmptyUserId_ShouldNotHaveError()
    {
        var query = new GetUserQuery { UserId = Guid.NewGuid() };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}
