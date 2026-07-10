using Microsoft.Extensions.Options;
using NursingPlatform.Application.Abstractions.Storage;
using NursingPlatform.Infrastructure.Configuration;

namespace NursingPlatform.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;

    public LocalFileStorageService(IOptions<FileStorageSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<StoredFileResult> SaveAsync(
        Stream content,
        string extension,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var normalizedExtension = NormalizeExtension(extension);
        var storageKey = $"{Guid.NewGuid():N}{normalizedExtension}";
        var rootPath = GetRootPath();
        Directory.CreateDirectory(rootPath);

        var filePath = Path.Combine(rootPath, storageKey);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(fileStream, cancellationToken);

        return new StoredFileResult
        {
            StorageKey = storageKey,
            ContentType = contentType,
            FileSizeBytes = fileStream.Length
        };
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            return Task.CompletedTask;

        var fileName = Path.GetFileName(storageKey);
        var filePath = Path.Combine(GetRootPath(), fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string GetRootPath()
    {
        return string.IsNullOrWhiteSpace(_settings.RootPath)
            ? Path.Combine(Path.GetTempPath(), "nursing-platform", "uploads")
            : _settings.RootPath;
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("File extension is required.", nameof(extension));

        var fileName = Path.GetFileName(extension.Trim());
        var normalizedExtension = fileName.StartsWith('.')
            ? fileName
            : $".{fileName}";

        return normalizedExtension.ToLowerInvariant();
    }
}
