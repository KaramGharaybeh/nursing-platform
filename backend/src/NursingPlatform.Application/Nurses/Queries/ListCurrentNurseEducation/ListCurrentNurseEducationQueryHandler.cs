using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseEducation;

public class ListCurrentNurseEducationQueryHandler : IRequestHandler<ListCurrentNurseEducationQuery, IReadOnlyList<NurseEducationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListCurrentNurseEducationQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<IReadOnlyList<NurseEducationDto>> Handle(ListCurrentNurseEducationQuery request, CancellationToken cancellationToken)
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

        var education = await _context.NurseEducation
            .Where(e => e.NurseProfileId == profile.Id)
            .OrderBy(e => e.EndDate.HasValue)
            .ThenByDescending(e => e.EndDate)
            .ThenByDescending(e => e.StartDate)
            .Select(e => new NurseEducationDto
            {
                Id = e.Id,
                InstitutionName = e.InstitutionName,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                CountryId = e.CountryId,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Description = e.Description
            })
            .ToListAsync(cancellationToken);

        var countryIds = education
            .Where(e => e.CountryId.HasValue)
            .Select(e => e.CountryId!.Value)
            .Distinct()
            .ToList();

        if (countryIds.Count == 0)
        {
            return education;
        }

        var countries = await _context.Countries
            .Where(c => countryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return education
            .Select(e => new NurseEducationDto
            {
                Id = e.Id,
                InstitutionName = e.InstitutionName,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                CountryId = e.CountryId,
                CountryName = e.CountryId.HasValue && countries.TryGetValue(e.CountryId.Value, out var countryName)
                    ? countryName
                    : null,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Description = e.Description
            })
            .ToList();
    }
}
