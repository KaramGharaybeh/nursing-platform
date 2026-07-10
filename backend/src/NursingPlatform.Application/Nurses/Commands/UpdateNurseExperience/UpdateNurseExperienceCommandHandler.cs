using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseExperience;

public class UpdateNurseExperienceCommandHandler : IRequestHandler<UpdateNurseExperienceCommand, NurseExperienceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public UpdateNurseExperienceCommandHandler(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<NurseExperienceDto> Handle(UpdateNurseExperienceCommand command, CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfileAsync(cancellationToken);
        var country = await GetActiveCountryAsync(command.CountryId, cancellationToken);

        var experience = await _context.NurseExperiences
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.NurseProfileId == profile.Id, cancellationToken);

        if (experience is null)
        {
            throw new KeyNotFoundException("Nurse experience was not found.");
        }

        experience.FacilityName = command.FacilityName;
        experience.JobTitle = command.JobTitle;
        experience.CountryId = command.CountryId;
        experience.StartDate = command.StartDate;
        experience.EndDate = command.EndDate;
        experience.IsCurrent = command.IsCurrent;
        experience.Description = command.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(experience, country);
    }

    private async Task<NurseProfile> GetCurrentProfileAsync(CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        return profile;
    }

    private async Task<Country?> GetActiveCountryAsync(Guid? countryId, CancellationToken cancellationToken)
    {
        if (!countryId.HasValue)
        {
            return null;
        }

        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == countryId.Value && c.IsActive, cancellationToken);

        if (country is null)
        {
            throw new InvalidOperationException("Country is invalid or inactive.");
        }

        return country;
    }

    private static NurseExperienceDto ToDto(NurseExperience experience, Country? country)
    {
        return new NurseExperienceDto
        {
            Id = experience.Id,
            FacilityName = experience.FacilityName,
            JobTitle = experience.JobTitle,
            CountryId = experience.CountryId,
            CountryName = country?.Name,
            StartDate = experience.StartDate,
            EndDate = experience.EndDate,
            IsCurrent = experience.IsCurrent,
            Description = experience.Description
        };
    }
}
