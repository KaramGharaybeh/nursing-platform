using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Recruitment.Common;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Queries.ListReceivedContactRequests;

public class ListReceivedContactRequestsQueryHandler : IRequestHandler<ListReceivedContactRequestsQuery, PaginatedResult<ReceivedContactRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListReceivedContactRequestsQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaginatedResult<ReceivedContactRequestDto>> Handle(ListReceivedContactRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await _context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ForbiddenAccessException("Nurse profile is required before listing received contact requests.");

        var query = _context.ContactRequests.Where(r => r.NurseProfileId == nurseProfileId);
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

        return new PaginatedResult<ReceivedContactRequestDto>
        {
            Items = items.Select(ContactRequestMapping.ToNurseDto).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
