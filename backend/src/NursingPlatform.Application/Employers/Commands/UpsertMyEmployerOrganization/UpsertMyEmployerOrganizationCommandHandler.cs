using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Employers.DTOs;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;

public class UpsertMyEmployerOrganizationCommandHandler : IRequestHandler<UpsertMyEmployerOrganizationCommand, EmployerOrganizationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public UpsertMyEmployerOrganizationCommandHandler(
        IApplicationDbContext context,
        EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<EmployerOrganizationDto> Handle(UpsertMyEmployerOrganizationCommand command, CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);
        var country = await ValidateCountryAsync(command.CountryId, cancellationToken);

        var profile = await _context.EmployerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new EmployerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };
            _context.EmployerProfiles.Add(profile);
        }

        var organization = await _context.EmployerOrganizations
            .FirstOrDefaultAsync(o => o.EmployerProfileId == profile.Id, cancellationToken);

        if (organization is null)
        {
            organization = new EmployerOrganization
            {
                Id = Guid.NewGuid(),
                EmployerProfileId = profile.Id
            };
            _context.EmployerOrganizations.Add(organization);
        }

        organization.Name = command.Name.Trim();
        organization.Type = NormalizeOptional(command.Type);
        organization.WebsiteUrl = NormalizeOptional(command.WebsiteUrl);
        organization.CountryId = command.CountryId;
        organization.City = NormalizeOptional(command.City);
        organization.AddressLine1 = NormalizeOptional(command.AddressLine1);
        organization.AddressLine2 = NormalizeOptional(command.AddressLine2);
        organization.PostalCode = NormalizeOptional(command.PostalCode);
        organization.Description = NormalizeOptional(command.Description);

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(organization, country);
    }

    private async Task<Country?> ValidateCountryAsync(Guid? countryId, CancellationToken cancellationToken)
    {
        if (!countryId.HasValue)
        {
            return null;
        }

        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == countryId.Value && c.IsActive, cancellationToken);

        if (country is null)
        {
            throw new InvalidOperationException("One or more countries are invalid or inactive.");
        }

        return country;
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static EmployerOrganizationDto ToDto(EmployerOrganization organization, Country? country)
    {
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
