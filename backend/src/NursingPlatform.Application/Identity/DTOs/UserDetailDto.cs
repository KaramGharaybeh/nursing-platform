namespace NursingPlatform.Application.Identity.DTOs;

public class UserDetailDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmailVerified { get; init; }
    public List<string> Roles { get; init; } = [];
    public List<string> Permissions { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
