using MediatR;

namespace NursingPlatform.Application.Identity.Commands.PublicRegister;

public class PublicRegisterCommand : IRequest<Unit>
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
