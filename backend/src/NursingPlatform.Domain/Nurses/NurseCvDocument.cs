using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Nurses;

public class NurseCvDocument : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;
}
