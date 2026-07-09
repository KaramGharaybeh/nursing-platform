using NursingPlatform.Application.Identity.Commands.ForgotPassword;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class ForgotPasswordCommandValidatorTests
{
    [Fact]
    public void Validate_EmptyEmail_ReturnsError()
    {
        var validator = new ForgotPasswordCommandValidator();
        var command = new ForgotPasswordCommand { Email = string.Empty };

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsError()
    {
        var validator = new ForgotPasswordCommandValidator();
        var command = new ForgotPasswordCommand { Email = "not-an-email" };

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
    }

    [Fact]
    public void Validate_ValidEmail_ReturnsValid()
    {
        var validator = new ForgotPasswordCommandValidator();
        var command = new ForgotPasswordCommand { Email = "user@test.com" };

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
