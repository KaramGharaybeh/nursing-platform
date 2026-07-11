using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Domain.Recruitment;

public class ContactRequest : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid EmployerProfileId { get; set; }
    public Guid EmployerOrganizationId { get; set; }
    public Guid NurseProfileId { get; set; }
    public ContactRequestStatus Status { get; set; } = ContactRequestStatus.Pending;
    public string? CandidateHeadlineSnapshot { get; set; }
    public string? CandidateLicenseCountryNameSnapshot { get; set; }
    public string? CandidateCurrentCountryNameSnapshot { get; set; }
    public string EmployerOrganizationNameSnapshot { get; set; } = string.Empty;
    public string? JobTitleSnapshot { get; set; }
    public string? DepartmentSnapshot { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public EmployerProfile EmployerProfile { get; set; } = null!;
    public EmployerOrganization EmployerOrganization { get; set; } = null!;
    public NurseProfile NurseProfile { get; set; } = null!;

    public bool IsTerminal =>
        Status is ContactRequestStatus.Approved or ContactRequestStatus.Rejected or ContactRequestStatus.Cancelled;
}
