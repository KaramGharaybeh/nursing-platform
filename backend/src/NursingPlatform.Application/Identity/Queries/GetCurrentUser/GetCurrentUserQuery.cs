using MediatR;
using NursingPlatform.Application.Identity.DTOs;

namespace NursingPlatform.Application.Identity.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<UserDetailDto>
{
}
