using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Recruitment.Common;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Queries.GetMyContactRequest;

public class GetMyContactRequestQueryHandler : IRequestHandler<GetMyContactRequestQuery, ContactRequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public GetMyContactRequestQueryHandler(IApplicationDbContext context, EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<ContactRequestDto> Handle(GetMyContactRequestQuery request, CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);
        var employerProfileId = await _context.EmployerProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ForbiddenAccessException("Employer profile is required before viewing contact requests.");

        var contactRequest = await _context.ContactRequests
            .Where(r => r.Id == request.Id && r.EmployerProfileId == employerProfileId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Contact request was not found.");

        return ContactRequestMapping.ToEmployerDto(contactRequest);
    }
}
