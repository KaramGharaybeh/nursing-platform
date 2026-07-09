using NursingPlatform.Application.Identity.Commands.ResetPassword;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class ResetPasswordCommandValidatorTests
{
    [Fact]
    public void Validate_EmptyEmail_ReturnsError()
    {
        var result = Validate(new ResetPasswordCommand { Email = string.Empty, Token = "token", NewPassword = "NewPass1!" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.Email));
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsError()
    {
        var result = Validate(new ResetPasswordCommand { Email = "not-an-email", Token = "token", NewPassword = "NewPass1!" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.Email));
    }

    [Fact]
    public void Validate_EmptyToken_ReturnsError()
    {
        var result = Validate(new ResetPasswordCommand { Email = "user@test.com", Token = string.Empty, NewPassword = "NewPass1!" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.Token));
    }

    [Fact]
    public void Validate_EmptyPassword_ReturnsError()
    {
        var result = Validate(new ResetPasswordCommand { Email = "user@test.com", Token = "token", NewPassword = string.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswordTooShort_ReturnsError()
    {
        var result = Validate(new ResetPasswordCommand { Email = "user@test.com", Token = "token", NewPassword = "Short1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswordMissingUppercase_ReturnsError()
    {
        var result = Validate(new ResetPasswordCommand { Email = "user@test.com", Token = "token", NewPassword = "newpass1!" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswordMissingDigit_ReturnsError()
    {
        var result = Validate(new ResetPasswordCommand { Email = "user@test.com", Token = "token", NewPassword = "NewPassword!" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsValid()
    {
        var result = Validate(new ResetPasswordCommand { Email = "user@test.com", Token = "token", NewPassword = "NewPass1!" });

        Assert.True(result.IsValid);
    }

    private static FluentValidation.Results.ValidationResult Validate(ResetPasswordCommand command)
    {
        var validator = new ResetPasswordCommandValidator();
        return validator.Validate(command);
    }
}
