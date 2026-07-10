using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.GetCurrentNurseProfile;

public class GetCurrentNurseProfileQueryHandler : IRequestHandler<GetCurrentNurseProfileQuery, NurseProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetCurrentNurseProfileQueryHandler(
        IApplicationDbContext context,
        NursingPlatform.Application.Abstractions.Auth.ICurrentUserService currentUser,
        NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<NurseProfileDto> Handle(GetCurrentNurseProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);

        var profile = await _context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                p.Id,
                p.UserId,
                p.Headline,
                p.ProfessionalSummary,
                p.LicenseNumber,
                p.LicenseCountryId,
                p.CurrentCountryId,
                p.YearsOfExperience,
                p.IsAvailableForRecruitment
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        var countryIds = new[] { profile.LicenseCountryId, profile.CurrentCountryId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var countries = await _context.Countries
            .Where(c => countryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return new NurseProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Headline = profile.Headline,
            ProfessionalSummary = profile.ProfessionalSummary,
            LicenseNumber = profile.LicenseNumber,
            LicenseCountryId = profile.LicenseCountryId,
            LicenseCountryName = profile.LicenseCountryId.HasValue && countries.TryGetValue(profile.LicenseCountryId.Value, out var licenseCountry)
                ? licenseCountry.Name
                : null,
            CurrentCountryId = profile.CurrentCountryId,
            CurrentCountryName = profile.CurrentCountryId.HasValue && countries.TryGetValue(profile.CurrentCountryId.Value, out var currentCountry)
                ? currentCountry.Name
                : null,
            YearsOfExperience = profile.YearsOfExperience,
            IsAvailableForRecruitment = profile.IsAvailableForRecruitment
        };
    }
}
