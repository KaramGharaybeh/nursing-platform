using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Nurses;

public class NurseLanguage : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public Guid LanguageId { get; set; }
    public string Proficiency { get; set; } = string.Empty;
    public NurseProfile NurseProfile { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
