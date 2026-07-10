namespace NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;

public abstract class UpsertNurseCertificateRequest
{
    public string Name { get; init; } = string.Empty;
    public string IssuingOrganization { get; init; } = string.Empty;
    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public string? CredentialId { get; init; }
    public string? CredentialUrl { get; init; }
}
