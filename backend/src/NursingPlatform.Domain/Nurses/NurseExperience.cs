using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Nurses;

public class NurseExperience : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public Guid? CountryId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;
    public Country? Country { get; set; }
}
