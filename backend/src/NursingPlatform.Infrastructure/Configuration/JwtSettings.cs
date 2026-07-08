using System.ComponentModel.DataAnnotations;

namespace NursingPlatform.Infrastructure.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required(AllowEmptyStrings = false)]
    public string Secret { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationInMinutes { get; set; }

    [Range(1, 365)]
    public int RefreshTokenExpirationInDays { get; set; }
}
