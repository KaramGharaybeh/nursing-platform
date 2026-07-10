using MediatR;
using NursingPlatform.Application.Employers.DTOs;

namespace NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;

public class UpsertMyEmployerOrganizationCommand : IRequest<EmployerOrganizationDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? WebsiteUrl { get; set; }
    public Guid? CountryId { get; set; }
    public string? City { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? PostalCode { get; set; }
    public string? Description { get; set; }
}
