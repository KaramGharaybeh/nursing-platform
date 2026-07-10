using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Nurses;

public class NurseProfile : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Headline { get; set; }
    public string? ProfessionalSummary { get; set; }
    public string? LicenseNumber { get; set; }
    public Guid? LicenseCountryId { get; set; }
    public Guid? CurrentCountryId { get; set; }
    public int YearsOfExperience { get; set; }
    public bool IsAvailableForRecruitment { get; set; }
    public User User { get; set; } = null!;
    public Country? LicenseCountry { get; set; }
    public Country? CurrentCountry { get; set; }
}
