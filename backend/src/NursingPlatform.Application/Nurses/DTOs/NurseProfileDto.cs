namespace NursingPlatform.Application.Nurses.DTOs;

public class NurseProfileDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? Headline { get; init; }
    public string? ProfessionalSummary { get; init; }
    public string? LicenseNumber { get; init; }
    public Guid? LicenseCountryId { get; init; }
    public string? LicenseCountryName { get; init; }
    public Guid? CurrentCountryId { get; init; }
    public string? CurrentCountryName { get; init; }
    public int YearsOfExperience { get; init; }
    public bool IsAvailableForRecruitment { get; init; }
}
