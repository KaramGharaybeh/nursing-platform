namespace NursingPlatform.Application.Nurses.Commands.CreateNurseExperience;

public abstract class UpsertNurseExperienceRequest
{
    public string FacilityName { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public Guid? CountryId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public bool IsCurrent { get; init; }
    public string? Description { get; init; }
}
