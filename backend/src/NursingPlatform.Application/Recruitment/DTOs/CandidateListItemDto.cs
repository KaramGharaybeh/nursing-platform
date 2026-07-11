namespace NursingPlatform.Application.Recruitment.DTOs;

public class CandidateListItemDto
{
    public Guid NurseProfileId { get; init; }
    public string? Headline { get; init; }
    public string? ProfessionalSummary { get; init; }
    public string? LicenseCountryName { get; init; }
    public string? CurrentCountryName { get; init; }
    public int YearsOfExperience { get; init; }
    public List<string> Skills { get; init; } = [];
    public List<CandidateLanguageDto> Languages { get; init; } = [];
    public string CertificatesSummary { get; init; } = "0 certificates";
    public int CertificatesCount { get; init; }
    public string? LatestExperienceTitle { get; init; }
    public string? EducationSummary { get; init; }
}
