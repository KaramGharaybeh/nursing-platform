using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Employers.DTOs;

namespace NursingPlatform.Application.Employers.Queries.GetMyEmployerOrganization;

public class GetMyEmployerOrganizationQueryHandler : IRequestHandler<GetMyEmployerOrganizationQuery, EmployerOrganizationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public GetMyEmployerOrganizationQueryHandler(
        IApplicationDbContext context,
        EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<EmployerOrganizationDto> Handle(GetMyEmployerOrganizationQuery request, CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);

        var profile = await _context.EmployerProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Employer profile was not found.");
        }

        var organization = await _context.EmployerOrganizations
            .Where(o => o.EmployerProfileId == profile.Id)
            .Select(o => new
            {
                o.Id,
                o.EmployerProfileId,
                o.Name,
                o.Type,
                o.WebsiteUrl,
                o.CountryId,
                o.City,
                o.AddressLine1,
                o.AddressLine2,
                o.PostalCode,
                o.Description
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (organization is null)
        {
            throw new KeyNotFoundException("Employer organization was not found.");
        }

        var country = organization.CountryId.HasValue
            ? await _context.Countries.FirstOrDefaultAsync(c => c.Id == organization.CountryId.Value, cancellationToken)
            : null;

        return new EmployerOrganizationDto
        {
            Id = organization.Id,
            EmployerProfileId = organization.EmployerProfileId,
            Name = organization.Name,
            Type = organization.Type,
            WebsiteUrl = organization.WebsiteUrl,
            CountryId = organization.CountryId,
            CountryName = country?.Name,
            City = organization.City,
            AddressLine1 = organization.AddressLine1,
            AddressLine2 = organization.AddressLine2,
            PostalCode = organization.PostalCode,
            Description = organization.Description
        };
    }
}
