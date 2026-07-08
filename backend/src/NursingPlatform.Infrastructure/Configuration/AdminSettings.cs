using System.ComponentModel.DataAnnotations;

namespace NursingPlatform.Infrastructure.Configuration;

public class AdminSettings
{
    public const string SectionName = "Admin";

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = "System";

    [Required]
    public string LastName { get; set; } = "Administrator";
}
