namespace NursingPlatform.Application.Employers.DTOs;

public class EmployerProfileDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
}
