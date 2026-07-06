namespace NursingPlatform.Infrastructure.Configuration;

public class RedisSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = string.Empty;
}
