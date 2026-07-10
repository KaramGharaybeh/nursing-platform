namespace NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;

public class UpsertMyEmployerOrganizationRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Type { get; init; }
    public string? WebsiteUrl { get; init; }
    public Guid? CountryId { get; init; }
    public string? City { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? PostalCode { get; init; }
    public string? Description { get; init; }
}
