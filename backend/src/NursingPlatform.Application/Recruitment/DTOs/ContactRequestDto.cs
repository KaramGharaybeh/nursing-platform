namespace NursingPlatform.Application.Recruitment.DTOs;

public class ContactRequestDto
{
    public Guid Id { get; init; }
    public Guid NurseProfileId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CandidateHeadline { get; init; }
    public string? CandidateLicenseCountryName { get; init; }
    public string? CandidateCurrentCountryName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? RespondedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
}
