using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.ReferenceData;

public class Country : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
