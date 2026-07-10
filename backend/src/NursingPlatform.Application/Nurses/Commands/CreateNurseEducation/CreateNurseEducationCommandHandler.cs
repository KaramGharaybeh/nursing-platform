using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;

public class CreateNurseEducationCommandHandler : IRequestHandler<CreateNurseEducationCommand, NurseEducationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public CreateNurseEducationCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<NurseEducationDto> Handle(CreateNurseEducationCommand command, CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfileAsync(cancellationToken);
        var country = await GetActiveCountryAsync(command.CountryId, cancellationToken);

        var education = new NurseEducation
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profile.Id,
            InstitutionName = command.InstitutionName,
            Degree = command.Degree,
            FieldOfStudy = command.FieldOfStudy,
            CountryId = command.CountryId,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Description = command.Description
        };

        _context.NurseEducation.Add(education);
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
