namespace NursingPlatform.Application.Nurses.Commands.UpsertNurseProfile;

public class UpsertNurseProfileRequest
{
    public string? Headline { get; init; }
    public string? ProfessionalSummary { get; init; }
    public string? LicenseNumber { get; init; }
    public Guid? LicenseCountryId { get; init; }
    public Guid? CurrentCountryId { get; init; }
    public int YearsOfExperience { get; init; }
    public bool IsAvailableForRecruitment { get; init; }
}
