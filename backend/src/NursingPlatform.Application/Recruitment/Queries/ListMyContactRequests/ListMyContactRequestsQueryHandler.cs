using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Recruitment.Common;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Queries.ListMyContactRequests;

public class ListMyContactRequestsQueryHandler : IRequestHandler<ListMyContactRequestsQuery, PaginatedResult<ContactRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public ListMyContactRequestsQueryHandler(IApplicationDbContext context, EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<PaginatedResult<ContactRequestDto>> Handle(ListMyContactRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);
        var employerProfileId = await _context.EmployerProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ForbiddenAccessException("Employer profile is required before listing contact requests.");

        var query = _context.ContactRequests.Where(r => r.EmployerProfileId == employerProfileId);
        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ContactRequestDto>
        {
            Items = items.Select(ContactRequestMapping.ToEmployerDto).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
