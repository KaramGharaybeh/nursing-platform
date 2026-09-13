using MediatR;

namespace NursingPlatform.Application.Identity.Commands.PublicRegister;

public class PublicRegisterCommand : IRequest<Unit>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
