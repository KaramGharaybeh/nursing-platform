namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseLanguages;

public class UpdateNurseLanguageRequest
{
    public Guid LanguageId { get; init; }
    public string Proficiency { get; init; } = string.Empty;
}
