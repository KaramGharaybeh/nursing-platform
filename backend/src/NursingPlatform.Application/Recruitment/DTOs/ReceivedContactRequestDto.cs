namespace NursingPlatform.Application.Recruitment.DTOs;

public class ReceivedContactRequestDto
{
    public Guid Id { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? RespondedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
}
