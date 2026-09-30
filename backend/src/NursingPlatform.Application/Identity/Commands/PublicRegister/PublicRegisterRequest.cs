namespace NursingPlatform.Application.Identity.Commands.PublicRegister;

public class PublicRegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
