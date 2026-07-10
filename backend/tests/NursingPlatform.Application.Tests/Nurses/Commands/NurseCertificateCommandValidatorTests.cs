using FluentValidation.TestHelper;
using NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class NurseCertificateCommandValidatorTests
{
    private readonly CreateNurseCertificateCommandValidator _validator = new();

    [Fact]
    public void CreateCertificate_ExpirationBeforeIssueDate_IsInvalid()
    {
        var command = new CreateNurseCertificateCommand
        {
            Name = "Critical Care Certificate",
            IssuingOrganization = "Nursing Board",
            IssueDate = new DateOnly(2024, 1, 1),
            ExpirationDate = new DateOnly(2023, 12, 31)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ExpirationDate);
    }

    [Fact]
    public void CreateCertificate_InvalidCredentialUrl_IsInvalid()
    {
        var command = new CreateNurseCertificateCommand
        {
            Name = "Critical Care Certificate",
            IssuingOrganization = "Nursing Board",
            CredentialUrl = "not-a-url"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CredentialUrl);
    }
}
