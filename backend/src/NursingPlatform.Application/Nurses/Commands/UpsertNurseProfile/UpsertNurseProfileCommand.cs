using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.UpsertNurseProfile;

public class UpsertNurseProfileCommand : IRequest<NurseProfileDto>
{
    public string? Headline { get; init; }
    public string? ProfessionalSummary { get; init; }
    public string? LicenseNumber { get; init; }
    public Guid? LicenseCountryId { get; init; }
    public Guid? CurrentCountryId { get; init; }
    public int YearsOfExperience { get; init; }
    public bool IsAvailableForRecruitment { get; init; }
}
