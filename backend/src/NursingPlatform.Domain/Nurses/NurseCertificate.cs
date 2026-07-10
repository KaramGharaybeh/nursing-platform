using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Nurses;

public class NurseCertificate : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;
}
