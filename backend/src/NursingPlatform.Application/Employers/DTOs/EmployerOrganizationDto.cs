namespace NursingPlatform.Application.Employers.DTOs;

public class EmployerOrganizationDto
{
    public Guid Id { get; init; }
    public Guid EmployerProfileId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Type { get; init; }
    public string? WebsiteUrl { get; init; }
    public Guid? CountryId { get; init; }
    public string? CountryName { get; init; }
    public string? City { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? PostalCode { get; init; }
    public string? Description { get; init; }
}
