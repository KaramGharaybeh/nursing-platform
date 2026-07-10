using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Domain.Employers;

public class EmployerProfile : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public EmployerOrganization? Organization { get; set; }
    public User User { get; set; } = null!;
}
