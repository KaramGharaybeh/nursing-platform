using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.GetCurrentNurseProfile;

public class GetCurrentNurseProfileQuery : IRequest<NurseProfileDto>
{
}
