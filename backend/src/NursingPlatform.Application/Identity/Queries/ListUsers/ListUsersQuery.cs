using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Identity.DTOs;

namespace NursingPlatform.Application.Identity.Queries.ListUsers;

public class ListUsersQuery : IRequest<PaginatedResult<UserListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
    public string? Role { get; init; }
    public string? Sort { get; init; }
}
