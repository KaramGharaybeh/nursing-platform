using FluentValidation.TestHelper;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;

namespace NursingPlatform.Application.Tests.Employers;

public class UpsertMyEmployerOrganizationCommandValidatorTests
{
    private readonly UpsertMyEmployerOrganizationCommandValidator _validator = new();

    [Fact]
    public void Validator_RejectsMissingName()
    {
        var command = new UpsertMyEmployerOrganizationCommand { Name = "   " };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_RejectsNameLongerThan200()
    {
        var command = new UpsertMyEmployerOrganizationCommand { Name = new string('A', 201) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_RejectsTypeLongerThan100()
    {
        var command = CreateValidCommand();
        command.Type = new string('A', 101);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Validator_RejectsWebsiteUrlLongerThan500()
    {
        var command = CreateValidCommand();
        command.WebsiteUrl = $"https://example.com/{new string('a', 501)}";

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.WebsiteUrl);
    }

    [Fact]
    public void Validator_RejectsRelativeWebsiteUrl()
    {
        var command = CreateValidCommand();
        command.WebsiteUrl = "/careers";

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.WebsiteUrl);
    }

    [Fact]
    public void Validator_RejectsNonHttpHttpsAbsoluteWebsiteUrl()
    {
        var command = CreateValidCommand();
        command.WebsiteUrl = "ftp://example.com";

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.WebsiteUrl);
    }

    [Fact]
    public void Validator_RejectsCityLongerThan120()
    {
        var command = CreateValidCommand();
        command.City = new string('A', 121);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.City);
    }

    [Fact]
    public void Validator_RejectsAddressLine1LongerThan200()
    {
        var command = CreateValidCommand();
        command.AddressLine1 = new string('A', 201);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AddressLine1);
    }

    [Fact]
    public void Validator_RejectsAddressLine2LongerThan200()
    {
        var command = CreateValidCommand();
        command.AddressLine2 = new string('A', 201);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AddressLine2);
    }

    [Fact]
    public void Validator_RejectsPostalCodeLongerThan40()
    {
        var command = CreateValidCommand();
        command.PostalCode = new string('A', 41);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.PostalCode);
    }

    [Fact]
    public void Validator_RejectsDescriptionLongerThan2000()
    {
        var command = CreateValidCommand();
        command.Description = new string('A', 2001);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    private static UpsertMyEmployerOrganizationCommand CreateValidCommand()
    {
        return new UpsertMyEmployerOrganizationCommand { Name = "General Hospital" };
    }
}
