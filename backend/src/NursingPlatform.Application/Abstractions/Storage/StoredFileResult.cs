namespace NursingPlatform.Application.Abstractions.Storage;

public class StoredFileResult
{
    public string StorageKey { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
}
