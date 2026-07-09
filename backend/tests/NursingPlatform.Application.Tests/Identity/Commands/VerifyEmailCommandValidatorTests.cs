using NursingPlatform.Application.Identity.Commands.VerifyEmail;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class VerifyEmailCommandValidatorTests
{
    [Fact]
    public void Validate_EmptyToken_ReturnsError()
    {
        var validator = new VerifyEmailCommandValidator();
        var command = new VerifyEmailCommand { Token = string.Empty };

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VerifyEmailCommand.Token));
    }

    [Fact]
    public void Validate_Token_ReturnsValid()
    {
        var validator = new VerifyEmailCommandValidator();
        var command = new VerifyEmailCommand { Token = "raw-token" };

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
