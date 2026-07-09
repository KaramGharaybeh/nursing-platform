using MediatR;
using NursingPlatform.Application.Identity.DTOs;

namespace NursingPlatform.Application.Identity.Queries.GetUser;

public class GetUserQuery : IRequest<UserDetailDto>
{
    public Guid UserId { get; init; }
}
