using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Nurses;

public class NurseSkill : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public NurseProfile NurseProfile { get; set; } = null!;
}
