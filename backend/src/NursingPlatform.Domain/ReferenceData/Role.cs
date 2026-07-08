using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Domain.ReferenceData;

public class Role : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
