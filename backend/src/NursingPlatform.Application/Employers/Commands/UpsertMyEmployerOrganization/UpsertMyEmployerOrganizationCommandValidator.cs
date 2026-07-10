using FluentValidation;

namespace NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;

public class UpsertMyEmployerOrganizationCommandValidator : AbstractValidator<UpsertMyEmployerOrganizationCommand>
{
    public UpsertMyEmployerOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Type).MaximumLength(100);
        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500)
            .Must(BeHttpOrHttpsUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl))
            .WithMessage("WebsiteUrl must be an absolute HTTP or HTTPS URL.");
        RuleFor(x => x.City).MaximumLength(120);
        RuleFor(x => x.AddressLine1).MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.PostalCode).MaximumLength(40);
        RuleFor(x => x.Description).MaximumLength(2000);
    }

    private static bool BeHttpOrHttpsUrl(string? value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}
