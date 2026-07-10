using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseEducation;

public class UpdateNurseEducationCommandHandler : IRequestHandler<UpdateNurseEducationCommand, NurseEducationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public UpdateNurseEducationCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<NurseEducationDto> Handle(UpdateNurseEducationCommand command, CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfileAsync(cancellationToken);
        var country = await GetActiveCountryAsync(command.CountryId, cancellationToken);
        var education = await _context.NurseEducation
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.NurseProfileId == profile.Id, cancellationToken);

        if (education is null)
        {
            throw new KeyNotFoundException("Nurse education was not found.");
        }

        education.InstitutionName = command.InstitutionName;
        education.Degree = command.Degree;
        education.FieldOfStudy = command.FieldOfStudy;
        education.CountryId = command.CountryId;
        education.StartDate = command.StartDate;
        education.EndDate = command.EndDate;
        education.Description = command.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(education, country);
    }

    private async Task<NurseProfile> GetCurrentProfileAsync(CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

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

        var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == countryId.Value && c.IsActive, cancellationToken);
        if (country is null)
        {
            throw new InvalidOperationException("Country is invalid or inactive.");
        }

        return country;
    }

    private static NurseEducationDto ToDto(NurseEducation education, Country? country)
    {
        return new NurseEducationDto
        {
            Id = education.Id,
            InstitutionName = education.InstitutionName,
            Degree = education.Degree,
            FieldOfStudy = education.FieldOfStudy,
            CountryId = education.CountryId,
            CountryName = country?.Name,
            StartDate = education.StartDate,
            EndDate = education.EndDate,
            Description = education.Description
        };
    }
}
