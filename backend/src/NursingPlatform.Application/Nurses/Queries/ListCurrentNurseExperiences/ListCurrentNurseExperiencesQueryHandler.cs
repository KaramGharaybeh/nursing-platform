using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseExperiences;

public class ListCurrentNurseExperiencesQueryHandler : IRequestHandler<ListCurrentNurseExperiencesQuery, IReadOnlyList<NurseExperienceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListCurrentNurseExperiencesQueryHandler(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<IReadOnlyList<NurseExperienceDto>> Handle(ListCurrentNurseExperiencesQuery request, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        var experiences = await _context.NurseExperiences
            .Where(e => e.NurseProfileId == profile.Id)
            .OrderByDescending(e => e.StartDate)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new NurseExperienceDto
            {
                Id = e.Id,
                FacilityName = e.FacilityName,
                JobTitle = e.JobTitle,
                CountryId = e.CountryId,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrent = e.IsCurrent,
                Description = e.Description
            })
            .ToListAsync(cancellationToken);

        var countryIds = experiences
            .Where(e => e.CountryId.HasValue)
            .Select(e => e.CountryId!.Value)
            .Distinct()
            .ToList();

        if (countryIds.Count == 0)
        {
            return experiences;
        }

        var countries = await _context.Countries
            .Where(c => countryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return experiences
            .Select(e => new NurseExperienceDto
            {
                Id = e.Id,
                FacilityName = e.FacilityName,
                JobTitle = e.JobTitle,
                CountryId = e.CountryId,
                CountryName = e.CountryId.HasValue && countries.TryGetValue(e.CountryId.Value, out var countryName)
                    ? countryName
                    : null,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrent = e.IsCurrent,
                Description = e.Description
            })
            .ToList();
    }
}
