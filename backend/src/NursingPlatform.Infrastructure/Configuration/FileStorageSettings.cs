namespace NursingPlatform.Infrastructure.Configuration;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = Path.Combine(
        Path.GetTempPath(),
        "nursing-platform",
        "uploads");
}
