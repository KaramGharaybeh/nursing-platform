namespace NursingPlatform.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(
        Stream content,
        string extension,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
