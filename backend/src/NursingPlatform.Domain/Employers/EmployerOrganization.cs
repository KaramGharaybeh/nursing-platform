using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Employers;

public class EmployerOrganization : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid EmployerProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? WebsiteUrl { get; set; }
    public Guid? CountryId { get; set; }
    public string? City { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? PostalCode { get; set; }
    public string? Description { get; set; }
    public EmployerProfile EmployerProfile { get; set; } = null!;
    public Country? Country { get; set; }
}
