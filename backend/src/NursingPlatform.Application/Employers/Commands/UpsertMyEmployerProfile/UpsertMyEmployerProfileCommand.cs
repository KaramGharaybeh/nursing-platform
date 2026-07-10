using MediatR;
using NursingPlatform.Application.Employers.DTOs;

namespace NursingPlatform.Application.Employers.Commands.UpsertMyEmployerProfile;

public class UpsertMyEmployerProfileCommand : IRequest<EmployerProfileDto>
{
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
}
