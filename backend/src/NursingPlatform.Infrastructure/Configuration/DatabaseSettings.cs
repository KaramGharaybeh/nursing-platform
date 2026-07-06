namespace NursingPlatform.Infrastructure.Configuration;

public class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;
}
