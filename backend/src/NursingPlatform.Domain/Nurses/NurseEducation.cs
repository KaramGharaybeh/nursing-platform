using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Nurses;

public class NurseEducation : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public Guid? CountryId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;
    public Country? Country { get; set; }
}
