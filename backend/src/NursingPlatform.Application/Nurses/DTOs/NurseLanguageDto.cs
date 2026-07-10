namespace NursingPlatform.Application.Nurses.DTOs;

public class NurseLanguageDto
{
    public Guid Id { get; init; }
    public Guid LanguageId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Proficiency { get; init; } = string.Empty;
}
