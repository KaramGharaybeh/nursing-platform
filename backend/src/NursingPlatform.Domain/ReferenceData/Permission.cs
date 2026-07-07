using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.ReferenceData;

public class Permission : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
