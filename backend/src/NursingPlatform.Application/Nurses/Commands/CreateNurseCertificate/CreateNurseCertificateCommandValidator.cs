using FluentValidation;

namespace NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;

public class CreateNurseCertificateCommandValidator : AbstractValidator<CreateNurseCertificateCommand>
{
    public CreateNurseCertificateCommandValidator()
    {
        Include(new UpsertNurseCertificateRequestValidator<CreateNurseCertificateCommand>());
    }
}

public class UpsertNurseCertificateRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : UpsertNurseCertificateRequest
{
    public UpsertNurseCertificateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.IssuingOrganization)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CredentialId).MaximumLength(200);

        RuleFor(x => x.CredentialUrl)
            .MaximumLength(500)
            .Must(BeAbsoluteHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CredentialUrl));

        RuleFor(x => x.ExpirationDate)
            .GreaterThanOrEqualTo(x => x.IssueDate!.Value)
            .When(x => x.IssueDate.HasValue && x.ExpirationDate.HasValue);
    }

    private static bool BeAbsoluteHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
