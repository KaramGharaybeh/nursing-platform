using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Nurses.Commands.UpsertNurseProfile;

public class UpsertNurseProfileCommandHandler : IRequestHandler<UpsertNurseProfileCommand, NurseProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public UpsertNurseProfileCommandHandler(
        IApplicationDbContext context,
        NursingPlatform.Application.Abstractions.Auth.ICurrentUserService currentUser,
        NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<NurseProfileDto> Handle(UpsertNurseProfileCommand command, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var countries = await ValidateCountriesAsync(command.LicenseCountryId, command.CurrentCountryId, cancellationToken);

        var profile = await _context.NurseProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new NurseProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };
            _context.NurseProfiles.Add(profile);
        }

        profile.Headline = command.Headline;
        profile.ProfessionalSummary = command.ProfessionalSummary;
        profile.LicenseNumber = command.LicenseNumber;
        profile.LicenseCountryId = command.LicenseCountryId;
        profile.CurrentCountryId = command.CurrentCountryId;
        profile.YearsOfExperience = command.YearsOfExperience;
        profile.IsAvailableForRecruitment = command.IsAvailableForRecruitment;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(profile, countries);
    }

    private async Task<Dictionary<Guid, Country>> ValidateCountriesAsync(
        Guid? licenseCountryId,
        Guid? currentCountryId,
        CancellationToken cancellationToken)
    {
        var requestedCountryIds = new[] { licenseCountryId, currentCountryId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (requestedCountryIds.Count == 0)
        {
            return [];
        }

        var countries = await _context.Countries
            .Where(c => requestedCountryIds.Contains(c.Id) && c.IsActive)
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        if (countries.Count != requestedCountryIds.Count)
        {
            throw new InvalidOperationException("One or more countries are invalid or inactive.");
        }

        return countries;
    }

    private static NurseProfileDto ToDto(NurseProfile profile, IReadOnlyDictionary<Guid, Country> countries)
    {
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
