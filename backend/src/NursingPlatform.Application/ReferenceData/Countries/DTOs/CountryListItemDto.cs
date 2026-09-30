namespace NursingPlatform.Application.ReferenceData.Countries.DTOs;

public class CountryListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
