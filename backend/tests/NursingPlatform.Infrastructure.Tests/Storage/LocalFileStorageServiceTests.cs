using Microsoft.Extensions.Options;
using NursingPlatform.Infrastructure.Configuration;
using NursingPlatform.Infrastructure.Storage;

namespace NursingPlatform.Infrastructure.Tests.Storage;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _rootPath = Path.Combine(Path.GetTempPath(), "nursing-platform-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SaveAsync_GeneratesStorageKeyWithoutClientFileName()
    {
        var service = CreateService();
        await using var content = new MemoryStream("test file"u8.ToArray());

        var result = await service.SaveAsync(content, ".pdf", "application/pdf");

        Assert.EndsWith(".pdf", result.StorageKey);
        Assert.DoesNotContain("resume", result.StorageKey, StringComparison.OrdinalIgnoreCase);
        Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(result.StorageKey), out _));
    }

    [Fact]
    public async Task SaveAsync_WritesContentToConfiguredRoot()
    {
        var service = CreateService();
        await using var content = new MemoryStream("stored content"u8.ToArray());

        var result = await service.SaveAsync(content, ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        var storedPath = Path.Combine(_rootPath, result.StorageKey);
        Assert.True(File.Exists(storedPath));
        Assert.Equal("stored content", await File.ReadAllTextAsync(storedPath));
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", result.ContentType);
        Assert.Equal("stored content"u8.ToArray().Length, result.FileSizeBytes);
    }

    [Fact]
    public async Task DeleteAsync_MissingFile_DoesNotThrow()
    {
        var service = CreateService();

        await service.DeleteAsync("missing-file.pdf");
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_RemovesFile()
    {
        var service = CreateService();
        await using var content = new MemoryStream("delete me"u8.ToArray());
        var result = await service.SaveAsync(content, ".doc", "application/msword");
        var storedPath = Path.Combine(_rootPath, result.StorageKey);

        await service.DeleteAsync(result.StorageKey);

        Assert.False(File.Exists(storedPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    private LocalFileStorageService CreateService()
    {
        return new LocalFileStorageService(Options.Create(new FileStorageSettings
        {
            RootPath = _rootPath
        }));
    }
}
