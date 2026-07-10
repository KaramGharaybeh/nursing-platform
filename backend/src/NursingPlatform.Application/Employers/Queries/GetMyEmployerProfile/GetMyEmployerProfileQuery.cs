using MediatR;
using NursingPlatform.Application.Employers.DTOs;

namespace NursingPlatform.Application.Employers.Queries.GetMyEmployerProfile;

public class GetMyEmployerProfileQuery : IRequest<EmployerProfileDto>
{
}
