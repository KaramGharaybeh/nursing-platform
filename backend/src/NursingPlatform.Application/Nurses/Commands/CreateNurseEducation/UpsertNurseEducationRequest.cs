namespace NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;

public abstract class UpsertNurseEducationRequest
{
    public string InstitutionName { get; init; } = string.Empty;
    public string Degree { get; init; } = string.Empty;
    public string? FieldOfStudy { get; init; }
    public Guid? CountryId { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
}
