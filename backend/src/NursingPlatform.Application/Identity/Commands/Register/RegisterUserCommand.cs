using MediatR;

namespace NursingPlatform.Application.Identity.Commands.Register;

public class RegisterUserCommand : IRequest<Guid>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public List<Guid> RoleIds { get; init; } = new();
}
