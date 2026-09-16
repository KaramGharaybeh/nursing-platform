namespace NursingPlatform.Application.ReferenceData.Languages.DTOs;

public class LanguageListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
