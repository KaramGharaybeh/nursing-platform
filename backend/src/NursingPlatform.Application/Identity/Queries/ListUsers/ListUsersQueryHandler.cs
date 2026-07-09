using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Identity.DTOs;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Queries.ListUsers;

public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, PaginatedResult<UserListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public ListUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<UserListItemDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        IQueryable<User> query = _context.Users;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
            query = query.Where(u =>
                u.Email.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == request.Role));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortedQuery = request.Sort?.ToLowerInvariant() switch
        {
            "email" => query.OrderBy(u => u.Email),
            "-email" => query.OrderByDescending(u => u.Email),
            "firstname" => query.OrderBy(u => u.FirstName),
            "-firstname" => query.OrderByDescending(u => u.FirstName),
            "lastname" => query.OrderBy(u => u.LastName),
            "-lastname" => query.OrderByDescending(u => u.LastName),
            "createdat" => query.OrderBy(u => u.CreatedAt),
            "-createdat" => query.OrderByDescending(u => u.CreatedAt),
            "lastloginat" => query.OrderBy(u => u.LastLoginAt),
            "-lastloginat" => query.OrderByDescending(u => u.LastLoginAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                EmailVerified = u.EmailVerified,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                Roles = u.UserRoles
                    .Select(ur => ur.Role.Name)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<UserListItemDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
