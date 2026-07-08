using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Identity;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
